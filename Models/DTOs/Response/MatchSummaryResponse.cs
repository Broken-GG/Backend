using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs.Response
{
    public class MatchSummaryResponse
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
        public PlayerPerformanceResponse MainPlayer { get; set; } = new PlayerPerformanceResponse();
        
        // All players in the match (for team composition display)
        [Required]
        public PlayerPerformanceResponse[] AllPlayers { get; set; } = Array.Empty<PlayerPerformanceResponse>();
    }
    
    public class PlayerPerformanceResponse
    {
        [Required]
        public string SummonerName { get; set; } = string.Empty;

        [Required]
        public string Tagline { get; set; } = string.Empty;
        
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
        public int CS { get; set; }

        [Required]
        public int VisionScore { get; set; }
        
        [Required]
        public string KDA { get; set; } = string.Empty;

        // Arena augments (stored as array of IDs)
        public int[] PlayerAugments { get; set; } = Array.Empty<int>();
        
        [Required]
        public int TeamId { get; set; } // 100 or 200 for team identification

        public string TeamPosition { get; set; } = string.Empty;

        // For Arena mode: subteam placement (1-4, indicates which duo team)
        public int SubteamPlacement { get; set; } = 0;

        [Required]
        public bool IsMainPlayer { get; set; } = false; // True for the player we searched for
        
        public int summoner1Id { get; set; }
        public int summoner2Id { get; set; }

        // Summoner spell image URLs
        public string Summoner1ImageUrl { get; set; } = string.Empty;
        public string Summoner2ImageUrl { get; set; } = string.Empty;

        public int item0 { get; set; }
        public int item1 { get; set; }
        public int item2 { get; set; }
        public int item3 { get; set; }
        public int item4 { get; set; }
        public int item5 { get; set; }
        public int item6 { get; set; }

        // Item image URLs
        public string Item0ImageUrl { get; set; } = string.Empty;
        public string Item1ImageUrl { get; set; } = string.Empty;
        public string Item2ImageUrl { get; set; } = string.Empty;
        public string Item3ImageUrl { get; set; } = string.Empty;
        public string Item4ImageUrl { get; set; } = string.Empty;
        public string Item5ImageUrl { get; set; } = string.Empty;
        public string Item6ImageUrl { get; set; } = string.Empty;
    }
}
