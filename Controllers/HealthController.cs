using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs.Response;

namespace Backend.Controllers
{
    /// <summary>
    /// Health check endpoint for container orchestration and monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Status information</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "Broken.GG Backend API",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Detailed health check with dependencies
        /// </summary>
        /// <returns>Detailed status information</returns>
        [HttpGet("detailed")]
        public IActionResult GetDetailed()
        {
            // Check if Riot API key is configured
            bool apiKeyConfigured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RIOT_API_KEY"));

            HealthDetailsResponse details = new HealthDetailsResponse
            {
                Status = apiKeyConfigured ? "Healthy" : "Degraded",
                Timestamp = DateTime.UtcNow,
                Service = "Broken.GG Backend API",
                Version = "1.0.0",
                Checks = new HealthChecksResponse
                {
                    RiotApiKey = new RiotApiKeyHealthResponse
                    {
                        Status = apiKeyConfigured ? "OK" : "Missing",
                        Message = apiKeyConfigured ? "API key is configured" : "RIOT_API_KEY environment variable is not set"
                    },
                    Environment = new EnvironmentHealthResponse
                    {
                        Status = "OK",
                        DotnetVersion = Environment.Version.ToString(),
                        OsVersion = Environment.OSVersion.ToString()
                    }
                }
            };

            return apiKeyConfigured ? Ok(details) : StatusCode(503, details);
        }
    }
}
