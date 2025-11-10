using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Backend.Models.DTOs.Response
{
    public class SummonerResponse
    {
        [Required]
        [JsonProperty("summonerName")]
        public string SummonerName { get; set; } = string.Empty;

        [Required]
        [JsonProperty("tagline")]
        public string Tagline { get; set; } = string.Empty;

        [Required]
        [JsonProperty("puuid")]
        public string PUUID { get; set; } = string.Empty;

        [Required]
        [JsonProperty("level")]
        public int Level { get; set; } = 0;

        [Required]
        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [Required]
        [JsonProperty("profileIconUrl")]
        public string ProfileIconUrl { get; set; } = string.Empty;
    }
}
