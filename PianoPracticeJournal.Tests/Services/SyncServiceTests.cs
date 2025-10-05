using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using Xunit;

namespace PianoPracticeJournal.Tests.Services;

public class SyncServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly Mock<ILogger<SyncService>> _mockLogger;
    private readonly SyncService _syncService;

    public SyncServiceTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _mockSessionManager = new Mock<ISessionManager>();
        _mockLogger = new Mock<ILogger<SyncService>>();
        _syncService = new SyncService(_mockApiClient.Object, _mockSessionManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SyncSessionAsync_WithSuccessfulApiResponse_ShouldMarkSessionAsSynced()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        var apiResponse = new SessionSubmissionResponse
        {
            Success = true,
            Message = "Success",
            SessionId = session.Id
        };

        _mockApiClient.Setup(x => x.SubmitSessionAsync(session))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _syncService.SyncSessionAsync(session);

        // Assert
        result.Should().BeTrue();
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(session.Id), Times.Once);
        _mockSessionManager.Verify(x => x.UpdateSessionSyncErrorAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncSessionAsync_WithFailedApiResponse_ShouldUpdateSessionError()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        var apiResponse = new SessionSubmissionResponse
        {
            Success = false,
            Message = "API Error",
            SessionId = session.Id
        };

        _mockApiClient.Setup(x => x.SubmitSessionAsync(session))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _syncService.SyncSessionAsync(session);

        // Assert
        result.Should().BeFalse();
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Never);
        _mockSessionManager.Verify(x => x.UpdateSessionSyncErrorAsync(session.Id, "API Error"), Times.Once);
    }

    [Fact]
    public async Task SyncSessionAsync_WithApiException_ShouldUpdateSessionError()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        var exception = new HttpRequestException("Network error");
        _mockApiClient.Setup(x => x.SubmitSessionAsync(session))
            .ThrowsAsync(exception);

        // Act
        var result = await _syncService.SyncSessionAsync(session);

        // Assert
        result.Should().BeFalse();
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Never);
        _mockSessionManager.Verify(x => x.UpdateSessionSyncErrorAsync(session.Id, "Network error"), Times.Once);
    }

    [Fact]
    public async Task SyncSessionAsync_WithAlreadySyncedSession_ShouldReturnTrue()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now,
            IsSynced = true
        };

        // Act
        var result = await _syncService.SyncSessionAsync(session);

        // Assert
        result.Should().BeTrue();
        _mockApiClient.Verify(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()), Times.Never);
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Never);
        _mockSessionManager.Verify(x => x.UpdateSessionSyncErrorAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SyncSessionAsync_WithIncompleteSession_ShouldReturnFalse()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30)
            // EndTime is null by default, making IsCompleted false
        };

        // Act
        var result = await _syncService.SyncSessionAsync(session);

        // Assert
        result.Should().BeFalse();
        _mockApiClient.Verify(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()), Times.Never);
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Never);
        _mockSessionManager.Verify(x => x.UpdateSessionSyncErrorAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSyncStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var allSessions = new List<PracticeSession>
        {
            new() { Id = Guid.NewGuid(), IsSynced = true, SyncError = null, EndTime = DateTime.Now },
            new() { Id = Guid.NewGuid(), IsSynced = false, SyncError = null, EndTime = DateTime.Now },
            new() { Id = Guid.NewGuid(), IsSynced = false, SyncError = "Error", EndTime = DateTime.Now }
        };

        var unsyncedSessions = allSessions.Where(s => !s.IsSynced && s.IsCompleted).ToList();

        _mockSessionManager.Setup(x => x.GetAllSessionsAsync())
            .ReturnsAsync(allSessions);
        _mockSessionManager.Setup(x => x.GetUnsyncedSessionsAsync())
            .ReturnsAsync(unsyncedSessions);

        // Act
        var result = await _syncService.GetSyncStatisticsAsync();

        // Assert
        result.TotalSessions.Should().Be(3);
        result.SyncedSessions.Should().Be(1);
        result.UnsyncedSessions.Should().Be(2);
        result.FailedSessions.Should().Be(1);
    }

    [Fact]
    public async Task SyncAllUnsyncedSessionsAsync_ShouldSyncAllUnsyncedSessions()
    {
        // Arrange
        var sessions = new List<PracticeSession>
        {
            new() { Id = Guid.NewGuid(), EndTime = DateTime.Now, IsSynced = false },
            new() { Id = Guid.NewGuid(), EndTime = DateTime.Now, IsSynced = false }
        };

        _mockSessionManager.Setup(x => x.GetUnsyncedSessionsAsync())
            .ReturnsAsync(sessions);

        var apiResponse = new SessionSubmissionResponse { Success = true };
        _mockApiClient.Setup(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()))
            .ReturnsAsync(apiResponse);

        SyncEventArgs? syncEventArgs = null;
        _syncService.SyncCompleted += (sender, e) => syncEventArgs = e;

        // Act
        await _syncService.SyncAllUnsyncedSessionsAsync();

        // Assert
        _mockApiClient.Verify(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()), Times.Exactly(2));
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Exactly(2));
        syncEventArgs.Should().NotBeNull();
        syncEventArgs!.SyncedCount.Should().Be(2);
        syncEventArgs.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task SyncAllUnsyncedSessionsAsync_WhenAlreadySyncing_ShouldSkip()
    {
        // Arrange
        var sessions = new List<PracticeSession>
        {
            new() { Id = Guid.NewGuid(), EndTime = DateTime.Now, IsSynced = false }
        };

        _mockSessionManager.Setup(x => x.GetUnsyncedSessionsAsync())
            .ReturnsAsync(sessions);

        var tcs = new TaskCompletionSource<SessionSubmissionResponse>();
        _mockApiClient.Setup(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()))
            .Returns(tcs.Task);

        // Act - Start first sync
        var firstSync = _syncService.SyncAllUnsyncedSessionsAsync();
        
        // Start second sync while first is still running
        var secondSync = _syncService.SyncAllUnsyncedSessionsAsync();

        // Complete the first sync
        tcs.SetResult(new SessionSubmissionResponse { Success = true });
        await firstSync;
        await secondSync;

        // Assert
        _mockApiClient.Verify(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()), Times.Once);
        _mockSessionManager.Verify(x => x.MarkSessionAsSyncedAsync(It.IsAny<Guid>()), Times.Once);
    }
}
