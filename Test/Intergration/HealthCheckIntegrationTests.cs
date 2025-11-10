using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests.Integration;

/// <summary>
/// Integration tests for the health check endpoints.
/// Tests the health check system working with real dependencies.
/// </summary>
public class HealthCheckIntegrationTests : IClassFixture<SimpleTestFactory>
{
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(SimpleTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_BasicEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task HealthCheck_RiotApiEndpoint_ReturnsResponse()
    {
        // Act
        var response = await _client.GetAsync("/health/riot");

        // Assert
        // Should return either OK (Riot API is up) or ServiceUnavailable (Riot API is down)
        // Both are valid responses indicating the health check is working
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected 200 or 503, but got {response.StatusCode}"
        );
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task HealthCheck_ReturnsJsonResponse()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("{", content.TrimStart()); // Valid JSON starts with {
    }

    [Fact]
    public async Task HealthCheck_MultipleRequests_AllSucceed()
    {
        // Act - Make multiple requests in parallel
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/health"))
            .ToArray();
        
        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed
        Assert.All(responses, response => 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        );
    }

    [Fact]
    public async Task HealthCheck_ResponseTime_IsReasonable()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/health");
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(duration.TotalSeconds < 5, $"Health check took {duration.TotalSeconds}s, should be under 5s");
    }

    [Fact]
    public async Task HealthCheck_ContainsTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("timestamp", content.ToLower());
    }
}
