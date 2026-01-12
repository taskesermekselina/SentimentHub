using System.Text.Json;
using SentimentHub.Web.DTOs;

namespace SentimentHub.Web.Services;

public interface IPythonApiService
{
    Task<AnalyzeResponseDto?> AnalyzeUrlAsync(string url, int limit = 50);
}

public class PythonApiService : IPythonApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonApiService> _logger;

    public PythonApiService(HttpClient httpClient, ILogger<PythonApiService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromHours(1); // Force 1 hour timeout for scraping
        _logger = logger;
    }

    public async Task<AnalyzeResponseDto?> AnalyzeUrlAsync(string url, int limit = 50)
    {
        try
        {
            var request = new AnalyzeRequestDto { Url = url, Limit = limit };
            var response = await _httpClient.PostAsJsonAsync("http://localhost:8001/analyze", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Python API Error: {response.StatusCode}, {error}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AnalyzeResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Python API");
            throw; 
        }
    }
}
