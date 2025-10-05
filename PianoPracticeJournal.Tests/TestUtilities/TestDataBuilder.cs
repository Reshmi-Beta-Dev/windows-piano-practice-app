using PianoPracticeJournal.Models;

namespace PianoPracticeJournal.Tests.TestUtilities;

public static class TestDataBuilder
{
    public static PracticeSession CreatePracticeSession(
        Guid? id = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        bool isSynced = false,
        string? syncError = null,
        int syncAttemptCount = 0)
    {
        return new PracticeSession
        {
            Id = id ?? Guid.NewGuid(),
            StartTime = startTime ?? DateTime.Now.AddMinutes(-30),
            EndTime = endTime,
            IsSynced = isSynced,
            SyncError = syncError,
            SyncAttemptCount = syncAttemptCount,
            CreatedAt = DateTime.UtcNow,
            LastSyncAttempt = syncError != null ? DateTime.UtcNow : null
        };
    }

    public static PracticeSession CreateCompletedSession(
        Guid? id = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var start = startTime ?? DateTime.Now.AddMinutes(-30);
        var end = endTime ?? DateTime.Now;
        
        return CreatePracticeSession(id, start, end);
    }

    public static PracticeSession CreateIncompleteSession(
        Guid? id = null,
        DateTime? startTime = null)
    {
        return CreatePracticeSession(id, startTime, null);
    }

    public static PracticeSession CreateSyncedSession(
        Guid? id = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var start = startTime ?? DateTime.Now.AddMinutes(-30);
        var end = endTime ?? DateTime.Now;
        
        return CreatePracticeSession(id, start, end, isSynced: true);
    }

    public static PracticeSession CreateFailedSession(
        Guid? id = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? errorMessage = "Test error",
        int syncAttemptCount = 1)
    {
        var start = startTime ?? DateTime.Now.AddMinutes(-30);
        var end = endTime ?? DateTime.Now;
        
        return CreatePracticeSession(id, start, end, false, errorMessage, syncAttemptCount);
    }

    public static List<PracticeSession> CreateMultipleSessions(int count, bool allCompleted = true)
    {
        var sessions = new List<PracticeSession>();
        var baseTime = DateTime.Now.AddHours(-count);

        for (int i = 0; i < count; i++)
        {
            var startTime = baseTime.AddMinutes(i * 30);
            var endTime = allCompleted ? startTime.AddMinutes(25) : (DateTime?)null;
            
            sessions.Add(CreatePracticeSession(startTime: startTime, endTime: endTime));
        }

        return sessions;
    }

    public static AppSettings CreateTestAppSettings(
        int midiTimeout = 60,
        string apiEndpoint = "https://api.test.com/practice-sessions",
        int apiTimeout = 30,
        bool autoStart = false,
        bool minimizeToTray = false)
    {
        return new AppSettings
        {
            MidiInactivityTimeoutSeconds = midiTimeout,
            ApiEndpoint = apiEndpoint,
            ApiTimeoutSeconds = apiTimeout,
            AutoStartWithWindows = autoStart,
            MinimizeToTray = minimizeToTray,
            LogLevel = "Information"
        };
    }
}
