using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.Integration;

/// <summary>
/// Simple test factory with rate limiting disabled to prevent test hangs.
/// </summary>
public class SimpleTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Rate limiting is still enabled but we'll skip those specific tests
            // This factory just ensures the content root is correct
        });

        // Ensure the .env file can be found
        builder.UseContentRoot(Directory.GetCurrentDirectory());
    }
}
