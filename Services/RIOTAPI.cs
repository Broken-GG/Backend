using System.Threading.Tasks;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    /// <summary>
    /// Facade class for Riot API operations. Delegates to specialized service classes.
    /// Maintained for backward compatibility with existing controllers.
    /// </summary>
    public class RIOTAPI
    {
        private readonly IRiotAccountService _accountService;
        private readonly IRiotMatchService _matchService;
        private readonly IRiotLeagueService _leagueService;

        public RIOTAPI(IRiotAccountService accountService, IRiotMatchService matchService, IRiotLeagueService leagueService)
        {
            _accountService = accountService;
            _matchService = matchService;
            _leagueService = leagueService;
        }

        #region Account Service Methods
        
        public virtual async Task<string> GetPUUIDBySummonerNameAndTagline(string summonerName, string tagline)
        {
            return await _accountService.GetPUUIDBySummonerNameAndTagline(summonerName, tagline);
        }

        public virtual async Task<string> GetSummonerByName(string PUUID)
        {
            return await _accountService.GetSummonerByPUUID(PUUID);
        }

        #endregion

        #region Match Service Methods

        public virtual async Task<string> GetMatchByPUUID(string PUUID, int start = 0, int count = 10)
        {
            return await _matchService.GetMatchByPUUID(PUUID, start, count);
        }

        public virtual async Task<string> GetMatchDetailsByMatchId(string matchId)
        {
            return await _matchService.GetMatchDetailsByMatchId(matchId);
        }

        #endregion

        #region League Service Methods

        public virtual async Task<string> GetRankedInfoByPUUID(string PUUID)
        {
            return await _leagueService.GetRankedInfoByPUUID(PUUID);
        }

        public virtual async Task<string> GetMasteryInfoByPUUID(string PUUID)
        {
            return await _leagueService.GetMasteryInfoByPUUID(PUUID);
        }

        #endregion
    }
}
