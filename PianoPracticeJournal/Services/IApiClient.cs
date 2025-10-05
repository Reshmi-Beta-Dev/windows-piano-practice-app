using PianoPracticeJournal.Models;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Interface for API client to sync practice sessions
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Submits a practice session to the API
    /// </summary>
    Task<SessionSubmissionResponse> SubmitSessionAsync(PracticeSession session);
    
    /// <summary>
    /// Tests the API connectivity
    /// </summary>
    Task<bool> TestConnectivityAsync();
    
    /// <summary>
    /// Gets the API endpoint URL
    /// </summary>
    string GetEndpoint();
}

/// <summary>
/// Interface for sync service that handles session synchronization
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Fired when sync operation completes
    /// </summary>
    event EventHandler<SyncEventArgs>? SyncCompleted;
    
    /// <summary>
    /// Fired when sync operation fails
    /// </summary>
    event EventHandler<SyncErrorEventArgs>? SyncFailed;
    
    /// <summary>
    /// Syncs all unsynced sessions
    /// </summary>
    Task SyncAllUnsyncedSessionsAsync();
    
    /// <summary>
    /// Syncs a specific session
    /// </summary>
    Task<bool> SyncSessionAsync(PracticeSession session);
    
    /// <summary>
    /// Gets sync statistics
    /// </summary>
    Task<SyncStatistics> GetSyncStatisticsAsync();
}

/// <summary>
/// Event arguments for sync completion
/// </summary>
public class SyncEventArgs : EventArgs
{
    public int SyncedCount { get; set; }
    public int FailedCount { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Event arguments for sync errors
/// </summary>
public class SyncErrorEventArgs : EventArgs
{
    public PracticeSession Session { get; set; } = null!;
    public Exception Error { get; set; } = null!;
}

/// <summary>
/// Sync statistics
/// </summary>
public class SyncStatistics
{
    public int TotalSessions { get; set; }
    public int SyncedSessions { get; set; }
    public int UnsyncedSessions { get; set; }
    public int FailedSessions { get; set; }
    public DateTime? LastSyncTime { get; set; }
}
