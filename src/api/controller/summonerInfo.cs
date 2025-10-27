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

    public class SummonerInfoController : ControllerBase
{
    private readonly RIOTAPI _riotApi;
 
    public SummonerInfoController(RIOTAPI riotApi)
    {
        _riotApi = riotApi;
    }

    [HttpGet("{summonerName}/{tagline}")]
    public async Task<IActionResult> GetSummonerInfo(string summonerName, string tagline)
    {
        try
        {
            // Get PUUID and other summoner info
            var puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
            var puuidInfo = JsonConvert.DeserializeObject<dynamic>(puuidData);

            if (puuidInfo?.puuid == null)
            {
                return NotFound(new { message = "Summoner not found." });
            }

            string puuid = puuidInfo.puuid;

            // Get additional summoner info using the PUUID
            var summonerInfoJson = await _riotApi.GetSummonerByName(puuid);
            
            // USE THE HELPER METHOD INSTEAD OF DIRECT DESERIALIZATION
            var summonerInfo = await DeserializeSummonerInfo(summonerInfoJson);

            if (summonerInfo == null)
            {
                return NotFound(new { message = "Summoner info not found." });
            }

            // Include PUUID and names in the response
            summonerInfo.PUUID = puuid;
            summonerInfo.SummonerName = puuidInfo.gameName ?? summonerName;
            summonerInfo.Tagline = puuidInfo.tagLine ?? tagline;

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
    private (string? PUUID, string? GameName) DeserializePUUIDInfo(string jsonData)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                return (null, null);
            }

            // Parse the JSON to extract PUUID and gameName
            dynamic? data = JsonConvert.DeserializeObject(jsonData);
            return (data?.puuid, data?.gameName);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }

    private async Task<SummonerInfo> DeserializeSummonerInfo(string jsonData)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new ArgumentException("JSON data cannot be null or empty");
            }

            // Parse the JSON and map to SummonerInfo
            dynamic? data = JsonConvert.DeserializeObject(jsonData);
            
            // Get profile icon ID with fallback
            var profileIconId = (int)(data?.profileIconId ?? 0);
            var summonerLevel = (int)(data?.summonerLevel ?? 0);
            
            // Debug logging
            Console.WriteLine($"🔍 Profile Icon ID: {profileIconId}");
            Console.WriteLine($"🔍 Summoner Level: {summonerLevel}");
            
            // Get latest Data Dragon version
            var version = await ChampionDataService.GetCurrentVersionAsync();
            var profileIconUrl = string.Empty;
            
            if (profileIconId > 0)
            {
                profileIconUrl = $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/{profileIconId}.png";
            }
            else
            {
                profileIconUrl = $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/0.png";
            }
            
            Console.WriteLine($"🔍 Profile Icon URL: {profileIconUrl}");
            
            return new SummonerInfo
            {
                SummonerName = data?.name ?? "Unknown",
                Tagline = "Unknown", // Will be set from the original request
                Level = summonerLevel,
                Region = "EU", // Default region for now
                ProfileIconUrl = profileIconUrl,
                PUUID = "" // Will be set in the main method
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deserializing summoner info: {ex.Message}");
            
            // Get latest Data Dragon version for fallback
            var version = await ChampionDataService.GetCurrentVersionAsync();
            
            // Return placeholder if deserialization fails
            return new SummonerInfo
            {
                SummonerName = "ExampleName",
                Tagline = "ExampleTagline",
                Level = 30,
                Region = "EU",
                ProfileIconUrl = $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/0.png"
            };
        }
    }
}
}