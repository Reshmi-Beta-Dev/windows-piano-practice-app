using PianoPracticeJournal.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for managing practice sessions with automatic start/stop based on MIDI input
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly AppSettings _settings;
    private readonly Timer _inactivityTimer;
    private readonly string _dataFilePath;
    private readonly List<PracticeSession> _sessions = new();
    private PracticeSession? _currentSession;
    private DateTime _lastMidiSignal = DateTime.MinValue;
    private readonly object _lock = new object();

    public event EventHandler<SessionEventArgs>? SessionStarted;
    public event EventHandler<SessionEventArgs>? SessionEnded;

    public PracticeSession? CurrentSession => _currentSession;

    public SessionManager(ILogger<SessionManager> logger, AppSettings settings, string? dataFilePath = null)
    {
        _logger = logger;
        _settings = settings;
        _dataFilePath = dataFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PianoPracticeJournal",
            "sessions.json"
        );

        // Initialize timer for inactivity detection
        _inactivityTimer = new Timer(OnInactivityTimeout, null, Timeout.Infinite, Timeout.Infinite);
        
        // Ensure data directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath)!);
        
        // Load existing sessions
        LoadSessionsAsync().Wait();
        
        _logger.LogInformation("Session manager initialized with {SessionCount} existing sessions", _sessions.Count);
    }

    public async Task StartSessionAsync()
    {
        lock (_lock)
        {
            if (_currentSession != null)
            {
                _logger.LogDebug("Session already active, ignoring start request");
                return;
            }

            _currentSession = new PracticeSession
            {
                StartTime = DateTime.Now
            };

            _sessions.Add(_currentSession);
            _lastMidiSignal = DateTime.Now;
            
            // Start inactivity timer
            _inactivityTimer.Change(_settings.MidiInactivityTimeoutSeconds * 1000, Timeout.Infinite);
            
            _logger.LogInformation("Practice session started at {StartTime}", _currentSession.StartTime);
        }

        // Fire event outside lock
        SessionStarted?.Invoke(this, new SessionEventArgs { Session = _currentSession });
        
        // Save sessions
        await SaveSessionsAsync();
    }

    public async Task EndSessionAsync()
    {
        PracticeSession? sessionToEnd = null;
        
        lock (_lock)
        {
            if (_currentSession == null)
            {
                _logger.LogDebug("No active session to end");
                return;
            }

            sessionToEnd = _currentSession;
            sessionToEnd.EndTime = DateTime.Now;
            _currentSession = null;
            
            // Stop inactivity timer
            _inactivityTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            _logger.LogInformation("Practice session ended at {EndTime}. Duration: {Duration}", 
                sessionToEnd.EndTime, sessionToEnd.Duration);
        }

        // Fire event outside lock
        if (sessionToEnd != null)
        {
            SessionEnded?.Invoke(this, new SessionEventArgs { Session = sessionToEnd });
        }

        // Save sessions
        await SaveSessionsAsync();
    }

    public async Task OnMidiSignalReceivedAsync()
    {
        lock (_lock)
        {
            _lastMidiSignal = DateTime.Now;
            
            // If no current session, start one
            if (_currentSession == null)
            {
                Task.Run(async () => await StartSessionAsync());
                return;
            }
            
            // Reset inactivity timer
            _inactivityTimer.Change(_settings.MidiInactivityTimeoutSeconds * 1000, Timeout.Infinite);
        }

        await Task.CompletedTask;
    }

    private void OnInactivityTimeout(object? state)
    {
        _logger.LogInformation("MIDI inactivity timeout reached, ending session");
        Task.Run(async () => await EndSessionAsync());
    }

    public async Task<List<PracticeSession>> GetAllSessionsAsync()
    {
        await LoadSessionsAsync();
        return new List<PracticeSession>(_sessions);
    }

    public async Task<List<PracticeSession>> GetUnsyncedSessionsAsync()
    {
        await LoadSessionsAsync();
        return _sessions.Where(s => !s.IsSynced && s.IsCompleted).ToList();
    }

    public async Task MarkSessionAsSyncedAsync(Guid sessionId)
    {
        lock (_lock)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.IsSynced = true;
                session.SyncError = null;
                _logger.LogInformation("Session {SessionId} marked as synced", sessionId);
            }
        }

        await SaveSessionsAsync();
    }

    public async Task UpdateSessionSyncErrorAsync(Guid sessionId, string error)
    {
        lock (_lock)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                session.SyncError = error;
                session.LastSyncAttempt = DateTime.UtcNow;
                session.SyncAttemptCount++;
                _logger.LogWarning("Session {SessionId} sync error: {Error}", sessionId, error);
            }
        }

        await SaveSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var sessions = JsonSerializer.Deserialize<List<PracticeSession>>(json);
            if (sessions != null)
            {
                lock (_lock)
                {
                    _sessions.Clear();
                    _sessions.AddRange(sessions);
                    
                    // Clear any incomplete sessions from previous runs
                    var incompleteSessions = _sessions.Where(s => !s.IsCompleted).ToList();
                    foreach (var incompleteSession in incompleteSessions)
                    {
                        _sessions.Remove(incompleteSession);
                        _logger.LogInformation("Removed incomplete session from previous run: {SessionId}", 
                            incompleteSession.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sessions from file");
        }
    }

    private async Task SaveSessionsAsync()
    {
        try
        {
            List<PracticeSession> sessionsToSave;
            lock (_lock)
            {
                sessionsToSave = new List<PracticeSession>(_sessions);
            }

            var json = JsonSerializer.Serialize(sessionsToSave, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save sessions to file");
        }
    }
}
