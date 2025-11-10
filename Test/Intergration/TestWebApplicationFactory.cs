using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests that:
/// - Disables rate limiting
/// - Uses test configuration
/// - Sets correct content root path
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root to the actual project directory
        var projectDir = Directory.GetCurrentDirectory();
        var backendDir = Path.GetFullPath(Path.Combine(projectDir, "..", "..", ".."));
        
        builder.UseContentRoot(backendDir);
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Disable rate limiting for tests
                ["RateLimiting:Enabled"] = "false",
                ["RateLimiting:RequestsPerMinute"] = "1000000", // Effectively unlimited
                
                // Keep other settings from .env
                ["RIOT_API_KEY"] = Environment.GetEnvironmentVariable("RIOT_API_KEY") ?? "test-api-key"
            });
        });

        builder.ConfigureServices(services =>
        {
            // You can add additional test service configurations here
            // For example, mock external services, use in-memory cache, etc.
        });
    }
}
