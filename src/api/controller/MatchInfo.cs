using Microsoft.AspNetCore.Mvc;
using api.models;
using api.service;
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

        public MatchInfoController(RIOTAPI riotApi)
        {
            _riotApi = riotApi;
        }

        [HttpGet("{puuid}")]
        public async Task<ActionResult<MatchSummary[]>> GetMatchInfo(string puuid)
        {
            try
            {
                // Step 1: Get match IDs for the PUUID (last 10 matches)
                var matchIdsJson = await _riotApi.GetMatchByPUUID(puuid);
                
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

                // Step 3: Get details for the last 10 matches (or however many are available)
                var matchSummaries = new List<MatchSummary>();
                var matchesToProcess = Math.Min(10, matchIds.Length); // Take up to 10 matches
                
                // Console.WriteLine($"🎯 Processing {matchesToProcess} matches for PUUID: {puuid}");
                
                for (int i = 0; i < matchesToProcess; i++)
                {
                    try
                    {
                        var matchId = matchIds[i];
                        // Line($"📊 Processing match {i + 1}/{matchesToProcess}: {matchId}");
                        
                        var matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchId);
                        var matchSummary = DeserializeMatchSummary(matchDetailsJson, puuid);
                        
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
        public async Task<ActionResult<MatchSummary[]>> GetMatchInfoBySummoner(string summonerName, string tagline)
        {
            try
            {
                // Step 1: Get PUUID using summoner name and tagline (keep it internal for security)
                var puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
                var puuidAndNameInfo = DeserializePUUIDInfo(puuidData);
                
                if (string.IsNullOrEmpty(puuidAndNameInfo.PUUID))
                {
                    return NotFound($"Summoner '{summonerName}#{tagline}' not found");
                }

                // Step 2: Get match IDs for the PUUID (last 10 matches)
                var matchIdsJson = await _riotApi.GetMatchByPUUID(puuidAndNameInfo.PUUID);
                
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

                // Step 4: Get details for multiple matches (up to 10)
                var matchSummaries = new List<MatchSummary>();
                var matchesToProcess = Math.Min(matchIds.Length, 10); // Limit to 10 matches
                
                for (int i = 0; i < matchesToProcess; i++)
                {
                    try
                    {
                        var matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchIds[i]);
                        var matchSummary = DeserializeMatchSummary(matchDetailsJson, puuidAndNameInfo.PUUID);
                        
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

        private MatchSummary? DeserializeMatchSummary(string jsonData, string playerPuuid)
        {
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
                        var summonerName = "";
                        if (!string.IsNullOrEmpty(participant?.riotIdGameName?.ToString()))
                        {
                            summonerName = participant?.riotIdGameName?.ToString() ?? "Unknown Player";
                        }
                        else if (!string.IsNullOrEmpty(participant?.summonerName?.ToString()))
                        {
                            summonerName = participant?.summonerName?.ToString() ?? "Unknown Player";
                        }
                        else
                        {
                            summonerName = "Unknown Player";
                        }
                        
                        var championName = participant?.championName?.ToString() ?? "Unknown";
                        var kills = (int)(participant?.kills ?? 0);
                        var deaths = (int)(participant?.deaths ?? 0);
                        var assists = (int)(participant?.assists ?? 0);
                        var teamId = (int)(participant?.teamId ?? 0);
                        
                        // Calculate KDA ratio like op.gg (K+A)/D
                        var kdaRatio = deaths > 0 ? Math.Round((double)(kills + assists) / deaths, 2) : kills + assists;
                        var kdaText = $"{kills}/{deaths}/{assists} ({kdaRatio}:1 KDA)";
                        
                        // Generate champion icon URL using Data Dragon CDN
                        var championIconUrl = championName != "Unknown" 
                            ? $"https://ddragon.leagueoflegends.com/cdn/14.20.1/img/champion/{championName}.png"
                            : "https://ddragon.leagueoflegends.com/cdn/14.20.1/img/champion/Unknown.png";
                        
                        var isMainPlayer = puuid == playerPuuid;
                        
                        allPlayers.Add(new PlayerPerformance
                        {
                            SummonerName = summonerName,
                            ChampionName = championName,
                            ChampionImageUrl = championIconUrl,
                            Kills = kills,
                            Deaths = deaths,
                            Assists = assists,
                            CS = (int)(participant?.totalMinionsKilled ?? 0) + (int)(participant?.neutralMinionsKilled ?? 0),
                            VisionScore = (int)(participant?.visionScore ?? 0),
                            KDA = kdaText,
                            TeamId = teamId,
                            IsMainPlayer = isMainPlayer,

                            // Summoner Spells
                            summoner1Id = participant?.summoner1Id ?? 0,
                            summoner2Id = participant?.summoner2Id ?? 0,

                            // Items
                            item0 = participant?.item0 ?? 0,
                            item1 = participant?.item1 ?? 0,
                            item2 = participant?.item2 ?? 0,
                            item3 = participant?.item3 ?? 0,
                            item4 = participant?.item4 ?? 0,
                            item5 = participant?.item5 ?? 0,
                            item6 = participant?.item6 ?? 0
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
                            // Console.WriteLine($"   - {puuid} ({name})");
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
        private (string? PUUID, string? GameName) DeserializePUUIDInfo(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return (null, null);
                }

                var puuidInfo = JsonConvert.DeserializeObject<dynamic>(jsonData);
                string? puuid = puuidInfo?.puuid;
                string? gameName = puuidInfo?.gameName;

                return (puuid, gameName);
            }
            catch (JsonException)
            {
                return (null, null);
            }
        }
        private SummonerInfo DeserializeSummonerInfo(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return new SummonerInfo();
                }

                var summonerInfo = JsonConvert.DeserializeObject<SummonerInfo>(jsonData);
                return summonerInfo ?? new SummonerInfo();
            }
            catch (JsonException)
            {
                return new SummonerInfo();
            }
        }
    }

}