using System.ComponentModel.DataAnnotations;

namespace api.models
{
    public class SummonerInfo
    {
        [Required]
        public string SummonerName { get; set; } = string.Empty;

        [Required]
        public string Tagline { get; set; } = string.Empty;

        public int Level { get; set; } = 0;

        [Required]
        public string Region { get; set; } = string.Empty;

        public string ProfileIconUrl { get; set; } = string.Empty;
    }
}