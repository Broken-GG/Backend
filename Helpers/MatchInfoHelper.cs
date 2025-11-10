using Backend.Models.DTOs.Response;
using Backend.Models.RiotApi;
using Backend.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Helpers
{
    /// <summary>
    /// Helper class for match information processing
    /// </summary>
    public static class MatchInfoHelper
    {
        /// <summary>
        /// Deserialize match summary from Riot match data
        /// </summary>
        /// <param name="jsonData">JSON data from Riot API</param>
        /// <param name="playerPuuid">The PUUID of the player to focus on</param>
        /// <param name="championDataService">Champion data service for fetching champion info</param>
        /// <param name="gameDataService">Game data service for fetching item and spell info</param>
        /// <returns>The MatchSummaryResponse object or null if parsing fails</returns>
        public static async Task<MatchSummaryResponse?> DeserializeMatchSummary(
            string jsonData, 
            string playerPuuid, 
            IChampionDataService championDataService, 
            IGameDataService gameDataService)
        {
            Console.WriteLine($"[DEBUG] DeserializeMatchSummary called with playerPuuid: {playerPuuid}");
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return null;
                }

                RiotMatchData? matchData = JsonConvert.DeserializeObject<RiotMatchData>(jsonData);

                if (matchData?.Info?.Participants == null)
                {
                    return null;
                }

                RiotParticipant? playerParticipant = null;
                List<RiotParticipant> participants = matchData.Info.Participants;

                foreach (RiotParticipant participant in participants)
                {
                    if (participant?.Puuid == playerPuuid)
                    {
                        playerParticipant = participant;
                        break;
                    }
                }

                if (playerParticipant == null)
                {
                    return null;
                }

                List<PlayerPerformanceResponse> allPlayers = new List<PlayerPerformanceResponse>();

                if (participants != null)
                {
                    foreach (RiotParticipant participant in participants)
                    {
                        string puuid = participant?.Puuid ?? "";
                        // Try multiple sources for summoner name in order of preference
                        string summonerName = !string.IsNullOrEmpty(participant?.RiotIdGameName)
                            ? participant?.RiotIdGameName ?? "Unknown Player"
                            : !string.IsNullOrEmpty(participant?.SummonerName)
                                ? participant?.SummonerName ?? "Unknown Player"
                                : "Unknown Player";
                        string tagLine = !string.IsNullOrEmpty(participant?.RiotIdTagline)
                            ? participant?.RiotIdTagline ?? ""
                            : !string.IsNullOrEmpty(participant?.SummonerTagline)
                                ? participant?.SummonerTagline ?? ""
                                : "Unknown Tagline";
                        string championName = participant?.ChampionName ?? "Unknown";
                        int kills = participant?.Kills ?? 0;
                        int deaths = participant?.Deaths ?? 0;
                        int assists = participant?.Assists ?? 0;
                        int teamId = participant?.TeamId ?? 0;
                        string teamPosition = participant?.TeamPosition ?? "Unknown";
                        int subteamPlacement = participant?.SubteamPlacement ?? 0; // Arena mode only

                        List<int> playerAugments = new List<int>();
                        if (participant?.PlayerAugment1 != null) playerAugments.Add(participant.PlayerAugment1.Value);
                        if (participant?.PlayerAugment2 != null) playerAugments.Add(participant.PlayerAugment2.Value);
                        if (participant?.PlayerAugment3 != null) playerAugments.Add(participant.PlayerAugment3.Value);
                        if (participant?.PlayerAugment4 != null) playerAugments.Add(participant.PlayerAugment4.Value);

                        double kdaRatio = deaths > 0 ? Math.Round((double)(kills + assists) / deaths, 2) : kills + assists;
                        string kdaText = $"{kills}/{deaths}/{assists} ({kdaRatio}:1 KDA)";

                        string version = await championDataService.GetCurrentVersionAsync();
                        string championIconUrl = championName != "Unknown"
                            ? $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png"
                            : $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/Unknown.png";
                        bool isMainPlayer = puuid == playerPuuid;

                        int summoner1Id = participant?.Summoner1Id ?? 0;
                        int summoner2Id = participant?.Summoner2Id ?? 0;

                        int item0 = participant?.Item0 ?? 0;
                        int item1 = participant?.Item1 ?? 0;
                        int item2 = participant?.Item2 ?? 0;
                        int item3 = participant?.Item3 ?? 0;
                        int item4 = participant?.Item4 ?? 0;
                        int item5 = participant?.Item5 ?? 0;
                        int item6 = participant?.Item6 ?? 0;

                        string summoner1Url = await gameDataService.GetSummonerSpellIconUrlAsync(summoner1Id);
                        string summoner2Url = await gameDataService.GetSummonerSpellIconUrlAsync(summoner2Id);
                        string item0Url = await gameDataService.GetItemIconUrlAsync(item0);
                        string item1Url = await gameDataService.GetItemIconUrlAsync(item1);
                        string item2Url = await gameDataService.GetItemIconUrlAsync(item2);
                        string item3Url = await gameDataService.GetItemIconUrlAsync(item3);
                        string item4Url = await gameDataService.GetItemIconUrlAsync(item4);
                        string item5Url = await gameDataService.GetItemIconUrlAsync(item5);
                        string item6Url = await gameDataService.GetItemIconUrlAsync(item6);
                        
                        allPlayers.Add(new PlayerPerformanceResponse
                        {
                            SummonerName = summonerName,
                            Tagline = tagLine,
                            ChampionName = championName,
                            ChampionImageUrl = championIconUrl,
                            Kills = kills,
                            Deaths = deaths,
                            Assists = assists,
                            TeamPosition = teamPosition,
                            CS = (participant?.TotalMinionsKilled ?? 0) + (participant?.NeutralMinionsKilled ?? 0),
                            VisionScore = participant?.VisionScore ?? 0,
                            KDA = kdaText,
                            TeamId = teamId,
                            IsMainPlayer = isMainPlayer,
                            SubteamPlacement = subteamPlacement,
                            PlayerAugments = playerAugments.ToArray(),

                            summoner1Id = summoner1Id,
                            summoner2Id = summoner2Id,
                            Summoner1ImageUrl = summoner1Url,
                            Summoner2ImageUrl = summoner2Url,

                            item0 = item0,
                            item1 = item1,
                            item2 = item2,
                            item3 = item3,
                            item4 = item4,
                            item5 = item5,
                            item6 = item6,

                            Item0ImageUrl = item0Url,
                            Item1ImageUrl = item1Url,
                            Item2ImageUrl = item2Url,
                            Item3ImageUrl = item3Url,
                            Item4ImageUrl = item4Url,
                            Item5ImageUrl = item5Url,
                            Item6ImageUrl = item6Url
                        });
                    }
                }

                PlayerPerformanceResponse? mainPlayer = allPlayers.FirstOrDefault(p => p.IsMainPlayer);
                if (mainPlayer == null)
                {
                    Console.WriteLine($"❌ Main player not found! Searched PUUID: {playerPuuid}");
                    Console.WriteLine($"🔍 Available PUUIDs in match:");
                    if (participants != null)
                    {
                        foreach (RiotParticipant participant in participants)
                        {
                            string puuid = participant?.Puuid ?? "null";
                            string name = participant?.RiotIdGameName ?? participant?.SummonerName ?? "Unknown";
                            Console.WriteLine($"   - {puuid} ({name})");
                        }
                    }
                    return null;
                }

                RiotMatchInfo? info = matchData?.Info;
                int queueId = info?.QueueId ?? 0;
                string gameMode = GetGameModeFromQueueId(queueId);

                return new MatchSummaryResponse
                {
                    MatchId = matchData?.Metadata?.MatchId ?? "Unknown",
                    GameMode = gameMode,
                    GameDate = info?.GameStartTimestamp != null
                        ? DateTimeOffset.FromUnixTimeMilliseconds(info.GameStartTimestamp.Value).DateTime
                        : DateTime.MinValue,
                    GameDurationMinutes = info?.GameDuration != null ? (int)Math.Round((double)info.GameDuration.Value / 60) : 0,
                    Victory = playerParticipant?.Win ?? false,
                    MainPlayer = mainPlayer,
                    AllPlayers = allPlayers.ToArray()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error parsing match data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get game mode string from queue ID
        /// </summary>
        /// <param name="queueId">The queue ID</param>
        /// <returns>The game mode as a string</returns>
        private static string GetGameModeFromQueueId(int queueId)
        {
            return queueId switch
            {
                420 => "Ranked Solo/Duo",
                440 => "Ranked Flex",
                450 => "ARAM",
                400 => "Normal Draft",
                430 => "Normal Blind",
                1700 => "Arena",
                _ => "Custom Game"
            };
        }
    }
}
