using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.service
{
    public interface IChampionDataService
    {
        Task<string> GetCurrentVersionAsync();
        Task<string> GetChampionNameByIdAsync(long championId);
        Task<string> GetChampionIconUrlAsync(long championId, string? version = null);
        Task<(string Name, string IconUrl)> GetChampionDataAsync(long championId, string? version = null);
    }

    public class ChampionDataService : IChampionDataService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private Dictionary<long, string>? _championIdToKey;
        private string? _latestVersion;
        private readonly object _lock = new object();
        private const string FallbackVersion = "14.20.1";

        // Fetch the latest Data Dragon version from Riot's API
        private async Task<string> GetLatestVersionAsync()
        {
            if (_latestVersion != null)
            {
                return _latestVersion;
            }

            try
            {
                string url = "https://ddragon.leagueoflegends.com/api/versions.json";
                string response = await _httpClient.GetStringAsync(url);
                string[]? versions = JsonConvert.DeserializeObject<string[]>(response);
                
                if (versions != null && versions.Length > 0)
                {
                    _latestVersion = versions[0]; // First version is the latest
                    Console.WriteLine($"✅ Latest Data Dragon version: {_latestVersion}");
                    return _latestVersion;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to fetch latest version, using fallback {FallbackVersion}: {ex.Message}");
            }

            _latestVersion = FallbackVersion;
            return _latestVersion;
        }

        // Public method to get the current version being used
        public async Task<string> GetCurrentVersionAsync()
        {
            return await GetLatestVersionAsync();
        }

        // Lazy load champion data from Data Dragon API
        private Task<Dictionary<long, string>> GetChampionMappingAsync()
        {
            if (_championIdToKey != null)
            {
                return Task.FromResult(_championIdToKey);
            }

            lock (_lock)
            {
                if (_championIdToKey != null)
                {
                    return Task.FromResult(_championIdToKey);
                }

                // Fetch champion data from Data Dragon
                string version = GetLatestVersionAsync().Result;
                string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json";

                try
                {
                    string response = _httpClient.GetStringAsync(url).Result;
                    JObject json = JObject.Parse(response);
                    JToken? champions = json["data"];

                    _championIdToKey = new Dictionary<long, string>();

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
                                    _championIdToKey[id] = name;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"✅ Loaded {_championIdToKey.Count} champions from Data Dragon");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error loading champion data: {ex.Message}");
                    _championIdToKey = new Dictionary<long, string>();
                }

                return Task.FromResult(_championIdToKey);
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
