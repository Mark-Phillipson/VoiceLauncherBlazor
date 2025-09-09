using Microsoft.JSInterop;
using RCLTalonShared.Models;
using System.Text.Json;

namespace VoiceLauncherWasm.Services
{
    public interface IIndexedDBService
    {
        Task InitializeAsync();
    Task<List<TalonVoiceCommand>> GetAllCommandsAsync();
    Task<List<TalonList>> GetAllListsAsync();
    Task AddCommandAsync(TalonVoiceCommand command);
    Task AddListAsync(TalonList list);
        Task ClearAllDataAsync();
    }

    public class IndexedDBService : IIndexedDBService
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _indexedDbModule;

        public IndexedDBService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            _indexedDbModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/indexeddb.js");
            await _indexedDbModule.InvokeVoidAsync("initializeDatabase");
        }

        public async Task<List<TalonVoiceCommand>> GetAllCommandsAsync()
        {
            if (_indexedDbModule == null) await InitializeAsync();
            var result = await _indexedDbModule!.InvokeAsync<string>("getAllCommands");
            return JsonSerializer.Deserialize<List<TalonVoiceCommand>>(result) ?? new List<TalonVoiceCommand>();
        }

        public async Task<List<TalonList>> GetAllListsAsync()
        {
            if (_indexedDbModule == null) await InitializeAsync();
            var result = await _indexedDbModule!.InvokeAsync<string>("getAllLists");
            return JsonSerializer.Deserialize<List<TalonList>>(result) ?? new List<TalonList>();
        }

    public async Task AddCommandAsync(TalonVoiceCommand command)
        {
            if (_indexedDbModule == null) await InitializeAsync();
            var json = JsonSerializer.Serialize(command);
            await _indexedDbModule!.InvokeVoidAsync("addCommand", json);
        }

    public async Task AddListAsync(TalonList list)
        {
            if (_indexedDbModule == null) await InitializeAsync();
            var json = JsonSerializer.Serialize(list);
            await _indexedDbModule!.InvokeVoidAsync("addList", json);
        }

        public async Task ClearAllDataAsync()
        {
            if (_indexedDbModule == null) await InitializeAsync();
            await _indexedDbModule!.InvokeVoidAsync("clearAllData");
        }
    }
}