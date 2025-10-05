using FluentAssertions;
using PianoPracticeJournal.Models;
using Xunit;

namespace PianoPracticeJournal.Tests.Models;

public class PracticeSessionTests
{
    [Fact]
    public void PracticeSession_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var session = new PracticeSession();

        // Assert
        session.Id.Should().NotBeEmpty();
        session.StartTime.Should().Be(default(DateTime));
        session.EndTime.Should().BeNull();
        session.IsCompleted.Should().BeFalse();
        session.IsSynced.Should().BeFalse();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.LastSyncAttempt.Should().BeNull();
        session.SyncError.Should().BeNull();
        session.SyncAttemptCount.Should().Be(0);
    }

    [Fact]
    public void Duration_WhenEndTimeIsNull_ShouldReturnZero()
    {
        // Arrange
        var session = new PracticeSession
        {
            StartTime = DateTime.Now.AddMinutes(-30)
        };

        // Act & Assert
        session.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Duration_WhenEndTimeIsSet_ShouldReturnCorrectDuration()
    {
        // Arrange
        var startTime = DateTime.Now.AddMinutes(-30);
        var endTime = DateTime.Now;
        var session = new PracticeSession
        {
            StartTime = startTime,
            EndTime = endTime
        };

        // Act & Assert
        session.Duration.Should().Be(endTime - startTime);
    }

    [Fact]
    public void IsCompleted_WhenEndTimeIsNull_ShouldReturnFalse()
    {
        // Arrange
        var session = new PracticeSession
        {
            StartTime = DateTime.Now.AddMinutes(-30)
            // EndTime is null by default
        };

        // Act & Assert
        session.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void IsCompleted_WhenEndTimeIsSet_ShouldReturnTrue()
    {
        // Arrange
        var session = new PracticeSession
        {
            EndTime = DateTime.Now
        };

        // Act & Assert
        session.IsCompleted.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void SyncAttemptCount_ShouldBeSetCorrectly(int expectedCount)
    {
        // Arrange
        var session = new PracticeSession
        {
            SyncAttemptCount = expectedCount
        };

        // Act & Assert
        session.SyncAttemptCount.Should().Be(expectedCount);
    }
}
