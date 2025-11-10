namespace Backend.Services.Interfaces
{
    public interface IGameDataService
    {
        Task<string> GetSummonerSpellNameByIdAsync(int spellId);
        Task<string> GetSummonerSpellIconUrlAsync(int spellId);
        Task<string> GetItemNameByIdAsync(int itemId);
        Task<string> GetItemIconUrlAsync(int itemId);
        Task<(string Name, string IconUrl)> GetSummonerSpellDataAsync(int spellId);
        Task<(string Name, string IconUrl)> GetItemDataAsync(int itemId);
        string GetArenaAugmentIconUrl(int augmentId);
    }
}
