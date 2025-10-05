using PianoPracticeJournal.Models;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service interface for managing practice sessions
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Fired when a new session starts
    /// </summary>
    event EventHandler<SessionEventArgs>? SessionStarted;
    
    /// <summary>
    /// Fired when a session ends
    /// </summary>
    event EventHandler<SessionEventArgs>? SessionEnded;
    
    /// <summary>
    /// Gets the current active session
    /// </summary>
    PracticeSession? CurrentSession { get; }
    
    /// <summary>
    /// Gets all stored sessions
    /// </summary>
    Task<List<PracticeSession>> GetAllSessionsAsync();
    
    /// <summary>
    /// Gets sessions that need to be synced
    /// </summary>
    Task<List<PracticeSession>> GetUnsyncedSessionsAsync();
    
    /// <summary>
    /// Starts a new practice session
    /// </summary>
    Task StartSessionAsync();
    
    /// <summary>
    /// Ends the current session
    /// </summary>
    Task EndSessionAsync();
    
    /// <summary>
    /// Handles MIDI signal to manage session state
    /// </summary>
    Task OnMidiSignalReceivedAsync();
    
    /// <summary>
    /// Marks a session as synced
    /// </summary>
    Task MarkSessionAsSyncedAsync(Guid sessionId);
    
    /// <summary>
    /// Updates session sync error information
    /// </summary>
    Task UpdateSessionSyncErrorAsync(Guid sessionId, string error);
}

/// <summary>
/// Event arguments for session events
/// </summary>
public class SessionEventArgs : EventArgs
{
    public PracticeSession Session { get; set; } = null!;
}
