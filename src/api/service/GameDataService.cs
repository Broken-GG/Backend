using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.service
{
    public interface IGameDataService
    {
        Task<string> GetSummonerSpellNameByIdAsync(int spellId);
        Task<string> GetSummonerSpellIconUrlAsync(int spellId);
        Task<string> GetItemNameByIdAsync(int itemId);
        Task<string> GetItemIconUrlAsync(int itemId);
        Task<(string Name, string IconUrl)> GetSummonerSpellDataAsync(int spellId);
        Task<(string Name, string IconUrl)> GetItemDataAsync(int itemId);
        string GetArenaAugmentIconUrl(int augmentId);
    }

    public class GameDataService : IGameDataService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private Dictionary<int, string>? _summonerSpellIdToKey;
        private Dictionary<int, string>? _itemIdToName;
        private readonly object _lock = new object();

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
                    // For version, you will need to inject IChampionDataService in the constructor for DI
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
                                    string name = spell.Path.Split('.')[1]; // Gets the spell key (e.g., "SummonerFlash")
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
                            string itemId = item.Path.Split('.')[1]; // Gets the item ID as string
                            if (int.TryParse(itemId, out int id))
                            {
                                JToken? itemData = item.First;
                                string name = itemData?["name"]?.ToString() ?? itemId;
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

                return Task.FromResult(_itemIdToName);
            }
        }

        private readonly IChampionDataService _championDataService;

        public GameDataService(IChampionDataService championDataService)
        {
            _championDataService = championDataService;
        }

        // Get summoner spell name by ID
        public async Task<string> GetSummonerSpellNameByIdAsync(int spellId)
        {
            if (spellId == 0)
            {
                return "None";
            }

            Dictionary<int, string> mapping = await GetSummonerSpellMappingAsync();
            return mapping.TryGetValue(spellId, out string? name) ? name : "Unknown";
        }

        // Get summoner spell icon URL by ID
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
                return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/SummonerBarrier.png"; // Fallback
            }
            
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/{spellName}.png";
        }

        // Get item name by ID
        public async Task<string> GetItemNameByIdAsync(int itemId)
        {
            if (itemId == 0)
            {
                return "Empty";
            }

            Dictionary<int, string> mapping = await GetItemMappingAsync();
            return mapping.TryGetValue(itemId, out string? name) ? name : "Unknown Item";
        }

        // Get item icon URL by ID
        public async Task<string> GetItemIconUrlAsync(int itemId)
        {
            if (itemId == 0)
            {
                return ""; // Empty slot
            }

            string version = await _championDataService.GetCurrentVersionAsync();
            return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/item/{itemId}.png";
        }

        // Get complete summoner spell data (name and icon URL)
        public async Task<(string Name, string IconUrl)> GetSummonerSpellDataAsync(int spellId)
        {
            string name = await GetSummonerSpellNameByIdAsync(spellId);
            string iconUrl = await GetSummonerSpellIconUrlAsync(spellId);
            return (name, iconUrl);
        }

        // Get complete item data (name and icon URL)
        public async Task<(string Name, string IconUrl)> GetItemDataAsync(int itemId)
        {
            string name = await GetItemNameByIdAsync(itemId);
            string iconUrl = await GetItemIconUrlAsync(itemId);
            return (name, iconUrl);
        }

        // Get Arena augment icon URL by ID
        // Arena augments use CommunityDragon CDN
        public string GetArenaAugmentIconUrl(int augmentId)
        {
            if (augmentId == 0)
            {
                return ""; // No augment
            }

            // CommunityDragon path for Arena augments (Cherry is the codename for Arena)
            // These augments are stored in the plugins directory
            return $"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/cherry-augments/{augmentId}.png";
        }
    }
}
