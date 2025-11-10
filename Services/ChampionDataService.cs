using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class ChampionDataService : IChampionDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChampionDataService> _logger;
        private const string FallbackVersion = "14.20.1";
        private const int CacheExpirationHours = 1;

        public ChampionDataService(HttpClient httpClient, IMemoryCache cache, ILogger<ChampionDataService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        // Fetch the latest Data Dragon version from Riot's API
        private async Task<string> GetLatestVersionAsync()
        {
            const string cacheKey = "ddragon-version";

            if (_cache.TryGetValue(cacheKey, out string? cachedVersion) && cachedVersion != null)
            {
                return cachedVersion;
            }

            try
            {
                string url = "https://ddragon.leagueoflegends.com/api/versions.json";
                string response = await _httpClient.GetStringAsync(url);
                string[]? versions = JsonConvert.DeserializeObject<string[]>(response);
                
                if (versions != null && versions.Length > 0)
                {
                    string latestVersion = versions[0];
                    _cache.Set(cacheKey, latestVersion, TimeSpan.FromHours(CacheExpirationHours));
                    _logger.LogInformation("Latest Data Dragon version: {Version}", latestVersion);
                    return latestVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch latest version, using fallback {Version}", FallbackVersion);
            }

            return FallbackVersion;
        }

        // Public method to get the current version being used
        public async Task<string> GetCurrentVersionAsync()
        {
            return await GetLatestVersionAsync();
        }

        // Lazy load champion data from Data Dragon API
        private async Task<Dictionary<long, string>> GetChampionMappingAsync()
        {
            string version = await GetLatestVersionAsync();
            string cacheKey = $"champion-mapping-{version}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<long, string>? cachedMapping) && cachedMapping != null)
            {
                return cachedMapping;
            }

            string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json";

            try
            {
                string response = await _httpClient.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                JToken? champions = json["data"];

                Dictionary<long, string> championIdToKey = new Dictionary<long, string>();

                if (champions != null)
                {
                    foreach (JToken champion in champions)
                    {
                        JToken? champData = champion.First;
                        if (champData != null && champData["key"] != null)
                        {
                            string? keyValue = champData["key"]?.ToString();
                            if (keyValue != null)
                            {
                                long id = long.Parse(keyValue);
                                string name = champion.Path.Split('.')[1];
                                championIdToKey[id] = name;
                            }
                        }
                    }
                }

                _cache.Set(cacheKey, championIdToKey, TimeSpan.FromHours(CacheExpirationHours));
                _logger.LogInformation("Loaded {Count} champions from Data Dragon version {Version}", championIdToKey.Count, version);
                return championIdToKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading champion data from version {Version}", version);
                return new Dictionary<long, string>();
            }
        }

        public async Task<string> GetChampionNameByIdAsync(long championId)
        {
            Dictionary<long, string> mapping = await GetChampionMappingAsync();
            return mapping.TryGetValue(championId, out string? name) ? name : "Unknown";
        }

        public async Task<string> GetChampionIconUrlAsync(long championId, string? version = null)
        {
            version ??= await GetLatestVersionAsync();
            string championName = await GetChampionNameByIdAsync(championId);
            if (championName == "Unknown")
            {
                return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/Unknown.png";
            }
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png";
        }

        public async Task<(string Name, string IconUrl)> GetChampionDataAsync(long championId, string? version = null)
        {
            version ??= await GetLatestVersionAsync();
            string name = await GetChampionNameByIdAsync(championId);
            string iconUrl = await GetChampionIconUrlAsync(championId, version);
            return (name, iconUrl);
        }
    }
}
