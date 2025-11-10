using Xunit;
using Moq;
using Backend.Controllers;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Models.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Backend.Tests.Unit.Controllers
{
    public class MatchControllerTest
    {
        private const string ValidPuuid = "test-puuid-1234567890-abcdef-1234567890-abcdef-1234567890";
        private const string ValidPuuidNoMatches = "test-puuid-no-matches-1234567890-abcdef-1234567890-abc";
        private const string ValidPuuidError = "test-puuid-error-case-1234567890-abcdef-1234567890-ab";
        private const string ValidMatchId = "EUW1_1234567890";

        private Mock<IRIOTAPI> CreateMockRiotApi() => new Mock<IRIOTAPI>();
        private Mock<IChampionDataService> CreateMockChampionService() => new Mock<IChampionDataService>();
        private Mock<IGameDataService> CreateMockGameDataService() => new Mock<IGameDataService>();

        private MatchController CreateController(
            Mock<IRIOTAPI> mockRiotApi,
            Mock<IChampionDataService> mockChampionService,
            Mock<IGameDataService> mockGameDataService)
        {
            return new MatchController(mockRiotApi.Object, mockChampionService.Object, mockGameDataService.Object);
        }
        /// <summary>
        /// Test 1: Valid PUUID should return 200 OK with match summaries
        /// </summary>
        [Fact]
        public async Task GetMatchInfo_ValidPuuid_ReturnsOkWithMatchData()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string mockMatchIdsJson = @"[""" + ValidMatchId + @"""]";
            string mockMatchDetailsJson = @"{
                ""metadata"": { ""matchId"": """ + ValidMatchId + @""" },
                ""info"": {
                    ""gameDuration"": 1800,
                    ""gameMode"": ""CLASSIC"",
                    ""participants"": [
                        {
                            ""puuid"": """ + ValidPuuid + @""",
                            ""championId"": 157,
                            ""kills"": 10,
                            ""deaths"": 2,
                            ""assists"": 5,
                            ""win"": true
                        }
                    ]
                }
            }";

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuid, 0, 10))
                .ReturnsAsync(mockMatchIdsJson);

            mockRiotApi
                .Setup(x => x.GetMatchDetailsByMatchId(ValidMatchId))
                .ReturnsAsync(mockMatchDetailsJson);

            mockChampionService
                .Setup(x => x.GetChampionDataAsync(157, null))
                .ReturnsAsync(("Yasuo", "http://ddragon.leagueoflegends.com/cdn/14.20.1/img/champion/Yasuo.png"));

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfo(ValidPuuid);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            MatchSummaryResponse[] matches = Assert.IsType<MatchSummaryResponse[]>(okResult.Value);
            Assert.Single(matches);
            Assert.Equal(ValidMatchId, matches[0].MatchId);
        }
        /// <summary>
        /// Test 2: Invalid PUUID should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData("abc")] // Too short
        public async Task GetMatchInfo_InvalidPuuid_ReturnsBadRequest(string invalidPuuid)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfo(invalidPuuid);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Equal("Invalid PUUID format", message);
        }

        /// <summary>
        /// Test 3: Invalid pagination parameters should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData(-1, 10)] // Negative start
        [InlineData(0, 0)]   // Count too low
        [InlineData(0, 101)] // Count too high
        public async Task GetMatchInfo_InvalidPagination_ReturnsBadRequest(int start, int count)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfo(ValidPuuid, start, count);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
        }

        /// <summary>
        /// Test 4: No matches found should return 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetMatchInfo_NoMatchesFound_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuidNoMatches, 0, 10))
                .ReturnsAsync("[]"); // Empty array

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfo(ValidPuuidNoMatches);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No matches found", notFound.Value?.ToString());
        }
        /// <summary>
        /// Test 5: API throws exception should return 500 Internal Server Error
        /// </summary>
        [Fact]
        public async Task GetMatchInfo_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuidError, 0, 10))
                .ThrowsAsync(new Exception("API connection failed"));

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfo(ValidPuuidError);

            // Assert
            ObjectResult serverError = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, serverError.StatusCode);
        }

        /// <summary>
        /// Test 6: Verify RIOTAPI is called to fetch match list and match details
        /// </summary>
        [Fact]
        public async Task GetMatchInfo_ValidPuuid_CallsRiotApiCorrectly()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string mockMatchIdsJson = @"[""" + ValidMatchId + @"""]";
            string mockMatchDetailsJson = @"{
                ""metadata"": { ""matchId"": """ + ValidMatchId + @""" },
                ""info"": {
                    ""gameDuration"": 1800,
                    ""gameMode"": ""CLASSIC"",
                    ""participants"": [
                        {
                            ""puuid"": """ + ValidPuuid + @""",
                            ""championId"": 157,
                            ""kills"": 10,
                            ""deaths"": 2,
                            ""assists"": 5,
                            ""win"": true
                        }
                    ]
                }
            }";

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuid, 0, 10))
                .ReturnsAsync(mockMatchIdsJson);

            mockRiotApi
                .Setup(x => x.GetMatchDetailsByMatchId(ValidMatchId))
                .ReturnsAsync(mockMatchDetailsJson);

            mockChampionService
                .Setup(x => x.GetCurrentVersionAsync())
                .ReturnsAsync("14.20.1");

            // Act
            await controller.GetMatchInfo(ValidPuuid);

            // Assert - Verify RIOT API was called correctly
            mockRiotApi.Verify(x => x.GetMatchByPUUID(ValidPuuid, 0, 10), Times.Once);
            mockRiotApi.Verify(x => x.GetMatchDetailsByMatchId(ValidMatchId), Times.Once);
        }

        #region GetMatchInfoBySummoner Tests

        /// <summary>
        /// Test 7: Valid summoner name and tagline should return 200 OK with match summaries
        /// </summary>
        [Fact]
        public async Task GetMatchInfoBySummoner_ValidSummonerAndTagline_ReturnsOkWithMatchData()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string summonerName = "TestPlayer";
            string tagline = "EUW";

            string mockPuuidJson = @"{
                ""puuid"": """ + ValidPuuid + @""",
                ""gameName"": """ + summonerName + @""",
                ""tagLine"": """ + tagline + @"""
            }";

            string mockMatchIdsJson = @"[""" + ValidMatchId + @"""]";
            string mockMatchDetailsJson = @"{
                ""metadata"": { ""matchId"": """ + ValidMatchId + @""" },
                ""info"": {
                    ""gameDuration"": 1800,
                    ""gameMode"": ""CLASSIC"",
                    ""participants"": [
                        {
                            ""puuid"": """ + ValidPuuid + @""",
                            ""championId"": 157,
                            ""kills"": 10,
                            ""deaths"": 2,
                            ""assists"": 5,
                            ""win"": true
                        }
                    ]
                }
            }";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline))
                .ReturnsAsync(mockPuuidJson);

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuid, 0, 10))
                .ReturnsAsync(mockMatchIdsJson);

            mockRiotApi
                .Setup(x => x.GetMatchDetailsByMatchId(ValidMatchId))
                .ReturnsAsync(mockMatchDetailsJson);

            mockChampionService
                .Setup(x => x.GetCurrentVersionAsync())
                .ReturnsAsync("14.20.1");

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner(summonerName, tagline);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            MatchSummaryResponse[] matches = Assert.IsType<MatchSummaryResponse[]>(okResult.Value);
            Assert.Single(matches);
            Assert.Equal(ValidMatchId, matches[0].MatchId);
        }

        /// <summary>
        /// Test 8: Invalid summoner name should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        public async Task GetMatchInfoBySummoner_InvalidSummonerName_ReturnsBadRequest(string invalidSummonerName)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner(invalidSummonerName, "EUW");

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Equal("Invalid summoner name format", message);
        }

        /// <summary>
        /// Test 9: Invalid tagline should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        public async Task GetMatchInfoBySummoner_InvalidTagline_ReturnsBadRequest(string invalidTagline)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner("TestPlayer", invalidTagline);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Equal("Invalid tagline format", message);
        }

        /// <summary>
        /// Test 10: Invalid pagination parameters should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData(-1, 10)] // Negative start
        [InlineData(0, 0)]   // Count too low
        [InlineData(0, 101)] // Count too high
        public async Task GetMatchInfoBySummoner_InvalidPagination_ReturnsBadRequest(int start, int count)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner("TestPlayer", "EUW", start, count);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequest.Value);
        }

        /// <summary>
        /// Test 11: Summoner not found should return 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetMatchInfoBySummoner_SummonerNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string summonerName = "NotFound";
            string tagline = "EUW";

            string mockPuuidJson = ""; // Empty response when summoner not found

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline))
                .ReturnsAsync(mockPuuidJson);

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner(summonerName, tagline);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value?.ToString());
        }

        /// <summary>
        /// Test 12: Valid summoner but no matches should return 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetMatchInfoBySummoner_NoMatchesFound_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string summonerName = "NewPlayer";
            string tagline = "EUW";

            string mockPuuidJson = @"{
                ""puuid"": """ + ValidPuuidNoMatches + @""",
                ""gameName"": """ + summonerName + @""",
                ""tagLine"": """ + tagline + @"""
            }";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline))
                .ReturnsAsync(mockPuuidJson);

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuidNoMatches, 0, 10))
                .ReturnsAsync("[]"); // Empty match list

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner(summonerName, tagline);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No matches found", notFound.Value?.ToString());
        }

        /// <summary>
        /// Test 13: API exception should return 500 Internal Server Error
        /// </summary>
        [Fact]
        public async Task GetMatchInfoBySummoner_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string summonerName = "ErrorPlayer";
            string tagline = "EUW";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline))
                .ThrowsAsync(new Exception("API connection failed"));

            // Act
            ActionResult<MatchSummaryResponse[]> result = await controller.GetMatchInfoBySummoner(summonerName, tagline);

            // Assert
            ObjectResult serverError = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, serverError.StatusCode);
        }

        /// <summary>
        /// Test 14: Verify RIOT API is called with correct summoner name and tagline
        /// </summary>
        [Fact]
        public async Task GetMatchInfoBySummoner_ValidSummoner_CallsRiotApiCorrectly()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var mockGameDataService = CreateMockGameDataService();
            var controller = CreateController(mockRiotApi, mockChampionService, mockGameDataService);

            string summonerName = "TestPlayer";
            string tagline = "EUW";

            string mockPuuidJson = @"{
                ""puuid"": """ + ValidPuuid + @""",
                ""gameName"": """ + summonerName + @""",
                ""tagLine"": """ + tagline + @"""
            }";

            string mockMatchIdsJson = @"[""" + ValidMatchId + @"""]";
            string mockMatchDetailsJson = @"{
                ""metadata"": { ""matchId"": """ + ValidMatchId + @""" },
                ""info"": {
                    ""gameDuration"": 1800,
                    ""gameMode"": ""CLASSIC"",
                    ""participants"": [
                        {
                            ""puuid"": """ + ValidPuuid + @""",
                            ""championId"": 157,
                            ""kills"": 10,
                            ""deaths"": 2,
                            ""assists"": 5,
                            ""win"": true
                        }
                    ]
                }
            }";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline))
                .ReturnsAsync(mockPuuidJson);

            mockRiotApi
                .Setup(x => x.GetMatchByPUUID(ValidPuuid, 0, 10))
                .ReturnsAsync(mockMatchIdsJson);

            mockRiotApi
                .Setup(x => x.GetMatchDetailsByMatchId(ValidMatchId))
                .ReturnsAsync(mockMatchDetailsJson);

            mockChampionService
                .Setup(x => x.GetCurrentVersionAsync())
                .ReturnsAsync("14.20.1");

            // Act
            await controller.GetMatchInfoBySummoner(summonerName, tagline);

            // Assert - Verify all API calls were made correctly
            mockRiotApi.Verify(x => x.GetPUUIDBySummonerNameAndTagline(summonerName, tagline), Times.Once);
            mockRiotApi.Verify(x => x.GetMatchByPUUID(ValidPuuid, 0, 10), Times.Once);
            mockRiotApi.Verify(x => x.GetMatchDetailsByMatchId(ValidMatchId), Times.Once);
        }

        #endregion
    }
}