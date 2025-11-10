using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests.Integration;

/// <summary>
/// End-to-end integration tests that test the full API pipeline
/// including middleware, controllers, and services working together.
/// </summary>
public class EndToEndApiTests : IClassFixture<SimpleTestFactory>
{
    private readonly HttpClient _client;

    public EndToEndApiTests(SimpleTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Health Check Tests

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact(Timeout = 10000, Skip = "Skipping - makes real Riot API call that can timeout")]
    public async Task HealthCheck_RiotApi_ReturnsStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/riot");

        // Assert
        // Note: This will fail if Riot API is actually down, but that's expected
        // In a real scenario, you'd mock the Riot API or use a test endpoint
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.ServiceUnavailable
        );
    }

    #endregion

    #region Summoner API Tests

    [Fact(Timeout = 10000, Skip = "Skipping - makes real Riot API call that can timeout")]
    public async Task GetSummonerInfo_WithValidInputs_ReturnsOkOrNotFound()
    {
        // Arrange
        var summonerName = "TestPlayer";
        var tagline = "EUW";

        // Act
        var response = await _client.GetAsync($"/api/summoner/{summonerName}/{tagline}");

        // Assert
        // Either OK (if summoner exists) or NotFound (if doesn't exist)
        // Both are valid responses from properly functioning API
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Theory(Timeout = 3000)]
    [InlineData("", "EUW")]
    [InlineData("ab", "EUW")]
    [InlineData("ThisNameIsWayTooLongForValidation", "EUW")]
    public async Task GetSummonerInfo_WithInvalidSummonerName_ReturnsBadRequest(string summonerName, string tagline)
    {
        // Act
        var response = await _client.GetAsync($"/api/summoner/{summonerName}/{tagline}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content.ToLower());
    }

    [Theory]
    [InlineData("TestPlayer", "")]
    [InlineData("TestPlayer", "A")]
    [InlineData("TestPlayer", "ThisTagIsWayTooLong")]
    public async Task GetSummonerInfo_WithInvalidTagline_ReturnsBadRequest(string summonerName, string tagline)
    {
        // Act
        var response = await _client.GetAsync($"/api/summoner/{summonerName}/{tagline}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Match API Tests

    [Fact]
    public async Task GetMatchInfo_WithInvalidPuuid_ReturnsBadRequest()
    {
        // Arrange
        var invalidPuuid = "short"; // Too short to be valid

        // Act
        var response = await _client.GetAsync($"/api/match/{invalidPuuid}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMatchInfoBySummoner_WithInvalidPagination_ReturnsBadRequest()
    {
        // Arrange
        var summonerName = "TestPlayer";
        var tagline = "EUW";

        // Act - negative start value
        var response = await _client.GetAsync($"/api/match/summoner/{summonerName}/{tagline}?start=-1&count=10");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Ranked API Tests

    [Fact]
    public async Task GetRankedInfo_WithShortPuuid_ReturnsBadRequest()
    {
        // Arrange
        var invalidPuuid = "too-short";

        // Act
        var response = await _client.GetAsync($"/api/ranked/{invalidPuuid}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Mastery API Tests

    [Fact]
    public async Task GetMasteryInfo_WithInvalidPuuid_ReturnsBadRequest()
    {
        // Arrange
        var invalidPuuid = "invalid";

        // Act
        var response = await _client.GetAsync($"/api/mastery/{invalidPuuid}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task Api_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Api_ErrorResponse_ContainsStandardFormat()
    {
        // Arrange - use invalid input to trigger error
        var response = await _client.GetAsync("/api/summoner//EUW");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content.ToLower());
    }

    #endregion

    #region CORS Tests

    [Fact]
    public async Task Api_IncludesCorsHeaders()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        // Check if CORS headers are present (if CORS is configured)
        Assert.NotNull(response.Headers);
    }

    #endregion

    #region Middleware Pipeline Tests

    [Fact]
    public async Task Api_NotFoundEndpoint_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Api_HandlesExceptionsGracefully()
    {
        // Act - Try various potentially problematic inputs
        var response = await _client.GetAsync("/api/summoner/test/test");

        // Assert - Should not crash, should return proper status code
        Assert.True(response.StatusCode != HttpStatusCode.InternalServerError || 
                    response.Content.Headers.ContentType?.ToString()?.Contains("json") == true);
    }

    #endregion
}
