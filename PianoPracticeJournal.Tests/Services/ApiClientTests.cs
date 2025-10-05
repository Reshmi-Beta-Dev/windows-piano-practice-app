using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using System.Net;
using System.Text;
using Xunit;

namespace PianoPracticeJournal.Tests.Services;

public class ApiClientTests
{
    private readonly Mock<ILogger<ApiClient>> _mockLogger;
    private readonly AppSettings _settings;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public ApiClientTests()
    {
        _mockLogger = new Mock<ILogger<ApiClient>>();
        _settings = new AppSettings
        {
            ApiEndpoint = "https://api.test.com/practice-sessions",
            ApiTimeoutSeconds = 30
        };
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task SubmitSessionAsync_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        var expectedResponse = new SessionSubmissionResponse
        {
            Success = true,
            Message = "Session recorded successfully",
            SessionId = session.Id
        };

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.SubmitSessionAsync(session);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Session recorded successfully");
        result.SessionId.Should().Be(session.Id);
    }

    [Fact]
    public async Task SubmitSessionAsync_WithErrorResponse_ShouldReturnFailure()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.SubmitSessionAsync(session);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("BadRequest");
        result.SessionId.Should().BeNull();
    }

    [Fact]
    public async Task SubmitSessionAsync_WithNetworkError_ShouldReturnFailure()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.SubmitSessionAsync(session);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Network error");
        result.SessionId.Should().BeNull();
    }

    [Fact]
    public async Task SubmitSessionAsync_WithTimeout_ShouldReturnFailure()
    {
        // Arrange
        var session = new PracticeSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddMinutes(-30),
            EndTime = DateTime.Now
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.SubmitSessionAsync(session);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("timeout");
        result.SessionId.Should().BeNull();
    }

    [Fact]
    public async Task TestConnectivityAsync_WithSuccessfulResponse_ShouldReturnTrue()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("OK", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.TestConnectivityAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectivityAsync_WithNotFoundResponse_ShouldReturnFalse()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.TestConnectivityAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectivityAsync_WithNetworkError_ShouldReturnFalse()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = await apiClient.TestConnectivityAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEndpoint_ShouldReturnCorrectEndpoint()
    {
        // Arrange
        var apiClient = new ApiClient(_httpClient, _mockLogger.Object, _settings);

        // Act
        var result = apiClient.GetEndpoint();

        // Assert
        result.Should().Be(_settings.ApiEndpoint);
    }
}
