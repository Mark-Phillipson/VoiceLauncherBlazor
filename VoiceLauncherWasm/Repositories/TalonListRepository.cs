using VoiceLauncherWasm.Models;
using VoiceLauncherWasm.Services.IndexedDB;

namespace VoiceLauncherWasm.Repositories
{
    public class TalonListRepository : ITalonListRepository
    {
        private readonly IIndexedDBService _indexedDB;
        private const string STORE_NAME = "TalonLists";

        public TalonListRepository(IIndexedDBService indexedDB)
        {
            _indexedDB = indexedDB;
        }

        public async Task<IEnumerable<TalonList>> GetAllAsync()
        {
            var lists = await _indexedDB.GetAllAsync<TalonList>(STORE_NAME);
            return lists.OrderBy(l => l.ListName).ThenBy(l => l.SpokenForm);
        }

        public async Task<TalonList?> GetByIdAsync(int id)
        {
            return await _indexedDB.GetByIdAsync<TalonList>(STORE_NAME, id);
        }

        public async Task<int> AddAsync(TalonList list)
        {
            list.CreatedAt = DateTime.UtcNow;
            list.ImportedAt = DateTime.UtcNow;
            var result = await _indexedDB.AddAsync(STORE_NAME, list);
            return Convert.ToInt32(result);
        }

        public async Task UpdateAsync(TalonList list)
        {
            await _indexedDB.UpdateAsync(STORE_NAME, list);
        }

        public async Task DeleteAsync(int id)
        {
            await _indexedDB.DeleteAsync(STORE_NAME, id);
        }

        public async Task ClearAllAsync()
        {
            await _indexedDB.ClearAsync(STORE_NAME);
        }

        public async Task<IEnumerable<TalonList>> GetByListNameAsync(string listName)
        {
            var allLists = await _indexedDB.GetAllAsync<TalonList>(STORE_NAME);
            return allLists.Where(l => l.ListName.Equals(listName, StringComparison.OrdinalIgnoreCase))
                          .OrderBy(l => l.SpokenForm);
        }

        public async Task<IEnumerable<TalonList>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var allLists = await _indexedDB.GetAllAsync<TalonList>(STORE_NAME);
            var lowerTerm = searchTerm.ToLower();

            return allLists.Where(l =>
                l.ListName.ToLower().Contains(lowerTerm) ||
                l.SpokenForm.ToLower().Contains(lowerTerm) ||
                l.ListValue.ToLower().Contains(lowerTerm)
            ).OrderBy(l => l.ListName).ThenBy(l => l.SpokenForm);
        }

        public async Task<int> GetCountAsync()
        {
            return await _indexedDB.CountAsync(STORE_NAME);
        }

        public async Task<int> ImportListsAsync(IEnumerable<TalonList> lists)
        {
            // Clear existing lists first
            await ClearAllAsync();

            int count = 0;
            foreach (var list in lists)
            {
                await AddAsync(list);
                count++;
            }

            return count;
        }
    }
}