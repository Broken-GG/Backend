using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.service
{
    public class ChampionDataService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Dictionary<long, string>? _championIdToKey;
        private static string? _latestVersion;
        private static readonly object _lock = new object();
        private const string FallbackVersion = "14.20.1";

        // Fetch the latest Data Dragon version from Riot's API
        private static async Task<string> GetLatestVersionAsync()
        {
            if (_latestVersion != null)
            {
                return _latestVersion;
            }

            try
            {
                var url = "https://ddragon.leagueoflegends.com/api/versions.json";
                var response = await _httpClient.GetStringAsync(url);
                var versions = JsonConvert.DeserializeObject<string[]>(response);
                
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
        public static async Task<string> GetCurrentVersionAsync()
        {
            return await GetLatestVersionAsync();
        }

        // Lazy load champion data from Data Dragon API
        private static async Task<Dictionary<long, string>> GetChampionMappingAsync()
        {
            if (_championIdToKey != null)
            {
                return _championIdToKey;
            }

            lock (_lock)
            {
                if (_championIdToKey != null)
                {
                    return _championIdToKey;
                }

                // Fetch champion data from Data Dragon
                var version = GetLatestVersionAsync().Result;
                var url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/champion.json";
                
                try
                {
                    var response = _httpClient.GetStringAsync(url).Result;
                    var json = JObject.Parse(response);
                    var champions = json["data"];

                    _championIdToKey = new Dictionary<long, string>();

                    if (champions != null)
                    {
                        foreach (var champion in champions)
                        {
                            var champData = champion.First;
                            if (champData != null && champData["key"] != null)
                            {
                                var keyValue = champData["key"]?.ToString();
                                if (keyValue != null)
                                {
                                    var id = long.Parse(keyValue);
                                    var name = champion.Path.Split('.')[1]; // Gets the champion key (e.g., "Ahri")
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

                return _championIdToKey;
            }
        }

        public static async Task<string> GetChampionNameByIdAsync(long championId)
        {
            var mapping = await GetChampionMappingAsync();
            return mapping.TryGetValue(championId, out var name) ? name : "Unknown";
        }

        public static async Task<string> GetChampionIconUrlAsync(long championId, string? version = null)
        {
            version ??= await GetLatestVersionAsync();
            var championName = await GetChampionNameByIdAsync(championId);
            if (championName == "Unknown")
            {
                return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/Unknown.png";
            }
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png";
        }

        public static async Task<(string Name, string IconUrl)> GetChampionDataAsync(long championId, string? version = null)
        {
            version ??= await GetLatestVersionAsync();
            var name = await GetChampionNameByIdAsync(championId);
            var iconUrl = await GetChampionIconUrlAsync(championId, version);
            return (name, iconUrl);
        }
    }
}
