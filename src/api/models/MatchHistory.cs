using System.ComponentModel.DataAnnotations;

namespace api.models
{
    public class MatchHistory
    {
        [Required]
        public string MatchId { get; set; } = string.Empty;

        [Required]
        public string[] ChampionName { get; set; } = Array.Empty<string>();
        public string[] ChampionImageUrl { get; set; } = Array.Empty<string>();
        public SummonerInfo[] Participants { get; set; } = Array.Empty<SummonerInfo>();

        public int Kills { get; set; } = 0;
        public int Deaths { get; set; } = 0;
        public int Assists { get; set; } = 0;

        public bool Win { get; set; } = false;

        public DateTime GameDate { get; set; } = DateTime.MinValue;

        public int GameDurationSeconds { get; set; } = 0;
    }
}