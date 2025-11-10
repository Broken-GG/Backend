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
    /// <summary>
    /// Unit tests for SummonerController
    /// Tests cover: input validation, successful responses, not found cases, and error handling
    /// </summary>
    public class SummonerControllerTest
    {
        private const string ValidSummonerName = "TestPlayer";
        private const string ValidTagline = "EUW";
        private const string ValidPuuid = "test-puuid-1234567890-abcdef-1234567890-abcdef-1234567890";

        private Mock<IRIOTAPI> CreateMockRiotApi() => new Mock<IRIOTAPI>();
        private Mock<IChampionDataService> CreateMockChampionService() => new Mock<IChampionDataService>();

        private SummonerController CreateController(
            Mock<IRIOTAPI> mockRiotApi,
            Mock<IChampionDataService> mockChampionService)
        {
            return new SummonerController(mockRiotApi.Object, mockChampionService.Object);
        }

        /// <summary>
        /// Test 1: Valid summoner name and tagline should return 200 OK with summoner data
        /// </summary>
        [Fact]
        public async Task GetSummonerInfo_ValidSummonerAndTagline_ReturnsOkWithSummonerData()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            string mockPuuidJson = @"{
                ""puuid"": """ + ValidPuuid + @""",
                ""gameName"": """ + ValidSummonerName + @""",
                ""tagLine"": """ + ValidTagline + @"""
            }";

            string mockSummonerJson = @"{
                ""id"": ""summoner-id-123"",
                ""accountId"": ""account-id-456"",
                ""puuid"": """ + ValidPuuid + @""",
                ""profileIconId"": 4901,
                ""summonerLevel"": 150
            }";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(ValidSummonerName, ValidTagline))
                .ReturnsAsync(mockPuuidJson);

            mockRiotApi
                .Setup(x => x.GetSummonerByName(ValidPuuid))
                .ReturnsAsync(mockSummonerJson);

            mockChampionService
                .Setup(x => x.GetCurrentVersionAsync())
                .ReturnsAsync("14.20.1");

            // Act
            IActionResult result = await controller.GetSummonerInfo(ValidSummonerName, ValidTagline);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SummonerResponse summonerInfo = Assert.IsType<SummonerResponse>(okResult.Value);
            Assert.Equal(ValidPuuid, summonerInfo.PUUID);
            Assert.Equal(ValidSummonerName, summonerInfo.SummonerName);
            Assert.Equal(ValidTagline, summonerInfo.Tagline);
        }

        /// <summary>
        /// Test 2: Invalid summoner name should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData("ab")] // Too short (< 3 chars)
        [InlineData("ThisNameIsTooLong")] // Too long (> 16 chars)
        public async Task GetSummonerInfo_InvalidSummonerName_ReturnsBadRequest(string invalidSummonerName)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            // Act
            IActionResult result = await controller.GetSummonerInfo(invalidSummonerName, ValidTagline);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Contains("Invalid summoner name", message);
        }

        /// <summary>
        /// Test 3: Invalid tagline should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData("A")] // Too short (< 2 chars)
        [InlineData("ThisIsTooLong")] // Too long (> 10 chars)
        public async Task GetSummonerInfo_InvalidTagline_ReturnsBadRequest(string invalidTagline)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            // Act
            IActionResult result = await controller.GetSummonerInfo(ValidSummonerName, invalidTagline);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Contains("Invalid tagline", message);
        }

        /// <summary>
        /// Test 4: Summoner not found should return 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetSummonerInfo_SummonerNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline("NotFound", ValidTagline))
                .ReturnsAsync(""); // Empty response when summoner not found

            // Act
            IActionResult result = await controller.GetSummonerInfo("NotFound", ValidTagline);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            string? message = notFound.Value.GetType().GetProperty("message")?.GetValue(notFound.Value, null)?.ToString();
            Assert.Equal("Summoner not found.", message);
        }

        /// <summary>
        /// Test 5: API exception should return 500 Internal Server Error
        /// </summary>
        [Fact]
        public async Task GetSummonerInfo_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(ValidSummonerName, ValidTagline))
                .ThrowsAsync(new Exception("API connection failed"));

            // Act
            IActionResult result = await controller.GetSummonerInfo(ValidSummonerName, ValidTagline);

            // Assert
            ObjectResult serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        /// <summary>
        /// Test 6: Verify RIOT API is called correctly
        /// </summary>
        [Fact]
        public async Task GetSummonerInfo_ValidSummoner_CallsRiotApiCorrectly()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            string mockPuuidJson = @"{
                ""puuid"": """ + ValidPuuid + @""",
                ""gameName"": """ + ValidSummonerName + @""",
                ""tagLine"": """ + ValidTagline + @"""
            }";

            string mockSummonerJson = @"{
                ""id"": ""summoner-id-123"",
                ""profileIconId"": 4901,
                ""summonerLevel"": 150
            }";

            mockRiotApi
                .Setup(x => x.GetPUUIDBySummonerNameAndTagline(ValidSummonerName, ValidTagline))
                .ReturnsAsync(mockPuuidJson);

            mockRiotApi
                .Setup(x => x.GetSummonerByName(ValidPuuid))
                .ReturnsAsync(mockSummonerJson);

            mockChampionService
                .Setup(x => x.GetCurrentVersionAsync())
                .ReturnsAsync("14.20.1");

            // Act
            await controller.GetSummonerInfo(ValidSummonerName, ValidTagline);

            // Assert - Verify both API calls were made
            mockRiotApi.Verify(x => x.GetPUUIDBySummonerNameAndTagline(ValidSummonerName, ValidTagline), Times.Once);
            mockRiotApi.Verify(x => x.GetSummonerByName(ValidPuuid), Times.Once);
        }
    }
}
