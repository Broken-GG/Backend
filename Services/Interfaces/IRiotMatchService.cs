namespace Backend.Services.Interfaces
{
    /// <summary>
    /// Interface for Riot Match API operations
    /// </summary>
    public interface IRiotMatchService
    {
        /// <summary>
        /// Gets match history IDs for a player by PUUID
        /// </summary>
        Task<string> GetMatchByPUUID(string puuid, int start = 0, int count = 10);

        /// <summary>
        /// Gets detailed match information by match ID
        /// </summary>
        Task<string> GetMatchDetailsByMatchId(string matchId);
    }
}
