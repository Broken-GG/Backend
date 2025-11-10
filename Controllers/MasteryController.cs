using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs.Response;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    /// <summary>
    /// Controller for champion mastery information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MasteryController : ControllerBase
    {
        private readonly IRIOTAPI _riotApi;
        private readonly IChampionDataService _championDataService;

        public MasteryController(IRIOTAPI riotApi, IChampionDataService championDataService)
        {
            _riotApi = riotApi;
            _championDataService = championDataService;
        }

        /// <summary>
        /// Get mastery information by PUUID
        /// </summary>
        /// <param name="puuid">The PUUID of the player</param>
        /// <returns>Mastery information</returns>
        [HttpGet("{puuid}")]
        public async Task<IActionResult> GetMasteryInfo(string puuid)
        {
            // Input validation
            if (!ValidationHelper.IsValidPuuid(puuid))
            {
                return BadRequest(new { message = "Invalid PUUID format" });
            }

            try
            {
                string masteryInfoJson = await _riotApi.GetMasteryInfoByPUUID(puuid);
                MasteryInfoResponse[]? masteryInfoArray = JsonConvert.DeserializeObject<MasteryInfoResponse[]>(masteryInfoJson);

                if (masteryInfoArray == null || masteryInfoArray.Length == 0)
                {
                    return NotFound(new { message = "Mastery info not found." });
                }

                // Populate champion names and icon URLs using the ChampionDataService
                foreach (MasteryInfoResponse mastery in masteryInfoArray)
                {
                    (string Name, string IconUrl) championData = await _championDataService.GetChampionDataAsync(mastery.ChampionId);
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
