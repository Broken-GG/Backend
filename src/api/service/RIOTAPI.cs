using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace api.service
{
    public class RIOTAPI
    {
        private readonly HttpClient _httpClient;
        private readonly string? apiKey;
        private readonly string baseUrl;
        private readonly string summonerUrl;

        public RIOTAPI()
        {
            _httpClient = new HttpClient();
            apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY");
            baseUrl = Environment.GetEnvironmentVariable("RIOT_API_URL") ?? "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id";
            summonerUrl = Environment.GetEnvironmentVariable("RIOT_SUMMONER_URL") ?? "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid";
            
            // Debug output to see what's loaded
            Console.WriteLine($"🔑 API Key loaded: {(string.IsNullOrEmpty(apiKey) ? "❌ MISSING" : "✅ Present (length: " + apiKey.Length + ")")}");
            Console.WriteLine($"🌐 Account API URL: {baseUrl}");
            Console.WriteLine($"🌐 Summoner API URL: {summonerUrl}");
        }

        private HttpRequestMessage SetRequestMessageHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("X-Riot-Token", apiKey);
            }
            return request;
        }
        public virtual async Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline)
        {
            var url = $"{baseUrl}/{summonerName}/{tagline}";
            // Console.WriteLine($"🌐 API Call 1: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            // Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }

        public virtual async Task<string> GetSummonerByName(string PUUID)
        {
            var url = $"{summonerUrl}/{PUUID}";
            // Console.WriteLine($"🌐 API Call 2: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            // Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }
        public virtual async Task<string> GetMatchByPUUID(string PUUID, int start = 0, int count = 10)
        {
            var url = $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{PUUID}/ids?start={start}&count={count}";
            Console.WriteLine($"🌐 API Call: GET {url}");

            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await _httpClient.SendAsync(request);
                    Console.WriteLine($"📊 Response Status: {response.StatusCode}");

                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📄 Response Content: {content.Substring(0, Math.Min(200, content.Length))}...");

                    return content;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"❌ Attempt {attempt}: {ex.Message}");

                    if (attempt == 3 || ex.StatusCode == null || (int)ex.StatusCode != 502)
                    {
                        throw;
                    }

                    Console.WriteLine("🔄 Retrying...");
                    await Task.Delay(1000); // Wait before retrying
                }
            }

            throw new Exception("Failed to fetch match data after 3 attempts.");
        }
        public virtual async Task<string> GetMatchDetailsByMatchId(string matchId)
        {
            var url = $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}";
            // Console.WriteLine($"🌐 API Call 4: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            // Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }

        public virtual async Task<string> GetRankedInfoByPUUID(string PUUID)
        {
            var url = $"https://euw1.api.riotgames.com/lol/league/v4/entries/by-puuid/{PUUID}";
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            
            return content;
        }

        public virtual async Task<string> GetMasteryInfoByPUUID(string PUUID)
        {
            var url = $"https://euw1.api.riotgames.com/lol/champion-mastery/v4/champion-masteries/by-puuid/{PUUID}";
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            
            return content;
        }
    }
}