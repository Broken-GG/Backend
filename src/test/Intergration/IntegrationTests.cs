using Xunit;
using Microsoft.AspNetCore.Mvc;
using api.models;
using api.controller;
using System.Threading.Tasks;
using api.service;

namespace api.test
{
    public class SummonerInfoIntegrationTests
    {
        [Fact]
        public async Task GetSummonerInfo_MockAPI_ReturnsCorrectData()
        {
            // Arrange mock services
            MockRiotAPI mockRiotApi = new MockRiotAPI();
            MockChampionDataService mockChampionDataService = new MockChampionDataService();
            SummonerInfoController controller = new SummonerInfoController(mockRiotApi, mockChampionDataService);

            // Act
            IActionResult result = await controller.GetSummonerInfo("MOCK_SUMMONER", "MOCK_TAGLINE");

            // Assert
            Assert.NotNull(result);
            if (result is OkObjectResult okResult)
            {
                SummonerInfo summoner = Assert.IsType<SummonerInfo>(okResult.Value);
                Assert.Equal("MOCK_SUMMONER", summoner.SummonerName);
                Assert.Equal("MOCK_TAGLINE", summoner.Tagline);
                Assert.Equal(30, summoner.Level);
            }
            else
            {
                throw new Xunit.Sdk.XunitException("Expected OkObjectResult but got " + result?.GetType().Name);
            }
        }
    }

    public class MockRiotAPI : api.service.RIOTAPI
    {
        public override Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline)
        {
            return Task.FromResult("{\"puuid\":\"MOCK_PUUID\"}");
        }

        public override Task<string> GetSummonerByName(string PUUID)
        {
            return Task.FromResult("{\"puuid\":\"MOCK_PUUID\",\"profileIconId\":1234,\"revisionDate\":1761668730000,\"summonerLevel\":30}");
        }
    }

    public class MockChampionDataService : api.service.IChampionDataService
    {
        public Task<string> GetCurrentVersionAsync()
        {
            return Task.FromResult("MOCK_VERSION");
        }

        public Task<string> GetChampionNameByIdAsync(long id)
        {
            return Task.FromResult("MockChampion");
        }

        public Task<string> GetChampionIconUrlAsync(long id, string? version)
        {
            return Task.FromResult("https://mockurl.com/champion-icon.png");
        }

        public Task<(string Name, string IconUrl)> GetChampionDataAsync(long id, string? version)
        {
            return Task.FromResult(("MockChampion", "https://mockurl.com/champion-icon.png"));
        }
    }
}

