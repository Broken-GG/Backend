namespace Backend.Services.Interfaces
{
    /// <summary>
    /// Interface for Riot League/Ranked API operations
    /// </summary>
    public interface IRiotLeagueService
    {
        /// <summary>
        /// Gets ranked/league information for a player by PUUID
        /// </summary>
        Task<string> GetRankedInfoByPUUID(string puuid);

        /// <summary>
        /// Gets champion mastery information for a player by PUUID
        /// </summary>
        Task<string> GetMasteryInfoByPUUID(string puuid);
    }
}
