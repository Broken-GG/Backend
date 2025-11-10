namespace Backend.Services.Interfaces
{
    public interface IChampionDataService
    {
        Task<string> GetCurrentVersionAsync();
        Task<string> GetChampionNameByIdAsync(long championId);
        Task<string> GetChampionIconUrlAsync(long championId, string? version = null);
        Task<(string Name, string IconUrl)> GetChampionDataAsync(long championId, string? version = null);
    }
}
