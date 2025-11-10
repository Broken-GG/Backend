using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs.Response;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Helpers;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for summoner information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SummonerController : ControllerBase
    {
        private readonly RIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;

        public SummonerController(RIOTAPI riotApi, IChampionDataService championDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
        }

        /// <summary>
        /// Get summoner information by name and tagline
        /// </summary>
        /// <param name="summonerName">The summoner's name</param>
        /// <param name="tagline">The summoner's tagline</param>
        /// <returns>Summoner information</returns>
        [HttpGet("{summonerName}/{tagline}")]
        public async Task<IActionResult> GetSummonerInfo(string summonerName, string tagline)
        {
            try
            {
                string puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
                (string? puuid, string? gameName) = RiotApiDeserializer.DeserializePUUIDInfo(puuidData);

                if (string.IsNullOrEmpty(puuid))
                {
                    return NotFound(new { message = "Summoner not found." });
                }

                string summonerInfoJson = await _riotApi.GetSummonerByName(puuid);
                
                SummonerResponse summonerInfo = await RiotApiDeserializer.DeserializeSummonerInfoAsync(summonerInfoJson, _championDataService);

                if (summonerInfo == null)
                {
                    return NotFound(new { message = "Summoner info not found." });
                }

                summonerInfo.PUUID = puuid;
                summonerInfo.SummonerName = gameName ?? summonerName;
                summonerInfo.Tagline = tagline;

                // Debug logging to see what we're sending
                Console.WriteLine($"📤 Sending to frontend:");
                Console.WriteLine($"   - Summoner Name: {summonerInfo.SummonerName}");
                Console.WriteLine($"   - Tagline: {summonerInfo.Tagline}");
                Console.WriteLine($"   - Level: {summonerInfo.Level}");
                Console.WriteLine($"   - Profile Icon URL: {summonerInfo.ProfileIconUrl}");
                Console.WriteLine($"   - PUUID: {summonerInfo.PUUID}");
                Console.WriteLine($"   - Region: {summonerInfo.Region}");

                return Ok(summonerInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching summoner info: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching summoner info." });
            }
        }
    }
}
