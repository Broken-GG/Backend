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
    public class SidePanelInfoController : ControllerBase
    {
        private readonly RIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;

        public SidePanelInfoController(RIOTAPI riotApi, IChampionDataService championDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
        }

        [HttpGet("ranked/{puuid}")]
        public async Task<IActionResult> GetRankedInfo(string puuid)
        {
            try
            {
                var rankedInfoJson = await _riotApi.GetRankedInfoByPUUID(puuid);
                var rankedInfo = JsonConvert.DeserializeObject<RankedInfo[]>(rankedInfoJson);

                if (rankedInfo == null || rankedInfo.Length == 0)
                {
                    return NotFound(new { message = "Ranked info not found." });
                }

                return Ok(rankedInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching ranked info: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching ranked info." });
            }
        }

        [HttpGet("mastery/{puuid}")]
        public async Task<IActionResult> GetMasteryInfo(string puuid)
        {
            try
            {
                var masteryInfoJson = await _riotApi.GetMasteryInfoByPUUID(puuid);
                var masteryInfoArray = JsonConvert.DeserializeObject<MasteryInfo[]>(masteryInfoJson);

                if (masteryInfoArray == null || masteryInfoArray.Length == 0)
                {
                    return NotFound(new { message = "Mastery info not found." });
                }

                // Populate champion names and icon URLs using the ChampionDataService
                foreach (var mastery in masteryInfoArray)
                {
                    var championData = await _championDataService.GetChampionDataAsync(mastery.ChampionId);
                    mastery.ChampionName = championData.Name;
                    mastery.ChampionIconUrl = championData.IconUrl;
                }

                return Ok(masteryInfoArray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching mastery info: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching mastery info." });
            }
        }
    }
}
