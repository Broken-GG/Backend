using System.ComponentModel.DataAnnotations;

namespace api.models
{
    public class MatchSummary
    {
        [Required]
        public string MatchId { get; set; } = string.Empty;
        
        [Required]
        public string GameMode { get; set; } = string.Empty; // "Ranked Flex", "ARAM", etc.
        
        [Required]
        public DateTime GameDate { get; set; }
        
        [Required]
        public int GameDurationMinutes { get; set; }
        
        [Required]
        public bool Victory { get; set; }
        
        // Main player (the one we searched for)
        [Required]
        public PlayerPerformance MainPlayer { get; set; } = new PlayerPerformance();
        
        // All players in the match (for team composition display)
        [Required]
        public PlayerPerformance[] AllPlayers { get; set; } = Array.Empty<PlayerPerformance>();
    }
    
    public class PlayerPerformance
    {
        [Required]
        public string SummonerName { get; set; } = string.Empty;
        
        [Required]
        public string ChampionName { get; set; } = string.Empty;
        
        [Required]
        public string ChampionImageUrl { get; set; } = string.Empty;
        
        [Required]
        public int Kills { get; set; }
        
        [Required]
        public int Deaths { get; set; }
        
        [Required]
        public int Assists { get; set; }
        
        [Required]
        public string KDA { get; set; } = string.Empty; // "2.14:1 KDA"
        
        [Required]
        public int TeamId { get; set; } // 100 or 200 for team identification
        
        [Required]
        public bool IsMainPlayer { get; set; } = false; // True for the player we searched for
    }
}