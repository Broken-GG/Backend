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
    /// Service for Riot Account and Summoner API operations
    /// </summary>
    public class RiotAccountService : RiotApiClient, IRiotAccountService
    {
        public RiotAccountService(HttpClient httpClient, IOptions<RiotApiSettings> settings, ILogger<RiotAccountService> logger)
            : base(httpClient, settings, logger)
        {
        }

        /// <summary>
        /// Gets PUUID and account information by summoner name and tagline
        /// </summary>
        public virtual async Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline)
        {
            ArgumentNullException.ThrowIfNull(summonerName);
            ArgumentNullException.ThrowIfNull(tagline);

            if (string.IsNullOrWhiteSpace(summonerName))
                throw new ArgumentException("Summoner name cannot be empty", nameof(summonerName));
            if (string.IsNullOrWhiteSpace(tagline))
                throw new ArgumentException("Tagline cannot be empty", nameof(tagline));

            string url = $"{_accountApiBaseUrl}/{summonerName}/{tagline}";
            
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Gets summoner information by PUUID
        /// </summary>
        public virtual async Task<string> GetSummonerByPUUID(string puuid)
        {
            ArgumentNullException.ThrowIfNull(puuid);

            if (string.IsNullOrWhiteSpace(puuid))
                throw new ArgumentException("PUUID cannot be empty", nameof(puuid));

            string url = $"{_summonerApiBaseUrl}/{puuid}";
            
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}

