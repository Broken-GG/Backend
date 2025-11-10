using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Backend.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures the test host to use the correct content root path.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the content root to the actual project directory
        var projectDir = Directory.GetCurrentDirectory();
        
        // Navigate up from Test\Intergration to the Backend root
        while (!File.Exists(Path.Combine(projectDir, "Program.cs")) && projectDir != null)
        {
            projectDir = Directory.GetParent(projectDir)?.FullName;
        }

        if (projectDir != null)
        {
            builder.UseContentRoot(projectDir);
        }

        builder.ConfigureServices(services =>
        {
            // You can override services here for testing
            // For example, you could replace the RIOT API with a mock
        });

        builder.UseEnvironment("Testing");
    }
}
