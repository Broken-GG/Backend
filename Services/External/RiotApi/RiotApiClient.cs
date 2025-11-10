using System;
using System.Net.Http;
using System.Threading.Tasks;
using Backend.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.External.RiotApi
{
    /// <summary>
    /// Base client for Riot API operations providing HTTP client, authentication, and configuration
    /// </summary>
    public class RiotApiClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly string? _apiKey;
        protected readonly string _accountApiBaseUrl;
        protected readonly string _summonerApiBaseUrl;
        protected readonly string _matchApiBaseUrl;
        protected readonly string _leagueApiBaseUrl;

        public RiotApiClient(HttpClient httpClient, IOptions<RiotApiSettings> settings, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            RiotApiSettings config = settings.Value;
            _apiKey = config.ApiKey;
            _accountApiBaseUrl = config.AccountApiBaseUrl;
            _summonerApiBaseUrl = config.SummonerApiBaseUrl;
            _matchApiBaseUrl = config.MatchApiBaseUrl;
            _leagueApiBaseUrl = config.LeagueApiBaseUrl;
            
            // Debug output
            _logger.LogInformation("🔑 API Key loaded: {Status}", 
                string.IsNullOrEmpty(_apiKey) ? "❌ MISSING" : $"✅ Present (length: {_apiKey.Length})");
            _logger.LogInformation("🌐 Account API URL: {Url}", _accountApiBaseUrl);
            _logger.LogInformation("🌐 Summoner API URL: {Url}", _summonerApiBaseUrl);
            _logger.LogInformation("🌐 Match API URL: {Url}", _matchApiBaseUrl);
            _logger.LogInformation("🌐 League API URL: {Url}", _leagueApiBaseUrl);
        }

        /// <summary>
        /// Adds Riot API authentication headers to the request
        /// </summary>
        protected HttpRequestMessage SetRequestMessageHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(_apiKey))
            {
                request.Headers.Add("X-Riot-Token", _apiKey);
            }
            return request;
        }

        /// <summary>
        /// Sends an HTTP request with retry logic for 502 errors
        /// </summary>
        protected async Task<string> SendRequestWithRetry(string url, int maxAttempts = 3)
        {
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "❌ Attempt {Attempt}/{MaxAttempts} failed for {Url}", attempt, maxAttempts, url);

                    if (attempt == maxAttempts || ex.StatusCode == null || (int)ex.StatusCode != 502)
                    {
                        throw;
                    }

                    _logger.LogInformation("🔄 Retrying request to {Url}...", url);
                    await Task.Delay(1000);
                }
            }

            throw new Exception($"Failed to fetch data from {url} after {maxAttempts} attempts.");
        }
    }
}

