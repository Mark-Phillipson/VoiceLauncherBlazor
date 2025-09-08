using Microsoft.JSInterop;
using System.Text.Json;

namespace VoiceLauncherWasm.Services.IndexedDB
{
    public class IndexedDBService : IIndexedDBService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _databaseName = "VoiceLauncherDB";
        private readonly int _version = 1;
    // TaskCompletionSource used to signal when InitializeAsync has completed.
    // Other methods will await this to ensure JS runtime and the DB are ready.
    private readonly TaskCompletionSource<bool> _initializedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IndexedDBService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.initialize", _databaseName, _version);
                _initializedTcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _initializedTcs.TrySetException(ex);
                throw;
            }
        }

        public async Task<T[]> GetAllAsync<T>(string storeName)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.getAll", storeName);
            if (string.IsNullOrEmpty(jsonResult))
                return Array.Empty<T>();
            
            return JsonSerializer.Deserialize<T[]>(jsonResult) ?? Array.Empty<T>();
        }

        public async Task<T?> GetByIdAsync<T>(string storeName, object id)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.getById", storeName, id);
            if (string.IsNullOrEmpty(jsonResult))
                return default;
            
            return JsonSerializer.Deserialize<T>(jsonResult);
        }

        public async Task<object> AddAsync<T>(string storeName, T item)
        {
            var json = JsonSerializer.Serialize(item);
            await _initializedTcs.Task.ConfigureAwait(false);
            return await _jsRuntime.InvokeAsync<object>("voiceIndexedDB.add", storeName, json);
        }

        public async Task UpdateAsync<T>(string storeName, T item)
        {
            var json = JsonSerializer.Serialize(item);
            await _initializedTcs.Task.ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.update", storeName, json);
        }

        public async Task DeleteAsync(string storeName, object id)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.delete", storeName, id);
        }

        public async Task ClearAsync(string storeName)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync("voiceIndexedDB.clear", storeName);
        }

        public async Task<T[]> QueryAsync<T>(string storeName, string indexName, object keyRange)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            var jsonResult = await _jsRuntime.InvokeAsync<string>("voiceIndexedDB.query", storeName, indexName, keyRange);
            if (string.IsNullOrEmpty(jsonResult))
                return Array.Empty<T>();
            
            return JsonSerializer.Deserialize<T[]>(jsonResult) ?? Array.Empty<T>();
        }

        public async Task<int> CountAsync(string storeName)
        {
            await _initializedTcs.Task.ConfigureAwait(false);
            return await _jsRuntime.InvokeAsync<int>("voiceIndexedDB.count", storeName);
        }
    }
}