using System.Net.Http;
using System.Threading.Tasks;
using Backend.Configuration;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.External.RiotApi
{
    /// <summary>
    /// Service for Riot League/Ranked API operations
    /// </summary>
    public class RiotLeagueService : RiotApiClient, IRiotLeagueService
    {
        public RiotLeagueService(HttpClient httpClient, IOptions<RiotApiSettings> settings, ILogger<RiotLeagueService> logger)
            : base(httpClient, settings, logger)
        {
        }

        /// <summary>
        /// Gets ranked/league information for a player by PUUID
        /// </summary>
        public virtual async Task<string> GetRankedInfoByPUUID(string puuid)
        {
            string url = $"{_leagueApiBaseUrl}/entries/by-puuid/{puuid}";
            
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        /// <summary>
        /// Gets champion mastery information for a player by PUUID
        /// </summary>
        public virtual async Task<string> GetMasteryInfoByPUUID(string puuid)
        {
            string url = $"https://euw1.api.riotgames.com/lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid}";
            
            HttpRequestMessage request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}

