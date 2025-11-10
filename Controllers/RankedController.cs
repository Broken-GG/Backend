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
    /// Controller for ranked information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RankedController : ControllerBase
    {
        private readonly IRIOTAPI _riotApi;

        public RankedController(IRIOTAPI riotApi)
        {
            _riotApi = riotApi;
        }

        /// <summary>
        /// Get ranked information by PUUID
        /// </summary>
        /// <param name="puuid">The PUUID of the player</param>
        /// <returns>Ranked information</returns>
        [HttpGet("{puuid}")]
        public async Task<IActionResult> GetRankedInfo(string puuid)
        {
            // Input validation
            if (!ValidationHelper.IsValidPuuid(puuid))
            {
                return BadRequest(new { message = "Invalid PUUID format" });
            }

            try
            {
                string rankedInfoJson = await _riotApi.GetRankedInfoByPUUID(puuid);
                RankedInfoResponse[]? rankedInfo = JsonConvert.DeserializeObject<RankedInfoResponse[]>(rankedInfoJson);

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
    }
}
