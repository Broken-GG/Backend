using Backend.Configuration;
using Backend.Services.Interfaces;
using Backend.Services.External.RiotApi;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Backend.Extensions
{
    /// <summary>
    /// Extension methods for configuring services in dependency injection container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add all Riot API related services to the DI container
        /// </summary>
        public static IServiceCollection AddRiotApiServices(this IServiceCollection services)
        {
            // Register HttpClient for Riot API services
            services.AddHttpClient<IRiotAccountService, RiotAccountService>();
            services.AddHttpClient<IRiotMatchService, RiotMatchService>();
            services.AddHttpClient<IRiotLeagueService, RiotLeagueService>();
            
            // Register facade interface for clean architecture and testability
            services.AddScoped<IRIOTAPI, RIOTAPI>();

            return services;
        }

        /// <summary>
        /// Add game data services (champion, item, spell data)
        /// </summary>
        public static IServiceCollection AddGameDataServices(this IServiceCollection services)
        {
            services.AddHttpClient<IChampionDataService, ChampionDataService>();
            services.AddHttpClient<IGameDataService, GameDataService>();

            return services;
        }

        /// <summary>
        /// Configure Riot API settings from configuration
        /// </summary>
        public static IServiceCollection AddRiotApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind RiotApi settings from environment variables
            services.Configure<RiotApiSettings>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY") ?? "";
                options.AccountApiBaseUrl = Environment.GetEnvironmentVariable("RIOT_API_URL") 
                    ?? "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id";
                options.SummonerApiBaseUrl = Environment.GetEnvironmentVariable("RIOT_SUMMONER_URL") 
                    ?? "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid";
                options.MatchApiBaseUrl = "https://europe.api.riotgames.com/lol/match/v5";
                options.LeagueApiBaseUrl = "https://euw1.api.riotgames.com/lol/league/v4";
                options.Region = "euw1";
                options.DefaultMatchCount = 10;
                options.MaxMatchCount = 100;
            });
            
            return services;
        }

        /// <summary>
        /// Add all application services at once
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRiotApiConfiguration(configuration);
            services.AddRiotApiServices();
            services.AddGameDataServices();

            return services;
        }
    }
}
