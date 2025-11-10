using Newtonsoft.Json;
using Backend.Models.RiotApi;
using Backend.Models.DTOs.Response;
using Backend.Services.Interfaces;

namespace Backend.Helpers
{
    /// <summary>
    /// Helper class for deserializing Riot API JSON responses
    /// </summary>
    public static class RiotApiDeserializer
    {
        /// <summary>
        /// Deserialize PUUID information from Riot account API response
        /// </summary>
        /// <param name="jsonData">JSON string from Riot API</param>
        /// <returns>Tuple containing PUUID and GameName, or nulls if parsing fails</returns>
        public static (string? PUUID, string? GameName) DeserializePUUIDInfo(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return (null, null);
                }

                RiotAccountData? data = JsonConvert.DeserializeObject<RiotAccountData>(jsonData);
                return (data?.Puuid, data?.GameName);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"⚠️ Error deserializing PUUID info: {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// Deserialize summoner information from Riot summoner API response
        /// </summary>
        /// <param name="jsonData">JSON string from Riot API</param>
        /// <returns>SummonerResponse object or a default instance if parsing fails</returns>
        public static async Task<SummonerResponse> DeserializeSummonerInfoAsync(string jsonData, IChampionDataService championDataService)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    throw new ArgumentException("JSON data cannot be null or empty");
                }

                RiotSummonerData? data = JsonConvert.DeserializeObject<RiotSummonerData>(jsonData);
                
                int profileIconId = data?.ProfileIconId ?? 0;
                int summonerLevel = data?.SummonerLevel ?? 0;

                Console.WriteLine($"🔍 Profile Icon ID: {profileIconId}");
                Console.WriteLine($"🔍 Summoner Level: {summonerLevel}");
                
                // Get latest Data Dragon version
                string version = await championDataService.GetCurrentVersionAsync();
                string profileIconUrl = profileIconId > 0
                    ? $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/{profileIconId}.png"
                    : $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/0.png";
                
                Console.WriteLine($"🔍 Profile Icon URL: {profileIconUrl}");
                
                return new SummonerResponse
                {
                    SummonerName = data?.Name ?? "Unknown",
                    Tagline = "Unknown", // Will be set from the original request
                    Level = summonerLevel,
                    Region = "EU", // Default region for now
                    ProfileIconUrl = profileIconUrl,
                    PUUID = "" // Will be set in the main method
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deserializing summoner info: {ex.Message}");
                
                // Get latest Data Dragon version for fallback
                string version = await championDataService.GetCurrentVersionAsync();
                
                // Return placeholder if deserialization fails
                return new SummonerResponse
                {
                    SummonerName = "Unknown",
                    Tagline = "Unknown",
                    Level = 0,
                    Region = "EU",
                    ProfileIconUrl = $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/0.png"
                };
            }
        }
    }
}
