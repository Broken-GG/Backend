
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
        private readonly string region;

        public RIOTAPI()
        {
            _httpClient = new HttpClient();
            apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY");
            baseUrl = Environment.GetEnvironmentVariable("RIOT_API_URL") ?? "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id";
            summonerUrl = Environment.GetEnvironmentVariable("RIOT_SUMMONER_URL") ?? "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid";
            region = "EU";
            
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
            Console.WriteLine($"🌐 API Call 1: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }

        public virtual async Task<string> GetSummonerByName(string PUUID)
        {
            var url = $"{summonerUrl}/{PUUID}";
            Console.WriteLine($"🌐 API Call 2: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }
        public virtual async Task<string> GetMatchByPUUID(string PUUID)
        {
            var url = $"https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/{PUUID}/ids?count=10";
            Console.WriteLine($"🌐 API Call 3: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }
        public virtual async Task<string> GetMatchDetailsByMatchId(string matchId)
        {
            var url = $"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}";
            Console.WriteLine($"🌐 API Call 4: GET {url}");
            
            var request = SetRequestMessageHeaders(new HttpRequestMessage(HttpMethod.Get, url));

            var response = await _httpClient.SendAsync(request);
            Console.WriteLine($"📊 Response Status: {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📄 Response Content: {content}");
            
            return content;
        }
    }
}