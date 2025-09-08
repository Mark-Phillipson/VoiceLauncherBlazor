using VoiceLauncherWasm.Models;

namespace VoiceLauncherWasm.Repositories
{
    public interface ITalonListRepository
    {
        Task<IEnumerable<TalonList>> GetAllAsync();
        Task<TalonList?> GetByIdAsync(int id);
        Task<int> AddAsync(TalonList list);
        Task UpdateAsync(TalonList list);
        Task DeleteAsync(int id);
        Task ClearAllAsync();
        Task<IEnumerable<TalonList>> GetByListNameAsync(string listName);
        Task<IEnumerable<TalonList>> SearchAsync(string searchTerm);
        Task<int> GetCountAsync();
        Task<int> ImportListsAsync(IEnumerable<TalonList> lists);
    }
}