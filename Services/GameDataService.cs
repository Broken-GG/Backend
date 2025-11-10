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
    public class GameDataService : IGameDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GameDataService> _logger;
        private readonly IChampionDataService _championDataService;
        private const int CacheExpirationHours = 1;

        public GameDataService(HttpClient httpClient, IChampionDataService championDataService, IMemoryCache cache, ILogger<GameDataService> logger)
        {
            _httpClient = httpClient;
            _championDataService = championDataService;
            _cache = cache;
            _logger = logger;
        }

        // Fetch summoner spell mapping from Data Dragon
        private async Task<Dictionary<int, string>> GetSummonerSpellMappingAsync()
        {
            string version = await _championDataService.GetCurrentVersionAsync();
            string cacheKey = $"summoner-spell-mapping-{version}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<int, string>? cachedMapping) && cachedMapping != null)
            {
                return cachedMapping;
            }

            try
            {
                string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/summoner.json";
                
                string response = await _httpClient.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                JToken? summonerSpells = json["data"];

                Dictionary<int, string> summonerSpellIdToKey = new Dictionary<int, string>();

                if (summonerSpells != null)
                {
                    foreach (JToken spell in summonerSpells)
                    {
                        JToken? spellData = spell.First;
                        if (spellData != null && spellData["key"] != null)
                        {
                            string? keyValue = spellData["key"]?.ToString();
                            if (keyValue != null && int.TryParse(keyValue, out int id))
                            {
                                string name = spell.Path.Split('.')[1];
                                summonerSpellIdToKey[id] = name;
                            }
                        }
                    }
                }

                _cache.Set(cacheKey, summonerSpellIdToKey, TimeSpan.FromHours(CacheExpirationHours));
                _logger.LogInformation("Loaded {Count} summoner spells from Data Dragon version {Version}", summonerSpellIdToKey.Count, version);
                return summonerSpellIdToKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading summoner spell data from version {Version}", version);
                return new Dictionary<int, string>();
            }
        }

        // Fetch item mapping from Data Dragon
        private async Task<Dictionary<int, string>> GetItemMappingAsync()
        {
            string version = await _championDataService.GetCurrentVersionAsync();
            string cacheKey = $"item-mapping-{version}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<int, string>? cachedMapping) && cachedMapping != null)
            {
                return cachedMapping;
            }

            try
            {
                string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json";
                
                string response = await _httpClient.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                JToken? items = json["data"];

                Dictionary<int, string> itemIdToName = new Dictionary<int, string>();

                if (items != null)
                {
                    foreach (JToken item in items)
                    {
                        string itemId = item.Path.Split('.')[1];
                        if (int.TryParse(itemId, out int id))
                        {
                            JToken? itemData = item.First;
                            string name = itemData?["name"]?.ToString() ?? itemId;
                            itemIdToName[id] = name;
                        }
                    }
                }

                _cache.Set(cacheKey, itemIdToName, TimeSpan.FromHours(CacheExpirationHours));
                _logger.LogInformation("Loaded {Count} items from Data Dragon version {Version}", itemIdToName.Count, version);
                return itemIdToName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item data from version {Version}", version);
                return new Dictionary<int, string>();
            }
        }

        public async Task<string> GetSummonerSpellNameByIdAsync(int spellId)
        {
            if (spellId == 0)
            {
                return "None";
            }

            Dictionary<int, string> mapping = await GetSummonerSpellMappingAsync();
            return mapping.TryGetValue(spellId, out string? name) ? name : "Unknown";
        }

        public async Task<string> GetSummonerSpellIconUrlAsync(int spellId)
        {
            if (spellId == 0)
            {
                return "";
            }

            string version = await _championDataService.GetCurrentVersionAsync();
            string spellName = await GetSummonerSpellNameByIdAsync(spellId);
            
            if (spellName == "Unknown" || spellName == "None")
            {
                return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/SummonerBarrier.png";
            }
            
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/{spellName}.png";
        }

        public async Task<string> GetItemNameByIdAsync(int itemId)
        {
            if (itemId == 0)
            {
                return "Empty";
            }

            Dictionary<int, string> mapping = await GetItemMappingAsync();
            return mapping.TryGetValue(itemId, out string? name) ? name : "Unknown Item";
        }

        public async Task<string> GetItemIconUrlAsync(int itemId)
        {
            if (itemId == 0)
            {
                return "";
            }

            string version = await _championDataService.GetCurrentVersionAsync();
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/item/{itemId}.png";
        }

        public async Task<(string Name, string IconUrl)> GetSummonerSpellDataAsync(int spellId)
        {
            string name = await GetSummonerSpellNameByIdAsync(spellId);
            string iconUrl = await GetSummonerSpellIconUrlAsync(spellId);
            return (name, iconUrl);
        }

        public async Task<(string Name, string IconUrl)> GetItemDataAsync(int itemId)
        {
            string name = await GetItemNameByIdAsync(itemId);
            string iconUrl = await GetItemIconUrlAsync(itemId);
            return (name, iconUrl);
        }

        public string GetArenaAugmentIconUrl(int augmentId)
        {
            if (augmentId == 0)
            {
                return "";
            }

            return $"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/cherry-augments/{augmentId}.png";
        }
    }
}
