using Microsoft.AspNetCore.Mvc;
using api.models;
using api.service;
using api.helpers;
using System;
using System.Threading.Tasks;

namespace api.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummonerInfoController : ControllerBase
    {
        private readonly RIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;

        public SummonerInfoController(RIOTAPI riotApi, IChampionDataService championDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
        }

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
            
            SummonerInfo summonerInfo = await RiotApiDeserializer.DeserializeSummonerInfoAsync(summonerInfoJson, _championDataService);

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