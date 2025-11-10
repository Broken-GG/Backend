using Microsoft.AspNetCore.Mvc;

namespace api.controller
{

    public class HealthDetails
    {
        public required string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Service { get; set; }
        public required string Version { get; set; }
        public required Checks Checks { get; set; }
    }

    public class Checks
    {
        public required RiotApiKey RiotApiKey { get; set; }
        public required EnvironmentCheck Environment { get; set; }
    }

    public class RiotApiKey
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
    }

    public class EnvironmentCheck
    {
        public required string Status { get; set; }
        public required string DotnetVersion { get; set; }
        public required string OsVersion { get; set; }
    }

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

            HealthDetails details = new HealthDetails
            {
                Status = apiKeyConfigured ? "Healthy" : "Degraded",
                Timestamp = DateTime.UtcNow,
                Service = "Broken.GG Backend API",
                Version = "1.0.0",
                Checks = new Checks
                {
                    RiotApiKey = new RiotApiKey
                    {
                        Status = apiKeyConfigured ? "OK" : "Missing",
                        Message = apiKeyConfigured ? "API key is configured" : "RIOT_API_KEY environment variable is not set"
                    },
                    Environment = new EnvironmentCheck
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
