using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.service
{
    public class GameDataService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Dictionary<int, string>? _summonerSpellIdToKey;
        private static Dictionary<int, string>? _itemIdToName;
        private static readonly object _lock = new object();

        // Fetch summoner spell mapping from Data Dragon
        private static async Task<Dictionary<int, string>> GetSummonerSpellMappingAsync()
        {
            if (_summonerSpellIdToKey != null)
            {
                return _summonerSpellIdToKey;
            }

            lock (_lock)
            {
                if (_summonerSpellIdToKey != null)
                {
                    return _summonerSpellIdToKey;
                }

                try
                {
                    var version = ChampionDataService.GetCurrentVersionAsync().Result;
                    var url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/summoner.json";
                    
                    var response = _httpClient.GetStringAsync(url).Result;
                    var json = JObject.Parse(response);
                    var summonerSpells = json["data"];

                    _summonerSpellIdToKey = new Dictionary<int, string>();

                    if (summonerSpells != null)
                    {
                        foreach (var spell in summonerSpells)
                        {
                            var spellData = spell.First;
                            if (spellData != null && spellData["key"] != null)
                            {
                                var keyValue = spellData["key"]?.ToString();
                                if (keyValue != null && int.TryParse(keyValue, out var id))
                                {
                                    var name = spell.Path.Split('.')[1]; // Gets the spell key (e.g., "SummonerFlash")
                                    _summonerSpellIdToKey[id] = name;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"✅ Loaded {_summonerSpellIdToKey.Count} summoner spells from Data Dragon");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error loading summoner spell data: {ex.Message}");
                    _summonerSpellIdToKey = new Dictionary<int, string>();
                }

                return _summonerSpellIdToKey;
            }
        }

        // Fetch item mapping from Data Dragon
        private static async Task<Dictionary<int, string>> GetItemMappingAsync()
        {
            if (_itemIdToName != null)
            {
                return _itemIdToName;
            }

            lock (_lock)
            {
                if (_itemIdToName != null)
                {
                    return _itemIdToName;
                }

                try
                {
                    var version = ChampionDataService.GetCurrentVersionAsync().Result;
                    var url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/item.json";
                    
                    var response = _httpClient.GetStringAsync(url).Result;
                    var json = JObject.Parse(response);
                    var items = json["data"];

                    _itemIdToName = new Dictionary<int, string>();

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var itemId = item.Path.Split('.')[1]; // Gets the item ID as string
                            if (int.TryParse(itemId, out var id))
                            {
                                var itemData = item.First;
                                var name = itemData?["name"]?.ToString() ?? itemId;
                                _itemIdToName[id] = name;
                            }
                        }
                    }

                    Console.WriteLine($"✅ Loaded {_itemIdToName.Count} items from Data Dragon");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error loading item data: {ex.Message}");
                    _itemIdToName = new Dictionary<int, string>();
                }

                return _itemIdToName;
            }
        }

        // Get summoner spell name by ID
        public static async Task<string> GetSummonerSpellNameByIdAsync(int spellId)
        {
            if (spellId == 0)
            {
                return "None";
            }

            var mapping = await GetSummonerSpellMappingAsync();
            return mapping.TryGetValue(spellId, out var name) ? name : "Unknown";
        }

        // Get summoner spell icon URL by ID
        public static async Task<string> GetSummonerSpellIconUrlAsync(int spellId)
        {
            if (spellId == 0)
            {
                return "";
            }

            var version = await ChampionDataService.GetCurrentVersionAsync();
            var spellName = await GetSummonerSpellNameByIdAsync(spellId);
            
            if (spellName == "Unknown" || spellName == "None")
            {
                return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/SummonerBarrier.png"; // Fallback
            }
            
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/{spellName}.png";
        }

        // Get item name by ID
        public static async Task<string> GetItemNameByIdAsync(int itemId)
        {
            if (itemId == 0)
            {
                return "Empty";
            }

            var mapping = await GetItemMappingAsync();
            return mapping.TryGetValue(itemId, out var name) ? name : "Unknown Item";
        }

        // Get item icon URL by ID
        public static async Task<string> GetItemIconUrlAsync(int itemId)
        {
            if (itemId == 0)
            {
                return ""; // Empty slot
            }

            var version = await ChampionDataService.GetCurrentVersionAsync();
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/item/{itemId}.png";
        }

        // Get complete summoner spell data (name and icon URL)
        public static async Task<(string Name, string IconUrl)> GetSummonerSpellDataAsync(int spellId)
        {
            var name = await GetSummonerSpellNameByIdAsync(spellId);
            var iconUrl = await GetSummonerSpellIconUrlAsync(spellId);
            return (name, iconUrl);
        }

        // Get complete item data (name and icon URL)
        public static async Task<(string Name, string IconUrl)> GetItemDataAsync(int itemId)
        {
            var name = await GetItemNameByIdAsync(itemId);
            var iconUrl = await GetItemIconUrlAsync(itemId);
            return (name, iconUrl);
        }
    }
}
