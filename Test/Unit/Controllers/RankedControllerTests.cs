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
    /// Unit tests for RankedController
    /// Tests cover: input validation, successful responses, not found cases, and error handling
    /// </summary>
    public class RankedControllerTests
    {
        private const string ValidPuuid = "test-puuid-1234567890-abcdef-1234567890-abcdef-1234567890";
        private const string ValidPuuidNoRanked = "test-puuid-no-ranked-1234567890-abcdef-1234567890-abc";
        private const string ValidPuuidError = "test-puuid-error-case-1234567890-abcdef-1234567890-ab";

        private Mock<IRIOTAPI> CreateMockRiotApi() => new Mock<IRIOTAPI>();

        private RankedController CreateController(Mock<IRIOTAPI> mockRiotApi)
        {
            return new RankedController(mockRiotApi.Object);
        }
        /// <summary>
        /// Test 1: Valid PUUID should return 200 OK with ranked data array
        /// </summary>
        [Fact]
        public async Task GetRankedInfo_ValidPuuid_ReturnsOkWithRankedData()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var controller = CreateController(mockRiotApi);

            string mockJsonResponse = @"[
                {
                    ""queueType"": ""RANKED_SOLO_5x5"",
                    ""tier"": ""GOLD"",
                    ""rank"": ""III"",
                    ""leaguePoints"": 50,
                    ""wins"": 100,
                    ""losses"": 90
                }
            ]";

            mockRiotApi
                .Setup(x => x.GetRankedInfoByPUUID(ValidPuuid))
                .ReturnsAsync(mockJsonResponse);

            // Act
            IActionResult result = await controller.GetRankedInfo(ValidPuuid);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            RankedInfoResponse[] rankedInfo = Assert.IsType<RankedInfoResponse[]>(okResult.Value);
            Assert.NotNull(rankedInfo);
            Assert.Single(rankedInfo);
            Assert.Equal("RANKED_SOLO_5x5", rankedInfo[0].QueueType);
        }

        /// <summary>
        /// Test 2: PUUID with no ranked data should return 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetRankedInfo_NoRankedData_ReturnsNotFound()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var controller = CreateController(mockRiotApi);

            mockRiotApi
                .Setup(x => x.GetRankedInfoByPUUID(ValidPuuidNoRanked))
                .ReturnsAsync("[]"); // Empty array

            // Act
            IActionResult result = await controller.GetRankedInfo(ValidPuuidNoRanked);

            // Assert
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            string? message = notFound.Value.GetType().GetProperty("message")?.GetValue(notFound.Value, null)?.ToString();
            Assert.Equal("Ranked info not found.", message);
        }
        /// <summary>
        /// Test 3: Invalid PUUID should return 400 Bad Request
        /// </summary>
        [Theory]
        [InlineData("")] // Empty string
        [InlineData("   ")] // Whitespace
        [InlineData("abc")] // Too short
        public async Task GetRankedInfo_InvalidPuuid_ReturnsBadRequest(string invalidPuuid)
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var controller = CreateController(mockRiotApi);

            // Act
            IActionResult result = await controller.GetRankedInfo(invalidPuuid);

            // Assert
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            string? message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value, null)?.ToString();
            Assert.Equal("Invalid PUUID format", message);
        }
        /// <summary>
        /// Test 4: API exception returns 500 Internal Server Error
        /// </summary>
        [Fact]
        public async Task GetRankedInfo_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var controller = CreateController(mockRiotApi);

            mockRiotApi
                .Setup(x => x.GetRankedInfoByPUUID(ValidPuuidError))
                .ThrowsAsync(new Exception("API connection failed"));

            // Act
            IActionResult result = await controller.GetRankedInfo(ValidPuuidError);

            // Assert
            ObjectResult serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
            Assert.NotNull(serverError.Value);
            string? message = serverError.Value.GetType().GetProperty("message")?.GetValue(serverError.Value, null)?.ToString();
            Assert.Equal("An error occurred while fetching ranked info.", message);
        }

        /// <summary>
        /// Test 5: Verify RIOT API is called with correct PUUID
        /// </summary>
        [Fact]
        public async Task GetRankedInfo_ValidPuuid_CallsRiotApiCorrectly()
        {
            // Arrange
            var mockRiotApi = CreateMockRiotApi();
            var controller = CreateController(mockRiotApi);

            string mockJsonResponse = @"[
                {
                    ""queueType"": ""RANKED_SOLO_5x5"",
                    ""tier"": ""GOLD"",
                    ""rank"": ""III""
                }
            ]";

            mockRiotApi
                .Setup(x => x.GetRankedInfoByPUUID(ValidPuuid))
                .ReturnsAsync(mockJsonResponse);

            // Act
            await controller.GetRankedInfo(ValidPuuid);

            // Assert - Verify RIOT API was called once with correct PUUID
            mockRiotApi.Verify(x => x.GetRankedInfoByPUUID(ValidPuuid), Times.Once);
        }
    }
}