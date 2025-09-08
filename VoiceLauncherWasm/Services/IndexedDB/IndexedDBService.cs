using Microsoft.JSInterop;
using System.Text.Json;

namespace VoiceLauncherWasm.Services.IndexedDB
{
    public class IndexedDBService : IIndexedDBService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _databaseName = "VoiceLauncherDB";
        private readonly int _version = 1;

        public IndexedDBService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.initialize", _databaseName, _version);
        }

        public async Task<T[]> GetAllAsync<T>(string storeName)
        {
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.getAll", storeName);
            if (string.IsNullOrEmpty(jsonResult))
                return Array.Empty<T>();
            
            return JsonSerializer.Deserialize<T[]>(jsonResult) ?? Array.Empty<T>();
        }

        public async Task<T?> GetByIdAsync<T>(string storeName, object id)
        {
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.getById", storeName, id);
            if (string.IsNullOrEmpty(jsonResult))
                return default;
            
            return JsonSerializer.Deserialize<T>(jsonResult);
        }

        public async Task<object> AddAsync<T>(string storeName, T item)
        {
            var json = JsonSerializer.Serialize(item);
            return await _jsRuntime.InvokeAsync<object>("voiceIndexedDB.add", storeName, json);
        }

        public async Task UpdateAsync<T>(string storeName, T item)
        {
            var json = JsonSerializer.Serialize(item);
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.update", storeName, json);
        }

        public async Task DeleteAsync(string storeName, object id)
        {
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.delete", storeName, id);
        }

        public async Task ClearAsync(string storeName)
        {
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.clear", storeName);
        }

        public async Task<T[]> QueryAsync<T>(string storeName, string indexName, object keyRange)
        {
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.query", storeName, indexName, keyRange);
            if (string.IsNullOrEmpty(jsonResult))
                return Array.Empty<T>();
            
            return JsonSerializer.Deserialize<T[]>(jsonResult) ?? Array.Empty<T>();
        }

        public async Task<int> CountAsync(string storeName)
        {
            return await _jsRuntime.InvokeAsync<int>("voiceIndexedDB.count", storeName);
        }
    }
}