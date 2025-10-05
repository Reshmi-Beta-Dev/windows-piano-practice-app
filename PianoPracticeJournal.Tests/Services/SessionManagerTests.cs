using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using Xunit;

namespace PianoPracticeJournal.Tests.Services;

public class SessionManagerTests : IDisposable
{
    private readonly Mock<ILogger<SessionManager>> _mockLogger;
    private readonly AppSettings _settings;
    private readonly string _testDataPath;

    public SessionManagerTests()
    {
        _mockLogger = new Mock<ILogger<SessionManager>>();
        _settings = new AppSettings
        {
            MidiInactivityTimeoutSeconds = 60
        };
        _testDataPath = Path.Combine(Path.GetTempPath(), $"PianoPracticeJournal_Test_{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task StartSessionAsync_WhenNoCurrentSession_ShouldCreateNewSession()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        PracticeSession? startedSession = null;
        sessionManager.SessionStarted += (sender, e) => startedSession = e.Session;

        // Act
        await sessionManager.StartSessionAsync();

        // Assert
        sessionManager.CurrentSession.Should().NotBeNull();
        sessionManager.CurrentSession!.StartTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        startedSession.Should().NotBeNull();
        startedSession.Should().Be(sessionManager.CurrentSession);
    }

    [Fact]
    public async Task StartSessionAsync_WhenSessionAlreadyActive_ShouldNotCreateNewSession()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        var currentSession = sessionManager.CurrentSession;
        var sessionStartedCount = 0;
        sessionManager.SessionStarted += (sender, e) => sessionStartedCount++;

        // Act
        await sessionManager.StartSessionAsync();

        // Assert
        sessionManager.CurrentSession.Should().Be(currentSession);
        sessionStartedCount.Should().Be(0);
    }

    [Fact]
    public async Task EndSessionAsync_WhenNoCurrentSession_ShouldDoNothing()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        var sessionEndedCount = 0;
        sessionManager.SessionEnded += (sender, e) => sessionEndedCount++;

        // Act
        await sessionManager.EndSessionAsync();

        // Assert
        sessionManager.CurrentSession.Should().BeNull();
        sessionEndedCount.Should().Be(0);
    }

    [Fact]
    public async Task EndSessionAsync_WhenSessionActive_ShouldEndSession()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        var currentSession = sessionManager.CurrentSession;
        PracticeSession? endedSession = null;
        sessionManager.SessionEnded += (sender, e) => endedSession = e.Session;

        // Act
        await sessionManager.EndSessionAsync();

        // Assert
        sessionManager.CurrentSession.Should().BeNull();
        currentSession!.EndTime.Should().NotBeNull();
        currentSession.EndTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        endedSession.Should().Be(currentSession);
    }

    [Fact]
    public async Task OnMidiSignalReceivedAsync_WhenNoCurrentSession_ShouldStartNewSession()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        var sessionStartedCount = 0;
        sessionManager.SessionStarted += (sender, e) => sessionStartedCount++;

        // Act
        await sessionManager.OnMidiSignalReceivedAsync();
        
        // Wait for async operation to complete
        await Task.Delay(50);

        // Assert
        sessionManager.CurrentSession.Should().NotBeNull();
        sessionStartedCount.Should().Be(1);
    }

    [Fact]
    public async Task OnMidiSignalReceivedAsync_WhenSessionActive_ShouldNotStartNewSession()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        var currentSession = sessionManager.CurrentSession;
        var sessionStartedCount = 0;
        sessionManager.SessionStarted += (sender, e) => sessionStartedCount++;

        // Act
        await sessionManager.OnMidiSignalReceivedAsync();

        // Assert
        sessionManager.CurrentSession.Should().Be(currentSession);
        sessionStartedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllSessionsAsync_ShouldReturnAllSessions()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        await sessionManager.EndSessionAsync();

        // Act
        var sessions = await sessionManager.GetAllSessionsAsync();

        // Assert
        sessions.Should().HaveCount(1);
        sessions[0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetUnsyncedSessionsAsync_ShouldReturnOnlyUnsyncedCompletedSessions()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        await sessionManager.EndSessionAsync();

        // Act
        var unsyncedSessions = await sessionManager.GetUnsyncedSessionsAsync();

        // Assert
        unsyncedSessions.Should().HaveCount(1);
        unsyncedSessions[0].IsSynced.Should().BeFalse();
        unsyncedSessions[0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task MarkSessionAsSyncedAsync_ShouldMarkSessionAsSynced()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        await sessionManager.EndSessionAsync();
        var sessionsBefore = await sessionManager.GetAllSessionsAsync();
        var sessionId = sessionsBefore[0].Id;

        // Act
        await sessionManager.MarkSessionAsSyncedAsync(sessionId);
        var sessions = await sessionManager.GetAllSessionsAsync();

        // Assert
        var session = sessions.First(s => s.Id == sessionId);
        session.IsSynced.Should().BeTrue();
        session.SyncError.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSessionSyncErrorAsync_ShouldUpdateSessionError()
    {
        // Arrange
        var sessionManager = new SessionManager(_mockLogger.Object, _settings, _testDataPath);
        await sessionManager.StartSessionAsync();
        await sessionManager.EndSessionAsync();
        var sessionsBefore = await sessionManager.GetAllSessionsAsync();
        var sessionId = sessionsBefore[0].Id;
        var errorMessage = "Test error";

        // Act
        await sessionManager.UpdateSessionSyncErrorAsync(sessionId, errorMessage);
        var sessions = await sessionManager.GetAllSessionsAsync();

        // Assert
        var session = sessions.First(s => s.Id == sessionId);
        session.SyncError.Should().Be(errorMessage);
        session.LastSyncAttempt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.SyncAttemptCount.Should().Be(1);
    }

    public void Dispose()
    {
        // Clean up test data file
        if (File.Exists(_testDataPath))
        {
            File.Delete(_testDataPath);
        }
    }
}
