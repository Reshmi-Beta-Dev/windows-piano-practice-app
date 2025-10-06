using System.Text.Json.Serialization;

namespace PianoPracticeJournal.Models;

/// <summary>
/// Represents a piano practice session with start and end times
/// </summary>
public class PracticeSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    
    public bool IsCompleted => EndTime.HasValue;
    
    public bool IsSynced { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastSyncAttempt { get; set; }
    
    public string? SyncError { get; set; }
    
    public int SyncAttemptCount { get; set; } = 0;
}

/// <summary>
/// Application settings configuration
/// </summary>
public class AppSettings
{
    public int MidiInactivityTimeoutSeconds { get; set; } = 60;
    
    public string ApiEndpoint { get; set; } = string.Empty;
    
    public int ApiTimeoutSeconds { get; set; } = 30;
    
    public bool AutoStartWithWindows { get; set; } = false;
    
    public string LogLevel { get; set; } = "Information";
}

/// <summary>
/// API response model for session submission
/// </summary>
public class SessionSubmissionResponse
{
    public bool Success { get; set; }
    
    public string? Message { get; set; }
    
    public Guid? SessionId { get; set; }
}
