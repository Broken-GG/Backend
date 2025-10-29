using Microsoft.AspNetCore.Mvc;

namespace api.controller
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
            var apiKeyConfigured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RIOT_API_KEY"));

            var details = new
            {
                status = apiKeyConfigured ? "Healthy" : "Degraded",
                timestamp = DateTime.UtcNow,
                service = "Broken.GG Backend API",
                version = "1.0.0",
                checks = new
                {
                    riotApiKey = new
                    {
                        status = apiKeyConfigured ? "OK" : "Missing",
                        message = apiKeyConfigured ? "API key is configured" : "RIOT_API_KEY environment variable is not set"
                    },
                    environment = new
                    {
                        status = "OK",
                        dotnetVersion = Environment.Version.ToString(),
                        osVersion = Environment.OSVersion.ToString()
                    }
                }
            };

            return apiKeyConfigured ? Ok(details) : StatusCode(503, details);
        }
    }
}
