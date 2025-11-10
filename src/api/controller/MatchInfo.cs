using Microsoft.AspNetCore.Mvc;
using api.models;
using api.service;
using api.helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchInfoController : ControllerBase
    {
        private readonly RIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;
        private readonly IGameDataService _gameDataService;

        public MatchInfoController(RIOTAPI riotApi, IChampionDataService championDataService, IGameDataService gameDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
            _gameDataService = gameDataService;
        }

        [HttpGet("{puuid}")]
        public async Task<ActionResult<MatchSummary[]>> GetMatchInfo(string puuid, [FromQuery] int start = 0, [FromQuery] int count = 10)
        {
            try
            {
                // Validate parameters
                if (start < 0)
                {
                    return BadRequest("Start index cannot be negative");
                }
                
                if (count < 1 || count > 100)
                {
                    return BadRequest("Count must be between 1 and 100");
                }

                // Step 1: Get match IDs for the PUUID with pagination
                string matchIdsJson = await _riotApi.GetMatchByPUUID(puuid, start, count);
                
                if (string.IsNullOrEmpty(matchIdsJson) || matchIdsJson == "[]")
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                // Step 2: Parse the match IDs array
                string[]? matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);
                
                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                // Step 3: Get details for all returned matches
                List<MatchSummary> matchSummaries = new List<MatchSummary>();
                
                // Console.WriteLine($"🎯 Processing {matchIds.Length} matches for PUUID: {puuid}");
                
                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        string matchId = matchIds[i];
                        // Line($"📊 Processing match {i + 1}/{matchesToProcess}: {matchId}");

                        string matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchId);
                        MatchSummary? matchSummary = await DeserializeMatchSummary(matchDetailsJson, puuid);

                        if (matchSummary != null)
                        {
                            matchSummaries.Add(matchSummary);
                            // Console.WriteLine($"✅ Successfully processed match {i + 1}: {(matchSummary.Victory ? "Victory" : "Defeat")}");
                            // Console.WriteLine($"   🏆 Main Player: {matchSummary.MainPlayer.ChampionName} ({matchSummary.MainPlayer.KDA})");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Failed to parse match {i + 1}: {matchId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing match {i + 1}: {ex.Message}");
                        // Continue processing other matches even if one fails
                        continue;
                    }
                }

                if (matchSummaries.Count == 0)
                {
                    return NotFound($"Could not parse any match details for PUUID '{puuid}'");
                }

                // Console.WriteLine($"🎉 Successfully processed {matchSummaries.Count} matches");
                return Ok(matchSummaries.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("summoner/{summonerName}/{tagline}")]
        public async Task<ActionResult<MatchSummary[]>> GetMatchInfoBySummoner(string summonerName, string tagline, [FromQuery] int start = 0, [FromQuery] int count = 10)
        {
            try
            {
                // Validate parameters
                if (start < 0)
                {
                    return BadRequest("Start index cannot be negative");
                }

                if (count < 1 || count > 100)
                {
                    return BadRequest("Count must be between 1 and 100");
                }

                string puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
                (string? puuid, string? gameName) = RiotApiDeserializer.DeserializePUUIDInfo(puuidData);

                if (string.IsNullOrEmpty(puuid))
                {
                    return NotFound($"Summoner '{summonerName}#{tagline}' not found");
                }

                string matchIdsJson = await _riotApi.GetMatchByPUUID(puuid, start, count);

                if (string.IsNullOrEmpty(matchIdsJson) || matchIdsJson == "[]")
                {
                    return NotFound($"No matches found for summoner '{summonerName}#{tagline}'");
                }

                // Step 3: Parse the match IDs array
                string[]? matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);

                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for summoner '{summonerName}#{tagline}'");
                }

                // Step 4: Get details for all returned matches
                List<MatchSummary> matchSummaries = new List<MatchSummary>();

                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        string matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchIds[i]);
                        MatchSummary? matchSummary = await DeserializeMatchSummary(matchDetailsJson, puuid);

                        if (matchSummary != null)
                        {
                            matchSummaries.Add(matchSummary);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Failed to process match {matchIds[i]}: {ex.Message}");
                        // Continue processing other matches
                    }
                }

                if (matchSummaries.Count == 0)
                {
                    return NotFound($"Could not parse any match details for summoner '{summonerName}#{tagline}'");
                }

                return Ok(matchSummaries.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<MatchSummary?> DeserializeMatchSummary(string jsonData, string playerPuuid)
        {
            Console.WriteLine($"[DEBUG] DeserializeMatchSummary called with playerPuuid: {playerPuuid}");
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return null;
                }

                // Deserialize to strongly-typed model
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
                    return null; // Player not found in this match
                }

                // Create list for all players
                List<PlayerPerformance> allPlayers = new List<PlayerPerformance>();
                
                // Extract all participant data
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

                        // Get Arena augments (only present in Arena mode)
                        List<int> playerAugments = new List<int>();
                        if (participant?.PlayerAugment1 != null) playerAugments.Add(participant.PlayerAugment1.Value);
                        if (participant?.PlayerAugment2 != null) playerAugments.Add(participant.PlayerAugment2.Value);
                        if (participant?.PlayerAugment3 != null) playerAugments.Add(participant.PlayerAugment3.Value);
                        if (participant?.PlayerAugment4 != null) playerAugments.Add(participant.PlayerAugment4.Value);

                        // Calculate KDA ratio like op.gg (K+A)/D
                        double kdaRatio = deaths > 0 ? Math.Round((double)(kills + assists) / deaths, 2) : kills + assists;
                        string kdaText = $"{kills}/{deaths}/{assists} ({kdaRatio}:1 KDA)";
                        // Generate champion icon URL using latest Data Dragon version
                        string version = await _championDataService.GetCurrentVersionAsync();
                        string championIconUrl = championName != "Unknown"
                            ? $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png"
                            : $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/Unknown.png";
                        bool isMainPlayer = puuid == playerPuuid;
                        // Get summoner spell IDs
                        int summoner1Id = participant?.Summoner1Id ?? 0;
                        int summoner2Id = participant?.Summoner2Id ?? 0;
                        // Get item IDs
                        int item0 = participant?.Item0 ?? 0;
                        int item1 = participant?.Item1 ?? 0;
                        int item2 = participant?.Item2 ?? 0;
                        int item3 = participant?.Item3 ?? 0;
                        int item4 = participant?.Item4 ?? 0;
                        int item5 = participant?.Item5 ?? 0;
                        int item6 = participant?.Item6 ?? 0;
                        // Fetch summoner spell and item URLs
                        string summoner1Url = await _gameDataService.GetSummonerSpellIconUrlAsync(summoner1Id);
                        string summoner2Url = await _gameDataService.GetSummonerSpellIconUrlAsync(summoner2Id);
                        string item0Url = await _gameDataService.GetItemIconUrlAsync(item0);
                        string item1Url = await _gameDataService.GetItemIconUrlAsync(item1);
                        string item2Url = await _gameDataService.GetItemIconUrlAsync(item2);
                        string item3Url = await _gameDataService.GetItemIconUrlAsync(item3);
                        string item4Url = await _gameDataService.GetItemIconUrlAsync(item4);
                        string item5Url = await _gameDataService.GetItemIconUrlAsync(item5);
                        string item6Url = await _gameDataService.GetItemIconUrlAsync(item6);
                        allPlayers.Add(new PlayerPerformance
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


                            // Summoner Spells
                            summoner1Id = summoner1Id,
                            summoner2Id = summoner2Id,
                            Summoner1ImageUrl = summoner1Url,
                            Summoner2ImageUrl = summoner2Url,

                            // Items
                            item0 = item0,
                            item1 = item1,
                            item2 = item2,
                            item3 = item3,
                            item4 = item4,
                            item5 = item5,
                            item6 = item6,

                            // Item URLs
                            Item0ImageUrl = item0Url,
                            Item1ImageUrl = item1Url,
                            Item2ImageUrl = item2Url,
                            Item3ImageUrl = item3Url,
                            Item4ImageUrl = item4Url,
                            Item5ImageUrl = item5Url,
                            Item6ImageUrl = item6Url
                        });
                        
                        // Console.WriteLine($"👤 Player: {summonerName} | Champion: {championName} | KDA: {kdaText} | Team: {teamId} | Main: {isMainPlayer}");
                    }
                }

                // Get main player data
                PlayerPerformance? mainPlayer = allPlayers.FirstOrDefault(p => p.IsMainPlayer);
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

                // Console.WriteLine($"✅ Main player found: {mainPlayer.SummonerName} playing {mainPlayer.ChampionName}");
                // Console.WriteLine($"📊 Total players in match: {allPlayers.Count}");

                // Determine game mode based on queue ID
                RiotMatchInfo? info = matchData?.Info;
                int queueId = info?.QueueId ?? 0;
                string gameMode = GetGameModeFromQueueId(queueId);

                return new MatchSummary
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

        private string GetGameModeFromQueueId(int queueId)
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