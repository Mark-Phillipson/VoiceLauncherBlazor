using System.Collections.Generic;
using System.Threading.Tasks;
using RCLTalonShared.Models;
using RCLTalonShared.Services;

namespace VoiceLauncherWasm.Services
{
    public class IndexedDbTalonRepository : ITalonRepository
    {
        private readonly IIndexedDBService _indexedDb;

        public IndexedDbTalonRepository(IIndexedDBService indexedDb)
        {
            _indexedDb = indexedDb;
        }

        public async Task DeleteAllAsync()
        {
            await _indexedDb.ClearAllDataAsync();
        }

        public async Task<IEnumerable<TalonList>> GetListsAsync()
        {
            return await _indexedDb.GetAllListsAsync();
        }

        public async Task<IEnumerable<TalonVoiceCommand>> GetCommandsAsync()
        {
            return await _indexedDb.GetAllCommandsAsync();
        }

        public async Task SaveCommandsAsync(IEnumerable<TalonVoiceCommand> commands)
        {
            foreach (var c in commands)
            {
                await _indexedDb.AddCommandAsync(c);
            }
        }

        public async Task SaveListsAsync(IEnumerable<TalonList> lists)
        {
            foreach (var l in lists)
            {
                await _indexedDb.AddListAsync(l);
            }
        }

        public async Task<string> ExportAllJsonAsync()
        {
            // Not implemented in this prototype
            return await Task.FromResult(string.Empty);
        }

        public async Task ImportFromJsonAsync(string json)
        {
            // Not implemented in this prototype
            await Task.CompletedTask;
        }
    }
}
