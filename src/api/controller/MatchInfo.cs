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
                var matchIdsJson = await _riotApi.GetMatchByPUUID(puuid, start, count);
                
                if (string.IsNullOrEmpty(matchIdsJson) || matchIdsJson == "[]")
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                // Step 2: Parse the match IDs array
                var matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);
                
                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                // Step 3: Get details for all returned matches
                var matchSummaries = new List<MatchSummary>();
                
                // Console.WriteLine($"🎯 Processing {matchIds.Length} matches for PUUID: {puuid}");
                
                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        var matchId = matchIds[i];
                        // Line($"📊 Processing match {i + 1}/{matchesToProcess}: {matchId}");
                        
                        var matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchId);
                        var matchSummary = await DeserializeMatchSummary(matchDetailsJson, puuid);
                        
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

                // Step 1: Get PUUID using summoner name and tagline (keep it internal for security)
                var puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
                var (puuid, gameName) = RiotApiDeserializer.DeserializePUUIDInfo(puuidData);
                
                if (string.IsNullOrEmpty(puuid))
                {
                    return NotFound($"Summoner '{summonerName}#{tagline}' not found");
                }

                // Step 2: Get match IDs for the PUUID with pagination
                var matchIdsJson = await _riotApi.GetMatchByPUUID(puuid, start, count);
                
                if (string.IsNullOrEmpty(matchIdsJson) || matchIdsJson == "[]")
                {
                    return NotFound($"No matches found for summoner '{summonerName}#{tagline}'");
                }

                // Step 3: Parse the match IDs array
                var matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);
                
                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for summoner '{summonerName}#{tagline}'");
                }

                // Step 4: Get details for all returned matches
                var matchSummaries = new List<MatchSummary>();
                
                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        var matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchIds[i]);
                        var matchSummary = await DeserializeMatchSummary(matchDetailsJson, puuid);
                        
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

                // Parse the complex Riot match data structure
                dynamic? matchData = JsonConvert.DeserializeObject(jsonData);
                
                if (matchData?.info == null)
                {
                    return null;
                }

                var info = matchData.info;
                var participants = matchData.info.participants;
                
                // Find the specific player's data
                dynamic? playerParticipant = null;
                if (participants != null)
                {
                    foreach (var participant in participants)
                    {
                        if (participant?.puuid?.ToString() == playerPuuid)
                        {
                            playerParticipant = participant;
                            break;
                        }
                    }
                }

                if (playerParticipant == null)
                {
                    return null; // Player not found in this match
                }

                // Create list for all players
                var allPlayers = new List<PlayerPerformance>();
                
                // Extract all participant data
                if (participants != null)
                {
                    foreach (var participant in participants)
                    {
                        var puuid = participant?.puuid?.ToString() ?? "";
                        // Try multiple sources for summoner name in order of preference
                        var summonerName = !string.IsNullOrEmpty(participant?.riotIdGameName?.ToString())
                            ? participant?.riotIdGameName?.ToString() ?? "Unknown Player"
                            : !string.IsNullOrEmpty(participant?.summonerName?.ToString())
                                ? participant?.summonerName?.ToString() ?? "Unknown Player"
                                : "Unknown Player";
                        var tagLine = !string.IsNullOrEmpty(participant?.riotIdTagline?.ToString())
                            ? participant?.riotIdTagline?.ToString() ?? ""
                            : !string.IsNullOrEmpty(participant?.summonerTagline?.ToString())
                                ? participant?.summonerTagline?.ToString() ?? ""
                                : "Unknown Tagline";
                        var championName = participant?.championName?.ToString() ?? "Unknown";
                        var kills = (int)(participant?.kills ?? 0);
                        var deaths = (int)(participant?.deaths ?? 0);
                        var assists = (int)(participant?.assists ?? 0);
                        var teamId = (int)(participant?.teamId ?? 0);
                        var teamPosition = participant?.teamPosition?.ToString() ?? "Unknown";
                        var subteamPlacement = (int)(participant?.subteamPlacement ?? 0); // Arena mode only
                        
                        // Get Arena augments (only present in Arena mode)
                        var playerAugments = new List<int>();
                        if (participant?.playerAugment1 != null) playerAugments.Add((int)participant.playerAugment1);
                        if (participant?.playerAugment2 != null) playerAugments.Add((int)participant.playerAugment2);
                        if (participant?.playerAugment3 != null) playerAugments.Add((int)participant.playerAugment3);
                        if (participant?.playerAugment4 != null) playerAugments.Add((int)participant.playerAugment4);
                        
                        // Calculate KDA ratio like op.gg (K+A)/D
                        var kdaRatio = deaths > 0 ? Math.Round((double)(kills + assists) / deaths, 2) : kills + assists;
                        var kdaText = $"{kills}/{deaths}/{assists} ({kdaRatio}:1 KDA)";
                        // Generate champion icon URL using latest Data Dragon version
                        var version = await _championDataService.GetCurrentVersionAsync();
                        var championIconUrl = championName != "Unknown"
                            ? $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/{championName}.png"
                            : $"https://ddragon.leagueoflegends.com/cdn/{version}/img/champion/Unknown.png";
                        var isMainPlayer = puuid == playerPuuid;
                        // Get summoner spell IDs
                        var summoner1Id = (int)(participant?.summoner1Id ?? 0);
                        var summoner2Id = (int)(participant?.summoner2Id ?? 0);
                        // Get item IDs
                        var item0 = (int)(participant?.item0 ?? 0);
                        var item1 = (int)(participant?.item1 ?? 0);
                        var item2 = (int)(participant?.item2 ?? 0);
                        var item3 = (int)(participant?.item3 ?? 0);
                        var item4 = (int)(participant?.item4 ?? 0);
                        var item5 = (int)(participant?.item5 ?? 0);
                        var item6 = (int)(participant?.item6 ?? 0);
                        // Fetch summoner spell and item URLs
                        var summoner1Url = await _gameDataService.GetSummonerSpellIconUrlAsync(summoner1Id);
                        var summoner2Url = await _gameDataService.GetSummonerSpellIconUrlAsync(summoner2Id);
                        var item0Url = await _gameDataService.GetItemIconUrlAsync(item0);
                        var item1Url = await _gameDataService.GetItemIconUrlAsync(item1);
                        var item2Url = await _gameDataService.GetItemIconUrlAsync(item2);
                        var item3Url = await _gameDataService.GetItemIconUrlAsync(item3);
                        var item4Url = await _gameDataService.GetItemIconUrlAsync(item4);
                        var item5Url = await _gameDataService.GetItemIconUrlAsync(item5);
                        var item6Url = await _gameDataService.GetItemIconUrlAsync(item6);
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
                            CS = (int)(participant?.totalMinionsKilled ?? 0) + (int)(participant?.neutralMinionsKilled ?? 0),
                            VisionScore = (int)(participant?.visionScore ?? 0),
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
                var mainPlayer = allPlayers.FirstOrDefault(p => p.IsMainPlayer);
                if (mainPlayer == null)
                {
                    Console.WriteLine($"❌ Main player not found! Searched PUUID: {playerPuuid}");
                    Console.WriteLine($"🔍 Available PUUIDs in match:");
                    if (participants != null)
                    {
                        foreach (var participant in participants)
                        {
                            var puuid = participant?.puuid?.ToString() ?? "null";
                            var name = participant?.riotIdGameName?.ToString() ?? participant?.summonerName?.ToString() ?? "Unknown";
                            Console.WriteLine($"   - {puuid} ({name})");
                        }
                    }
                    return null;
                }

                // Console.WriteLine($"✅ Main player found: {mainPlayer.SummonerName} playing {mainPlayer.ChampionName}");
                // Console.WriteLine($"📊 Total players in match: {allPlayers.Count}");

                // Determine game mode based on queue ID
                var queueId = (int)(info?.queueId ?? 0);
                var gameMode = GetGameModeFromQueueId(queueId);

                return new MatchSummary
                {
                    MatchId = matchData?.metadata?.matchId ?? "Unknown",
                    GameMode = gameMode,
                    GameDate = info?.gameStartTimestamp != null 
                        ? DateTimeOffset.FromUnixTimeMilliseconds((long)info.gameStartTimestamp).DateTime 
                        : DateTime.MinValue,
                    GameDurationMinutes = info?.gameDuration != null ? (int)Math.Round((double)info.gameDuration / 60) : 0,
                    Victory = playerParticipant?.win ?? false,
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