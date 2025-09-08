using System.Text.Json;

namespace VoiceLauncherWasm.Services.IndexedDB
{
    public interface IIndexedDBService
    {
        Task InitializeAsync();
        Task<T[]> GetAllAsync<T>(string storeName);
        Task<T?> GetByIdAsync<T>(string storeName, object id);
        Task<object> AddAsync<T>(string storeName, T item);
        Task UpdateAsync<T>(string storeName, T item);
        Task DeleteAsync(string storeName, object id);
        Task ClearAsync(string storeName);
        Task<T[]> QueryAsync<T>(string storeName, string indexName, object keyRange);
        Task<int> CountAsync(string storeName);
    }
}