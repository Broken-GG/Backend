using Newtonsoft.Json;

namespace api.models
{
    public class MasteryInfo
    {
        [JsonProperty("puuid")]
        public string Puuid { get; set; } = string.Empty;
        
        [JsonProperty("championId")]
        public long ChampionId { get; set; }
        
        [JsonProperty("championLevel")]
        public int ChampionLevel { get; set; }
        
        [JsonProperty("championPoints")]
        public int ChampionPoints { get; set; }
        
        [JsonProperty("championName")]
        public string ChampionName { get; set; } = string.Empty;
        
        [JsonProperty("championIconUrl")]
        public string ChampionIconUrl { get; set; } = string.Empty;
        
        [JsonProperty("lastPlayTime")]
        public long LastPlayTime { get; set; }
        
        [JsonProperty("championPointsSinceLastLevel")]
        public int ChampionPointsSinceLastLevel { get; set; }
        
        [JsonProperty("championPointsUntilNextLevel")]
        public int ChampionPointsUntilNextLevel { get; set; }
        
        [JsonProperty("chestGranted")]
        public bool ChestGranted { get; set; }
        
        [JsonProperty("tokensEarned")]
        public int TokensEarned { get; set; }
    }
}
