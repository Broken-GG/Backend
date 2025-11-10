using System.Threading.Tasks;

namespace Backend.Services.Interfaces
{
    /// <summary>
    /// Interface for Riot API facade operations
    /// </summary>
    public interface IRIOTAPI
    {
        // Account Service Methods
        Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline);
        Task<string> GetSummonerByName(string PUUID);

        // Match Service Methods
        Task<string> GetMatchByPUUID(string puuid, int start, int count);
        Task<string> GetMatchDetailsByMatchId(string matchId);

        // League Service Methods
        Task<string> GetRankedInfoByPUUID(string puuid);
        Task<string> GetMasteryInfoByPUUID(string puuid);
    }
}
