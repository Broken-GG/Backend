using Newtonsoft.Json;
using api.models;

namespace api.helpers
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

                dynamic? data = JsonConvert.DeserializeObject(jsonData);
                return (data?.puuid, data?.gameName);
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
        /// <returns>SummonerInfo object or a default instance if parsing fails</returns>
        public static async Task<SummonerInfo> DeserializeSummonerInfoAsync(string jsonData, api.service.IChampionDataService championDataService)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    throw new ArgumentException("JSON data cannot be null or empty");
                }

                dynamic? data = JsonConvert.DeserializeObject(jsonData);
                
                var profileIconId = (int)(data?.profileIconId ?? 0);
                var summonerLevel = (int)(data?.summonerLevel ?? 0);
                
                Console.WriteLine($"🔍 Profile Icon ID: {profileIconId}");
                Console.WriteLine($"🔍 Summoner Level: {summonerLevel}");
                
                // Get latest Data Dragon version
                var version = await championDataService.GetCurrentVersionAsync();
                var profileIconUrl = profileIconId > 0
                    ? $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/{profileIconId}.png"
                    : $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/0.png";
                
                Console.WriteLine($"🔍 Profile Icon URL: {profileIconUrl}");
                
                return new SummonerInfo
                {
                    SummonerName = data?.name ?? "Unknown",
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
                var version = await championDataService.GetCurrentVersionAsync();
                
                // Return placeholder if deserialization fails
                return new SummonerInfo
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
