using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class RiotApiHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RiotApiHealthCheck> _logger;

        public RiotApiHealthCheck(IHttpClientFactory httpClientFactory, ILogger<RiotApiHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                
                // Check Data Dragon API (version endpoint)
                string versionUrl = "https://ddragon.leagueoflegends.com/api/versions.json";
                HttpResponseMessage response = await client.GetAsync(versionUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Riot API health check successful");
                    return HealthCheckResult.Healthy("Data Dragon API is reachable");
                }
                else
                {
                    _logger.LogWarning("Riot API health check failed with status code: {StatusCode}", response.StatusCode);
                    return HealthCheckResult.Degraded($"Data Dragon API returned status code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Riot API health check failed due to HTTP request exception");
                return HealthCheckResult.Unhealthy("Data Dragon API is unreachable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Riot API health check timed out");
                return HealthCheckResult.Unhealthy("Data Dragon API request timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Riot API health check failed with unexpected exception");
                return HealthCheckResult.Unhealthy("Unexpected error occurred during health check", ex);
            }
        }
    }
}
