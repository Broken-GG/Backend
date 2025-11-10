using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs.Response;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for retrieving match information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IRIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;
        private readonly IGameDataService _gameDataService;
        
        public MatchController(IRIOTAPI riotApi, IChampionDataService championDataService, IGameDataService gameDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
            _gameDataService = gameDataService;
        }

        /// <summary>
        /// Get match information by PUUID
        /// </summary>
        /// <param name="puuid">The PUUID of the player</param>
        /// <param name="start">The start index for pagination</param>
        /// <param name="count">The number of matches to retrieve</param>
        /// <returns>A list of match summaries</returns>
        [HttpGet("{puuid}")]
        public async Task<ActionResult<MatchSummaryResponse[]>> GetMatchInfo(string puuid, [FromQuery] int start = 0, [FromQuery] int count = 10)
        {
            // Input validation
            if (!ValidationHelper.IsValidPuuid(puuid))
            {
                return BadRequest(new { message = "Invalid PUUID format" });
            }

            if (!ValidationHelper.IsValidPaginationStart(start))
            {
                return BadRequest(new { message = "Start index cannot be negative" });
            }

            if (!ValidationHelper.IsValidPaginationCount(count))
            {
                return BadRequest(new { message = "Count must be between 1 and 100" });
            }

            try
            {

                string matchIdsJson = await _riotApi.GetMatchByPUUID(puuid, start, count);

                if (string.IsNullOrEmpty(matchIdsJson) || matchIdsJson == "[]")
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                string[]? matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);

                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for PUUID '{puuid}'");
                }

                List<MatchSummaryResponse> matchSummaries = new List<MatchSummaryResponse>();

                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        string matchId = matchIds[i];

                        string matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchId);
                        MatchSummaryResponse? matchSummary = await MatchInfoHelper.DeserializeMatchSummary(
                            matchDetailsJson, 
                            puuid, 
                            _championDataService, 
                            _gameDataService
                        );

                        if (matchSummary != null)
                        {
                            matchSummaries.Add(matchSummary);
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Failed to parse match {i + 1}: {matchId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing match {i + 1}: {ex.Message}");
                        continue;
                    }
                }

                if (matchSummaries.Count == 0)
                {
                    return NotFound($"Could not parse any match details for PUUID '{puuid}'");
                }

                return Ok(matchSummaries.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get match information by summoner name and tagline
        /// </summary>
        /// <param name="summonerName">The summoner's name</param>
        /// <param name="tagline">The summoner's tagline</param>
        /// <param name="start">The start index for pagination</param>
        /// <param name="count">The number of matches to retrieve</param>
        /// <returns>A list of match summaries</returns>
        [HttpGet("summoner/{summonerName}/{tagline}")]
        public async Task<ActionResult<MatchSummaryResponse[]>> GetMatchInfoBySummoner(string summonerName, string tagline, [FromQuery] int start = 0, [FromQuery] int count = 10)
        {
            // Input validation
            if (!ValidationHelper.IsValidSummonerName(summonerName))
            {
                return BadRequest(new { message = "Invalid summoner name format" });
            }

            if (!ValidationHelper.IsValidTagline(tagline))
            {
                return BadRequest(new { message = "Invalid tagline format" });
            }

            if (!ValidationHelper.IsValidPaginationStart(start))
            {
                return BadRequest(new { message = "Start index cannot be negative" });
            }

            if (!ValidationHelper.IsValidPaginationCount(count))
            {
                return BadRequest(new { message = "Count must be between 1 and 100" });
            }

            try
            {

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

                string[]? matchIds = JsonConvert.DeserializeObject<string[]>(matchIdsJson);

                if (matchIds == null || matchIds.Length == 0)
                {
                    return NotFound($"No matches found for summoner '{summonerName}#{tagline}'");
                }

                List<MatchSummaryResponse> matchSummaries = new List<MatchSummaryResponse>();

                for (int i = 0; i < matchIds.Length; i++)
                {
                    try
                    {
                        string matchDetailsJson = await _riotApi.GetMatchDetailsByMatchId(matchIds[i]);
                        MatchSummaryResponse? matchSummary = await MatchInfoHelper.DeserializeMatchSummary(
                            matchDetailsJson,
                            puuid,
                            _championDataService,
                            _gameDataService
                        );

                        if (matchSummary != null)
                        {
                            matchSummaries.Add(matchSummary);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Failed to process match {matchIds[i]}: {ex.Message}");
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
    }
}
