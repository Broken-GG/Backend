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
    /// Unit tests for MasteryController
    /// Tests cover: input validation, successful responses, not found cases, and error handling
    /// </summary>
    public class MasteryControllerTests
    {
        private const string ValidPuuid = "test-puuid-1234567890-abcdef-1234567890-abcdef-1234567890";
        private const string ValidPuuidNoMastery = "test-puuid-no-mastery-1234567890-abcdef-1234567890-abc";
        private const string ValidPuuidError = "test-puuid-error-case-1234567890-abcdef-1234567890-ab";
        private const string ValidPuuidMultiple = "test-puuid-multiple-champions-1234567890-abcdef-12345";

        private Mock<IRIOTAPI> CreateMockRiotApi() => new Mock<IRIOTAPI>();
        private Mock<IChampionDataService> CreateMockChampionService() => new Mock<IChampionDataService>();

        private MasteryController CreateController(
            Mock<IRIOTAPI> mockRiotApi,
            Mock<IChampionDataService> mockChampionService)
        {
            return new MasteryController(mockRiotApi.Object, mockChampionService.Object);
        }

        /// <summary>
        /// Test 1: Valid PUUID should return 200 OK with mastery data
        /// </summary>
        [Fact]
        public async Task GetMasteryInfo_ValidPuuid_ReturnsOkWithMasteryData()
        {
            // Arrange - Set up test data
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            string mockJsonResponse = @"[
                {
                    ""puuid"": """ + ValidPuuid + @""",
                    ""championId"": 157,
                    ""championLevel"": 7,
                    ""championPoints"": 250000
                }
            ]";

            // Mock the RIOT API to return our test data
            mockRiotApi
                .Setup(x => x.GetMasteryInfoByPUUID(ValidPuuid))
                .ReturnsAsync(mockJsonResponse);

            // Mock ChampionDataService to return champion info
            mockChampionService
                .Setup(x => x.GetChampionDataAsync(157, null))
                .ReturnsAsync(("Yasuo", "http://ddragon.leagueoflegends.com/cdn/14.20.1/img/champion/Yasuo.png"));

            // Act - Call the controller method
            IActionResult result = await controller.GetMasteryInfo(ValidPuuid);

            // Assert - Verify the result
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            MasteryInfoResponse[] masteryArray = Assert.IsType<MasteryInfoResponse[]>(okResult.Value);
            Assert.Single(masteryArray);
            Assert.Equal(157, masteryArray[0].ChampionId);
            Assert.Equal("Yasuo", masteryArray[0].ChampionName);
        }

        /// <summary>
        /// Test 2: Invalid PUUID format should return 400 BadRequest
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData("abc")] // Too short
        public async Task GetMasteryInfo_InvalidPuuid_ReturnsBadRequest(string invalidPuuid)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            // Act
            IActionResult result = await controller.GetMasteryInfo(invalidPuuid);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Equal("Invalid PUUID format", message);
        }

        /// <summary>
        /// Test 3: Empty mastery data should return 404 NotFound
        /// </summary>
        [Fact]
        public async Task GetMasteryInfo_NoMasteryFound_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            mockRiotApi
                .Setup(x => x.GetMasteryInfoByPUUID(ValidPuuidNoMastery))
                .ReturnsAsync("[]"); // Empty array

            // Act
            IActionResult result = await controller.GetMasteryInfo(ValidPuuidNoMastery);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            string? message = notFound.Value.GetType().GetProperty("message")?.GetValue(notFound.Value, null)?.ToString();
            Assert.Equal("Mastery info not found.", message);
        }

        /// <summary>
        /// Test 4: API exception should return 500 Internal Server Error
        /// </summary>
        [Fact]
        public async Task GetMasteryInfo_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            mockRiotApi
                .Setup(x => x.GetMasteryInfoByPUUID(ValidPuuidError))
                .ThrowsAsync(new Exception("API connection failed"));

            // Act
            IActionResult result = await controller.GetMasteryInfo(ValidPuuidError);

            // Assert
            ObjectResult serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        /// <summary>
        /// Test 5: Verify that ChampionDataService is called for each mastery entry
        /// </summary>
        [Fact]
        public async Task GetMasteryInfo_MultipleChampions_CallsChampionServiceForEach()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var mockChampionService = CreateMockChampionService();
            var controller = CreateController(mockRiotApi, mockChampionService);

            string mockJsonResponse = @"[
                { ""championId"": 157, ""championLevel"": 7 },
                { ""championId"": 238, ""championLevel"": 6 }
            ]";

            mockRiotApi
                .Setup(x => x.GetMasteryInfoByPUUID(ValidPuuidMultiple))
                .ReturnsAsync(mockJsonResponse);

            mockChampionService
                .Setup(x => x.GetChampionDataAsync(It.IsAny<long>(), null))
                .ReturnsAsync(("TestChampion", "http://test.url"));

            // Act
            await controller.GetMasteryInfo(ValidPuuidMultiple);

            // Assert - Verify ChampionDataService was called twice (once for each champion)
            mockChampionService.Verify(x => x.GetChampionDataAsync(157, null), Times.Once);
            mockChampionService.Verify(x => x.GetChampionDataAsync(238, null), Times.Once);
        }
    }
}