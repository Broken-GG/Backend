namespace Backend.Services.Interfaces
{
    /// <summary>
    /// Interface for Riot Account and Summoner API operations
    /// </summary>
    public interface IRiotAccountService
    {
        /// <summary>
        /// Gets PUUID and account information by summoner name and tagline
        /// </summary>
        Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline);

        /// <summary>
        /// Gets summoner information by PUUID
        /// </summary>
        Task<string> GetSummonerByPUUID(string puuid);
    }
}
