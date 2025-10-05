using PianoPracticeJournal.Models;
using Microsoft.Extensions.Logging;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for synchronizing practice sessions with the API
/// </summary>
public class SyncService : ISyncService
{
    private readonly IApiClient _apiClient;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<SyncService> _logger;
    private readonly object _syncLock = new object();
    private bool _isSyncing = false;

    public event EventHandler<SyncEventArgs>? SyncCompleted;
    public event EventHandler<SyncErrorEventArgs>? SyncFailed;

    public SyncService(
        IApiClient apiClient,
        ISessionManager sessionManager,
        ILogger<SyncService> logger)
    {
        _apiClient = apiClient;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public async Task SyncAllUnsyncedSessionsAsync()
    {
        // Prevent multiple simultaneous sync operations
        lock (_syncLock)
        {
            if (_isSyncing)
            {
                _logger.LogWarning("Sync operation already in progress, skipping");
                return;
            }
            _isSyncing = true;
        }

        var startTime = DateTime.UtcNow;
        var syncedCount = 0;
        var failedCount = 0;

        try
        {
            _logger.LogInformation("Starting sync of all unsynced sessions");

            var unsyncedSessions = await _sessionManager.GetUnsyncedSessionsAsync();
            _logger.LogInformation("Found {Count} unsynced sessions", unsyncedSessions.Count);

            foreach (var session in unsyncedSessions)
            {
                var success = await SyncSessionAsync(session);
                if (success)
                {
                    syncedCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Sync completed: {SyncedCount} synced, {FailedCount} failed in {Duration}",
                syncedCount, failedCount, duration);

            SyncCompleted?.Invoke(this, new SyncEventArgs
            {
                SyncedCount = syncedCount,
                FailedCount = failedCount,
                Duration = duration
            });
        }
        finally
        {
            lock (_syncLock)
            {
                _isSyncing = false;
            }
        }
    }

    public async Task<bool> SyncSessionAsync(PracticeSession session)
    {
        if (session.IsSynced)
        {
            _logger.LogDebug("Session {SessionId} is already synced", session.Id);
            return true;
        }

        if (!session.IsCompleted)
        {
            _logger.LogWarning("Cannot sync incomplete session {SessionId}", session.Id);
            return false;
        }

        try
        {
            _logger.LogInformation("Syncing session {SessionId} (Duration: {Duration})",
                session.Id, session.Duration);

            var response = await _apiClient.SubmitSessionAsync(session);

            if (response.Success)
            {
                await _sessionManager.MarkSessionAsSyncedAsync(session.Id);
                _logger.LogInformation("Session {SessionId} synced successfully", session.Id);
                return true;
            }
            else
            {
                var errorMessage = response.Message ?? "Unknown error";
                await _sessionManager.UpdateSessionSyncErrorAsync(session.Id, errorMessage);
                _logger.LogWarning("Session {SessionId} sync failed: {Error}", session.Id, errorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            await _sessionManager.UpdateSessionSyncErrorAsync(session.Id, ex.Message);
            _logger.LogError(ex, "Error syncing session {SessionId}", session.Id);
            
            SyncFailed?.Invoke(this, new SyncErrorEventArgs
            {
                Session = session,
                Error = ex
            });

            return false;
        }
    }

    public async Task<SyncStatistics> GetSyncStatisticsAsync()
    {
        var allSessions = await _sessionManager.GetAllSessionsAsync();
        var unsyncedSessions = await _sessionManager.GetUnsyncedSessionsAsync();

        var syncedSessions = allSessions.Where(s => s.IsSynced).ToList();
        var failedSessions = allSessions.Where(s => s.SyncError != null && !s.IsSynced).ToList();
        var lastSyncTime = allSessions.Where(s => s.IsSynced).Max(s => s.LastSyncAttempt);

        return new SyncStatistics
        {
            TotalSessions = allSessions.Count,
            SyncedSessions = syncedSessions.Count,
            UnsyncedSessions = unsyncedSessions.Count,
            FailedSessions = failedSessions.Count,
            LastSyncTime = lastSyncTime
        };
    }
}
