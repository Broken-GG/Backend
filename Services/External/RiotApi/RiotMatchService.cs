using System;
using System.Net.Http;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.External.RiotApi
{
    /// <summary>
    /// Service for Riot Match API operations
    /// </summary>
    public class RiotMatchService : RiotApiClient, IRiotMatchService
    {
        public RiotMatchService(HttpClient httpClient, IOptions<RiotApiSettings> settings, ILogger<RiotMatchService> logger)
            : base(httpClient, settings, logger)
        {
        }

        /// <summary>
        /// Gets match history IDs for a player by PUUID with pagination
        /// </summary>
        public virtual async Task<string> GetMatchByPUUID(string puuid, int start = 0, int count = 10)
        {
            string url = $"{_matchApiBaseUrl}/matches/by-puuid/{puuid}/ids?start={start}&count={count}";
            _logger.LogInformation("🌐 API Call: GET {Url}", url);

            return await SendRequestWithRetry(url);
        }

        /// <summary>
        /// Gets detailed match information by match ID
        /// </summary>
        public virtual async Task<string> GetMatchDetailsByMatchId(string matchId)
        {
            string url = $"{_matchApiBaseUrl}/matches/{matchId}";
            
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}

