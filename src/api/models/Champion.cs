using System.ComponentModel.DataAnnotations;

namespace api.models
{
    public class ChampionInfo
    {
        [Required]
        public string ChampionName { get; set; } = string.Empty;

        public int MasteryLevel { get; set; } = 0;

        public int MasteryPoints { get; set; } = 0;

    }
}