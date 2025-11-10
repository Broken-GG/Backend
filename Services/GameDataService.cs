using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class GameDataService : IGameDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GameDataService> _logger;
        private Dictionary<int, string>? _summonerSpellIdToKey;
        private Dictionary<int, string>? _itemIdToName;
        private readonly object _lock = new object();
        private readonly IChampionDataService _championDataService;

        public GameDataService(HttpClient httpClient, IChampionDataService championDataService, ILogger<GameDataService> logger)
        {
            _httpClient = httpClient;
            _championDataService = championDataService;
            _logger = logger;
        }

        // Fetch summoner spell mapping from Data Dragon
        private Task<Dictionary<int, string>> GetSummonerSpellMappingAsync()
        {
            if (_summonerSpellIdToKey != null)
            {
                return Task.FromResult(_summonerSpellIdToKey);
            }

            lock (_lock)
            {
                if (_summonerSpellIdToKey != null)
                {
                    return Task.FromResult(_summonerSpellIdToKey);
                }

                try
                {
                    string version = _championDataService.GetCurrentVersionAsync().Result;
                    string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/summoner.json";
                    
                    string response = _httpClient.GetStringAsync(url).Result;
                    JObject json = JObject.Parse(response);
                    JToken? summonerSpells = json["data"];

                    _summonerSpellIdToKey = new Dictionary<int, string>();

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
                                    _summonerSpellIdToKey[id] = name;
                                }
                            }
                        }
                    }

                    _logger.LogInformation("✅ Loaded {Count} summoner spells from Data Dragon", _summonerSpellIdToKey.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error loading summoner spell data");
                    _summonerSpellIdToKey = new Dictionary<int, string>();
                }

                return Task.FromResult(_summonerSpellIdToKey);
            }
        }

        // Fetch item mapping from Data Dragon
        private Task<Dictionary<int, string>> GetItemMappingAsync()
        {
            if (_itemIdToName != null)
            {
                return Task.FromResult(_itemIdToName);
            }

            lock (_lock)
            {
                if (_itemIdToName != null)
                {
                    return Task.FromResult(_itemIdToName);
                }

                try
                {
                    string version = _championDataService.GetCurrentVersionAsync().Result;
                    string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json";
                    
                    string response = _httpClient.GetStringAsync(url).Result;
                    JObject json = JObject.Parse(response);
                    JToken? items = json["data"];

                    _itemIdToName = new Dictionary<int, string>();

                    if (items != null)
                    {
                        foreach (JToken item in items)
                        {
                            string itemId = item.Path.Split('.')[1];
                            if (int.TryParse(itemId, out int id))
                            {
                                JToken? itemData = item.First;
                                string name = itemData?["name"]?.ToString() ?? itemId;
                                _itemIdToName[id] = name;
                            }
                        }
                    }

                    _logger.LogInformation("✅ Loaded {Count} items from Data Dragon", _itemIdToName.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error loading item data");
                    _itemIdToName = new Dictionary<int, string>();
                }

                return Task.FromResult(_itemIdToName);
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
