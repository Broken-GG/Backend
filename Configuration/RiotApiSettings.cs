namespace Backend.Configuration
{
    /// <summary>
    /// Configuration settings for Riot API
    /// </summary>
    public class RiotApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string AccountApiBaseUrl { get; set; } = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id";
        public string SummonerApiBaseUrl { get; set; } = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid";
        public string MatchApiBaseUrl { get; set; } = "https://europe.api.riotgames.com/lol/match/v5";
        public string LeagueApiBaseUrl { get; set; } = "https://euw1.api.riotgames.com/lol/league/v4";
        public string Region { get; set; } = "euw1";
        public int DefaultMatchCount { get; set; } = 10;
        public int MaxMatchCount { get; set; } = 100;
    }
}
