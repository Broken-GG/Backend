using Newtonsoft.Json;

namespace Backend.Models.RiotApi
{
    // Root object for Riot API match data
    public class RiotMatchData
    {
        [JsonProperty("metadata")]
        public RiotMatchMetadata? Metadata { get; set; }

        [JsonProperty("info")]
        public RiotMatchInfo? Info { get; set; }
    }

    public class RiotMatchMetadata
    {
        [JsonProperty("matchId")]
        public string? MatchId { get; set; }

        [JsonProperty("participants")]
        public string[]? Participants { get; set; }
    }

    public class RiotMatchInfo
    {
        [JsonProperty("gameCreation")]
        public long? GameCreation { get; set; }

        [JsonProperty("gameDuration")]
        public int? GameDuration { get; set; }

        [JsonProperty("gameEndTimestamp")]
        public long? GameEndTimestamp { get; set; }

        [JsonProperty("gameId")]
        public long? GameId { get; set; }

        [JsonProperty("gameMode")]
        public string? GameMode { get; set; }

        [JsonProperty("gameName")]
        public string? GameName { get; set; }

        [JsonProperty("gameStartTimestamp")]
        public long? GameStartTimestamp { get; set; }

        [JsonProperty("gameType")]
        public string? GameType { get; set; }

        [JsonProperty("gameVersion")]
        public string? GameVersion { get; set; }

        [JsonProperty("mapId")]
        public int? MapId { get; set; }

        [JsonProperty("participants")]
        public List<RiotParticipant>? Participants { get; set; }

        [JsonProperty("platformId")]
        public string? PlatformId { get; set; }

        [JsonProperty("queueId")]
        public int? QueueId { get; set; }

        [JsonProperty("tournamentCode")]
        public string? TournamentCode { get; set; }
    }

    public class RiotParticipant
    {
        [JsonProperty("puuid")]
        public string? Puuid { get; set; }

        [JsonProperty("summonerName")]
        public string? SummonerName { get; set; }

        [JsonProperty("riotIdGameName")]
        public string? RiotIdGameName { get; set; }

        [JsonProperty("riotIdTagline")]
        public string? RiotIdTagline { get; set; }

        [JsonProperty("summonerTagline")]
        public string? SummonerTagline { get; set; }

        [JsonProperty("championName")]
        public string? ChampionName { get; set; }

        [JsonProperty("championId")]
        public int? ChampionId { get; set; }

        [JsonProperty("kills")]
        public int? Kills { get; set; }

        [JsonProperty("deaths")]
        public int? Deaths { get; set; }

        [JsonProperty("assists")]
        public int? Assists { get; set; }

        [JsonProperty("teamId")]
        public int? TeamId { get; set; }

        [JsonProperty("teamPosition")]
        public string? TeamPosition { get; set; }

        [JsonProperty("win")]
        public bool? Win { get; set; }

        [JsonProperty("totalMinionsKilled")]
        public int? TotalMinionsKilled { get; set; }

        [JsonProperty("neutralMinionsKilled")]
        public int? NeutralMinionsKilled { get; set; }

        [JsonProperty("visionScore")]
        public int? VisionScore { get; set; }

        [JsonProperty("summoner1Id")]
        public int? Summoner1Id { get; set; }

        [JsonProperty("summoner2Id")]
        public int? Summoner2Id { get; set; }

        [JsonProperty("item0")]
        public int? Item0 { get; set; }

        [JsonProperty("item1")]
        public int? Item1 { get; set; }

        [JsonProperty("item2")]
        public int? Item2 { get; set; }

        [JsonProperty("item3")]
        public int? Item3 { get; set; }

        [JsonProperty("item4")]
        public int? Item4 { get; set; }

        [JsonProperty("item5")]
        public int? Item5 { get; set; }

        [JsonProperty("item6")]
        public int? Item6 { get; set; }

        [JsonProperty("subteamPlacement")]
        public int? SubteamPlacement { get; set; }

        [JsonProperty("playerAugment1")]
        public int? PlayerAugment1 { get; set; }

        [JsonProperty("playerAugment2")]
        public int? PlayerAugment2 { get; set; }

        [JsonProperty("playerAugment3")]
        public int? PlayerAugment3 { get; set; }

        [JsonProperty("playerAugment4")]
        public int? PlayerAugment4 { get; set; }
    }
}
