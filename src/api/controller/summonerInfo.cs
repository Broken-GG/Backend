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
    public async Task<ActionResult<SummonerInfo>> GetSummonerInfo(string summonerName, string tagline)
    {
        try
        {
            // First, get the PUUID using summoner name and tagline
            var puuidData = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
            var puuidAndNameInfo = DeserializePUUIDInfo(puuidData);
            
            if (string.IsNullOrEmpty(puuidAndNameInfo.PUUID))
            {
                return NotFound($"Summoner '{summonerName}#{tagline}' not found");
            }

            // Then, get summoner details using the PUUID
            var summonerData = await _riotApi.GetSummonerByName(puuidAndNameInfo.PUUID);
            var summonerInfo = DeserializeSummonerInfo(summonerData);
            
            // Set the name and tagline from the first API call
            summonerInfo.SummonerName = puuidAndNameInfo.GameName ?? summonerName;
            summonerInfo.Tagline = tagline;

            return Ok(summonerInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
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

    private SummonerInfo DeserializeSummonerInfo(string jsonData)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new ArgumentException("JSON data cannot be null or empty");
            }

            // Parse the JSON and map to SummonerInfo
            dynamic? data = JsonConvert.DeserializeObject(jsonData);
            
            return new SummonerInfo
            {
                SummonerName = data?.name ?? "Unknown",
                Tagline = "Unknown", // Will be set from the original request
                Level = data?.summonerLevel ?? 0,
                Region = "EU" // Default region for now
            };
        }
        catch (Exception)
        {
            // Return placeholder if deserialization fails
            return new SummonerInfo
            {
                SummonerName = "ExampleName",
                Tagline = "ExampleTagline",
                Level = 30,
                Region = "EU"
            };
        }
    }
}
}