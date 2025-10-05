using PianoPracticeJournal.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace PianoPracticeJournal.Services;

/// <summary>
/// API client for submitting practice sessions to web API
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly AppSettings _settings;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, AppSettings settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings;
        
        // Configure HTTP client
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.ApiTimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PianoPracticeJournal/1.0");
    }

    public async Task<SessionSubmissionResponse> SubmitSessionAsync(PracticeSession session)
    {
        try
        {
            _logger.LogInformation("Submitting session {SessionId} to API", session.Id);

            // Create request payload
            var payload = new
            {
                sessionId = session.Id,
                startTime = session.StartTime,
                endTime = session.EndTime,
                duration = session.Duration.TotalSeconds,
                submittedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Make API call
            var response = await _httpClient.PostAsync(_settings.ApiEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<SessionSubmissionResponse>(responseContent);
                
                _logger.LogInformation("Session {SessionId} submitted successfully", session.Id);
                return apiResponse ?? new SessionSubmissionResponse { Success = true };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API returned error {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                
                return new SessionSubmissionResponse
                {
                    Success = false,
                    Message = $"HTTP {response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error submitting session {SessionId}", session.Id);
            return new SessionSubmissionResponse
            {
                Success = false,
                Message = $"Network error: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout submitting session {SessionId}", session.Id);
            return new SessionSubmissionResponse
            {
                Success = false,
                Message = "Request timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error submitting session {SessionId}", session.Id);
            return new SessionSubmissionResponse
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}"
            };
        }
    }

    public async Task<bool> TestConnectivityAsync()
    {
        try
        {
            _logger.LogInformation("Testing API connectivity to {Endpoint}", _settings.ApiEndpoint);

            // Create a test request (you might want to implement a health check endpoint)
            var testPayload = new
            {
                test = true,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Try to make a test request
            var response = await _httpClient.PostAsync(_settings.ApiEndpoint, content);
            
            // For now, consider any response (even error) as connectivity success
            // In a real implementation, you'd check for a specific health endpoint
            var isConnected = response.StatusCode != System.Net.HttpStatusCode.NotFound;
            
            _logger.LogInformation("API connectivity test result: {IsConnected}", isConnected);
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API connectivity test failed");
            return false;
        }
    }

    public string GetEndpoint()
    {
        return _settings.ApiEndpoint;
    }
}
