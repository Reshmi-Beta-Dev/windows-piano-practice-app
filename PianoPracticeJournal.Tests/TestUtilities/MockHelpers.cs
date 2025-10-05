using Microsoft.Extensions.Logging;
using Moq;
using PianoPracticeJournal.Models;

namespace PianoPracticeJournal.Tests.TestUtilities;

public static class MockHelpers
{
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        var mockLogger = new Mock<ILogger<T>>();
        
        // Setup the logger to accept any log level and message
        mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        return mockLogger;
    }

    public static Mock<ILogger<T>> CreateMockLoggerWithVerification<T>(
        LogLevel expectedLevel,
        string expectedMessage)
    {
        var mockLogger = new Mock<ILogger<T>>();
        
        mockLogger.Setup(x => x.Log(
            expectedLevel,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        return mockLogger;
    }

    public static void VerifyLogCalled<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel expectedLevel,
        string expectedMessage,
        Times times)
    {
        mockLogger.Verify(x => x.Log(
            expectedLevel,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
    }

    public static void VerifyLogCalled<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel expectedLevel,
        Times times)
    {
        mockLogger.Verify(x => x.Log(
            expectedLevel,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
    }

    public static void VerifyAnyLogCalled<T>(Mock<ILogger<T>> mockLogger)
    {
        mockLogger.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }
}
