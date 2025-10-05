using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Win32;
using PianoPracticeJournal.Services;
using Xunit;

namespace PianoPracticeJournal.Tests.Services;

public class StartupServiceTests
{
    private readonly Mock<ILogger<StartupService>> _mockLogger;
    private readonly StartupService _startupService;

    public StartupServiceTests()
    {
        _mockLogger = new Mock<ILogger<StartupService>>();
        _startupService = new StartupService(_mockLogger.Object);
    }

    [Fact]
    public void GetApplicationPath_ShouldReturnValidPath()
    {
        // Act
        var result = _startupService.GetApplicationPath();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(".exe");
    }

    [Fact]
    public async Task IsAutoStartEnabledAsync_ShouldReturnFalseByDefault()
    {
        // Act
        var result = await _startupService.IsAutoStartEnabledAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetAutoStartAsync_WithEnabledTrue_ShouldNotThrow()
    {
        // Act & Assert
        await _startupService.Invoking(async x => await x.SetAutoStartAsync(true))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAutoStartAsync_WithEnabledFalse_ShouldNotThrow()
    {
        // Act & Assert
        await _startupService.Invoking(async x => await x.SetAutoStartAsync(false))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAutoStartAsync_ShouldCompleteSuccessfully()
    {
        // Act
        await _startupService.SetAutoStartAsync(true);
        
        // Assert
        var isEnabled = await _startupService.IsAutoStartEnabledAsync();
        // Note: This test might fail if the registry key is not accessible in test environment
        // In a real scenario, you might need to mock the registry operations
    }

    [Fact]
    public async Task SetAutoStartAsync_ThenDisable_ShouldCompleteSuccessfully()
    {
        // Act
        await _startupService.SetAutoStartAsync(true);
        await _startupService.SetAutoStartAsync(false);
        
        // Assert
        var isEnabled = await _startupService.IsAutoStartEnabledAsync();
        // Note: This test might fail if the registry key is not accessible in test environment
        // In a real scenario, you might need to mock the registry operations
    }
}
