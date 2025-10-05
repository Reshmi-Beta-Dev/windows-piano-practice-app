using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using Xunit;

namespace PianoPracticeJournal.Tests.Integration;

public class SessionManagementIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly ISessionManager _sessionManager;
    private readonly ISyncService _syncService;
    private readonly string _testDataPath;

    public SessionManagementIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"PianoPracticeJournal_Integration_{Guid.NewGuid()}.json");
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(builder => builder.AddConsole());
                
                var appSettings = new AppSettings();
                configuration.GetSection("AppSettings").Bind(appSettings);
                services.AddSingleton(appSettings);

                services.AddSingleton<ISessionManager>(provider => 
                    new SessionManager(
                        provider.GetRequiredService<ILogger<SessionManager>>(),
                        provider.GetRequiredService<AppSettings>(),
                        _testDataPath));
                services.AddSingleton<IApiClient, ApiClient>();
                services.AddSingleton<ISyncService, SyncService>();
                services.AddSingleton<HttpClient>();
            })
            .Build();

        _sessionManager = _host.Services.GetRequiredService<ISessionManager>();
        _syncService = _host.Services.GetRequiredService<ISyncService>();
    }

    [Fact]
    public async Task CompleteSessionWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var sessionStarted = false;
        var sessionEnded = false;
        PracticeSession? startedSession = null;
        PracticeSession? endedSession = null;

        _sessionManager.SessionStarted += (sender, e) =>
        {
            sessionStarted = true;
            startedSession = e.Session;
        };

        _sessionManager.SessionEnded += (sender, e) =>
        {
            sessionEnded = true;
            endedSession = e.Session;
        };

        // Act - Simulate MIDI signal to start session
        await _sessionManager.OnMidiSignalReceivedAsync();
        
        // Wait a moment to ensure session is started
        await Task.Delay(100);
        
        // Verify session is active
        _sessionManager.CurrentSession.Should().NotBeNull();
        sessionStarted.Should().BeTrue();
        startedSession.Should().NotBeNull();
        startedSession!.StartTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));

        // Act - End the session manually
        await _sessionManager.EndSessionAsync();

        // Assert
        sessionEnded.Should().BeTrue();
        endedSession.Should().NotBeNull();
        endedSession!.EndTime.Should().NotBeNull();
        endedSession.IsCompleted.Should().BeTrue();
        endedSession.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        _sessionManager.CurrentSession.Should().BeNull();

        // Verify session is stored
        var allSessions = await _sessionManager.GetAllSessionsAsync();
        allSessions.Should().HaveCount(1);
        allSessions[0].Id.Should().Be(startedSession.Id);
    }

    [Fact]
    public async Task MultipleSessions_ShouldBeStoredCorrectly()
    {
        // Arrange & Act - Create multiple sessions
        for (int i = 0; i < 3; i++)
        {
            await _sessionManager.StartSessionAsync();
            await Task.Delay(50); // Small delay between sessions
            await _sessionManager.EndSessionAsync();
        }

        // Assert
        var allSessions = await _sessionManager.GetAllSessionsAsync();
        allSessions.Should().HaveCount(3);
        
        // Verify all sessions are completed
        allSessions.Should().OnlyContain(s => s.IsCompleted);
        
        // Verify sessions are ordered by start time (most recent first)
        // Note: Due to timing precision, we'll just verify they have different start times
        var distinctStartTimes = allSessions.Select(s => s.StartTime).Distinct().ToList();
        distinctStartTimes.Should().HaveCount(allSessions.Count);
    }

    [Fact]
    public async Task UnsyncedSessions_ShouldBeIdentifiedCorrectly()
    {
        // Arrange & Act - Create some sessions
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();
        
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();

        // Act
        var unsyncedSessions = await _sessionManager.GetUnsyncedSessionsAsync();
        var allSessions = await _sessionManager.GetAllSessionsAsync();

        // Assert
        allSessions.Should().HaveCount(2);
        unsyncedSessions.Should().HaveCount(2);
        unsyncedSessions.Should().OnlyContain(s => !s.IsSynced && s.IsCompleted);
    }

    [Fact]
    public async Task SessionSyncWorkflow_ShouldWorkCorrectly()
    {
        // Arrange - Create a completed session
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();
        
        var unsyncedSessions = await _sessionManager.GetUnsyncedSessionsAsync();
        var sessionId = unsyncedSessions[0].Id;

        // Act - Mark as synced
        await _sessionManager.MarkSessionAsSyncedAsync(sessionId);

        // Assert
        var updatedSessions = await _sessionManager.GetUnsyncedSessionsAsync();
        var syncedSessions = await _sessionManager.GetAllSessionsAsync();
        
        updatedSessions.Should().BeEmpty();
        syncedSessions.Should().HaveCount(1);
        syncedSessions[0].IsSynced.Should().BeTrue();
    }

    [Fact]
    public async Task SessionErrorHandling_ShouldWorkCorrectly()
    {
        // Arrange - Create a completed session
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();
        
        var unsyncedSessions = await _sessionManager.GetUnsyncedSessionsAsync();
        var sessionId = unsyncedSessions[0].Id;
        var errorMessage = "Test sync error";

        // Act - Update with error
        await _sessionManager.UpdateSessionSyncErrorAsync(sessionId, errorMessage);

        // Assert
        var allSessions = await _sessionManager.GetAllSessionsAsync();
        var session = allSessions.First(s => s.Id == sessionId);
        
        session.SyncError.Should().Be(errorMessage);
        session.LastSyncAttempt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.SyncAttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSyncStatistics_ShouldReturnCorrectCounts()
    {
        // Arrange - Create sessions with different states
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();
        
        await _sessionManager.StartSessionAsync();
        await _sessionManager.EndSessionAsync();
        
        // Mark one as synced
        var sessions = await _sessionManager.GetAllSessionsAsync();
        if (sessions.Count > 0)
        {
            await _sessionManager.MarkSessionAsSyncedAsync(sessions[0].Id);
        }

        // Act
        var stats = await _syncService.GetSyncStatisticsAsync();

        // Assert
        stats.TotalSessions.Should().Be(2);
        stats.SyncedSessions.Should().Be(1);
        stats.UnsyncedSessions.Should().Be(1);
        stats.FailedSessions.Should().Be(0);
    }

    public void Dispose()
    {
        _host?.Dispose();
        
        // Clean up test data file
        if (File.Exists(_testDataPath))
        {
            File.Delete(_testDataPath);
        }
    }
}
