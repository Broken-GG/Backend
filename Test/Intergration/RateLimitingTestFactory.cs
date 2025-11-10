using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Backend.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for rate limiting tests that:
/// - Enables rate limiting with strict limits
/// - Uses test configuration
/// </summary>
public class RateLimitingTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root to the actual project directory
        var projectDir = Directory.GetCurrentDirectory();
        var backendDir = Path.GetFullPath(Path.Combine(projectDir, "..", "..", ".."));
        
        builder.UseContentRoot(backendDir);
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration with STRICT rate limits
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Enable rate limiting with very low limits for testing
                ["RateLimiting:Enabled"] = "true",
                ["RateLimiting:RequestsPerMinute"] = "5", // Very low limit for testing
                
                // Keep other settings from .env
                ["RIOT_API_KEY"] = Environment.GetEnvironmentVariable("RIOT_API_KEY") ?? "test-api-key"
            });
        });
    }
}
