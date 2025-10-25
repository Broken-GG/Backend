using Newtonsoft.Json;

namespace api.models
{
    public class RankedInfo
    {
        [JsonProperty("queueType")]
        public string QueueType { get; set; } = string.Empty;
        
        [JsonProperty("tier")]
        public string Tier { get; set; } = string.Empty;
        
        [JsonProperty("rank")]
        public string Rank { get; set; } = string.Empty;
        
        [JsonProperty("leaguePoints")]
        public int LeaguePoints { get; set; }
        
        [JsonProperty("wins")]
        public int Wins { get; set; }
        
        [JsonProperty("losses")]
        public int Losses { get; set; }
        
        [JsonProperty("hotStreak")]
        public bool HotStreak { get; set; }
        
        [JsonProperty("veteran")]
        public bool Veteran { get; set; }
        
        [JsonProperty("freshBlood")]
        public bool FreshBlood { get; set; }
        
        [JsonProperty("inactive")]
        public bool Inactive { get; set; }
    }
}
