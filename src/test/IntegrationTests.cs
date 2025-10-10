using Xunit;
using Microsoft.AspNetCore.Mvc;
using api.models;
using api.service;
using api.controller;
using System.Threading.Tasks;
using System;
using DotNetEnv;

namespace api.test
{
    public class SummonerInfoIntegrationTests
    {
        private readonly RIOTAPI _riotApi;
        private readonly SummonerInfoController _controller;

        public SummonerInfoIntegrationTests()
        {
            // Load environment variables from .env file for tests
            Console.WriteLine("📁 Loading .env file for integration tests...");
            
            // Try different paths
            var envPath1 = "src/.env";
            var envPath2 = "../../src/.env";
            var envPath3 = @"c:\Users\Simon\BrokenGG\Backend\src\.env";
            
            Console.WriteLine($"🔍 Trying path: {envPath1}");
            if (System.IO.File.Exists(envPath1))
            {
                Console.WriteLine($"✅ Found .env at: {envPath1}");
                Env.Load(envPath1);
            }
            else if (System.IO.File.Exists(envPath2))
            {
                Console.WriteLine($"✅ Found .env at: {envPath2}");
                Env.Load(envPath2);
            }
            else if (System.IO.File.Exists(envPath3))
            {
                Console.WriteLine($"✅ Found .env at: {envPath3}");
                Env.Load(envPath3);
            }
            else
            {
                Console.WriteLine($"❌ .env file not found at any path");
            }
            
            // Debug: Check what environment variables are loaded
            var apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY");
            var apiUrl = Environment.GetEnvironmentVariable("RIOT_API_URL");
            Console.WriteLine($"🔍 Debug - RIOT_API_KEY: '{apiKey}' (Length: {apiKey?.Length ?? 0})");
            Console.WriteLine($"🔍 Debug - RIOT_API_URL: '{apiUrl}'");
            
            // Use REAL RIOTAPI service - no mocking!
            _riotApi = new RIOTAPI();
            _controller = new SummonerInfoController(_riotApi);
        }

        [Fact]
        public async Task GetSummonerInfo_RealAPI_ValidSummoner_ReturnsCorrectData()
        {
            // Arrange - Using a well-known summoner that should exist
            string summonerName = "Shipulski";
            string tagline = "GOON";
            
            Console.WriteLine($"🌐 REAL API TEST: Testing with {summonerName}#{tagline}");
            Console.WriteLine($"📡 Step 1: Calling GetPUUIDBySummonerNameAndTagline('{summonerName}', '{tagline}')");

            try
            {
                // Act - Make REAL API calls
                var result = await _controller.GetSummonerInfo(summonerName, tagline);

                Console.WriteLine($"✅ API calls completed successfully!");

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var summonerInfo = Assert.IsType<SummonerInfo>(okResult.Value);
                
                Console.WriteLine($"📊 Retrieved Data:");
                Console.WriteLine($"   - Summoner Name: {summonerInfo.SummonerName}");
                Console.WriteLine($"   - Tagline: {summonerInfo.Tagline}");
                Console.WriteLine($"   - Level: {summonerInfo.Level}");
                Console.WriteLine($"   - Region: {summonerInfo.Region}");

                // Verify the data makes sense
                Assert.NotNull(summonerInfo);
                Assert.NotEqual("Unknown", summonerInfo.SummonerName);
                Assert.Equal(tagline, summonerInfo.Tagline);
                Assert.True(summonerInfo.Level > 0, "Level should be greater than 0");
                Assert.Equal("EU", summonerInfo.Region);

                Console.WriteLine($"🎉 All real API assertions passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Real API test failed: {ex.Message}");
                Console.WriteLine($"🔍 This might indicate:");
                Console.WriteLine($"   - API key is invalid");
                Console.WriteLine($"   - API endpoints are wrong");
                Console.WriteLine($"   - Network connectivity issues");
                Console.WriteLine($"   - Rate limiting");
                throw; // Re-throw to fail the test
            }
        }

        [Fact]
        public async Task GetSummonerInfo_RealAPI_TestAPIEndpoints()
        {
            // Test the individual API methods directly
            string summonerName = "Shipulski";
            string tagline = "GOON";

            Console.WriteLine($"🔧 DIRECT API ENDPOINT TEST: {summonerName}#{tagline}");

            try
            {
                // Test Step 1: Get PUUID
                Console.WriteLine($"📡 Testing GetPUUIDBySummonerNameAndTagline...");
                var puuidResponse = await _riotApi.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
                Console.WriteLine($"📄 PUUID Response: {puuidResponse}");
                
                // Parse PUUID from response
                dynamic? puuidData = Newtonsoft.Json.JsonConvert.DeserializeObject(puuidResponse);
                string? puuid = puuidData?.puuid;
                
                Assert.NotNull(puuid);
                Console.WriteLine($"✅ PUUID extracted: {puuid}");

                // Test Step 2: Get Summoner Data
                Console.WriteLine($"📡 Testing GetSummonerByName with PUUID...");
                var summonerResponse = await _riotApi.GetSummonerByName(puuid);
                Console.WriteLine($"📄 Summoner Response: {summonerResponse}");

                // Parse summoner data
                dynamic? summonerData = Newtonsoft.Json.JsonConvert.DeserializeObject(summonerResponse);
                
                // The summoner API only returns puuid, profileIconId, revisionDate, and summonerLevel
                // It does NOT include name or tagLine - those come from the account API
                Assert.NotNull(summonerData);
                Assert.NotNull(summonerData?.puuid);
                Assert.True(summonerData?.summonerLevel > 0);
                
                // Verify the name and tagline from the account API response
                Assert.Equal(summonerName, (string?)puuidData?.gameName);
                Assert.Equal(tagline, (string?)puuidData?.tagLine);

                Console.WriteLine($"✅ Both API endpoints working correctly!");
                Console.WriteLine($"   - Summoner Name (from account API): {puuidData?.gameName}");
                Console.WriteLine($"   - Tagline (from account API): {puuidData?.tagLine}");
                Console.WriteLine($"   - Level (from summoner API): {summonerData?.summonerLevel}");
                Console.WriteLine($"   - PUUID: {summonerData?.puuid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Direct API endpoint test failed: {ex.Message}");
                Console.WriteLine($"🔍 Check your API URLs and authentication");
                throw;
            }
        }
    }
}