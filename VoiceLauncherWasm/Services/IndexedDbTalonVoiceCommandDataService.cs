using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RCLTalonShared.Models;
using VoiceLauncherWasm.Services;
using SharedContracts.Services;
using SharedContracts.Models;

namespace VoiceLauncherWasm.Services
{
    // Lightweight WASM-compatible implementation that reads from IIndexedDBService
    public class IndexedDbTalonVoiceCommandDataService : ITalonVoiceCommandRepository
    {
        private readonly IIndexedDBService _indexedDb;

        public IndexedDbTalonVoiceCommandDataService(IIndexedDBService indexedDb)
        {
            _indexedDb = indexedDb;
        }

        public async Task<int> ImportFromTalonFilesAsync(string rootFolder)
        {
            // Not supported in WASM; return 0
            return await Task.FromResult(0);
        }
        public async Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsAsync()
        {
            var commands = await _indexedDb.GetAllCommandsAsync();
            // Map RCLTalonShared.Models.TalonVoiceCommand -> SharedContracts.Models.TalonVoiceCommandDto
            return commands.Select(c => new TalonVoiceCommandDto
            {
                Id = 0,
                Command = c.VoiceCommand ?? string.Empty,
                Script = c.TalonScript ?? string.Empty,
                Application = string.IsNullOrWhiteSpace(c.Application) ? "global" : c.Application,
                Title = null,
                Mode = null,
                OperatingSystem = null,
                FilePath = c.FilePath ?? string.Empty,
                Repository = c.Repository,
                Tags = null,
                CodeLanguage = null,
                Language = null,
                Hostname = null,
                CreatedAt = c.DateCreated
            }).ToList();
        }

        // Backwards-compatible method implementations
        public Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsForFiltersAsync()
        {
            // For now, return all commands. Filtering will be implemented later.
            return GetAllCommandsAsync();
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchAsync(string searchTerm)
        {
            // Fallback to simple search
            return SearchCommandsAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchWithListsAsync(string searchTerm)
        {
            return SearchCommandsAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            return SearchCommandsAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchScriptOnlyAsync(string searchTerm)
        {
            return SearchCommandsAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchAllAsync(string searchTerm)
        {
            return SearchCommandsAsync(searchTerm);
        }

        // For semantic searches we'll fallback to simple text matching in WASM
        public async Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandsAsync(string searchTerm)
        {
            var all = (await GetAllCommandsAsync()).ToList();
            if (string.IsNullOrWhiteSpace(searchTerm)) return all.OrderByDescending(c => c.CreatedAt).Take(100);
            var lower = searchTerm.ToLower();
            return all.Where(c => (c.Command ?? "").ToLower().Contains(lower) || (c.Script ?? "").ToLower().Contains(lower) || ((c.Title ?? "").ToLower().Contains(lower)))
                      .OrderByDescending(c => c.CreatedAt).Take(100);
        }

        public async Task<IEnumerable<TalonListDto>> GetListContentsAsync(string listName)
        {
            var lists = await _indexedDb.GetAllListsAsync();
            var matches = lists.Where(l => l.ListName == listName || (listName.StartsWith("user.") ? l.ListName == listName : l.ListName == $"user.{listName}"))
                               .Select(l => new TalonListDto { Id = 0, ListName = l.ListName, SpokenForm = l.ListName, ListValue = l.Value, SourceFile = l.Repository, CreatedAt = l.DateCreated })
                               .ToList();
            return matches;
        }

        public Task<string> ExportAllJsonAsync()
        {
            // Not implemented in this prototype
            return Task.FromResult(string.Empty);
        }

        public async Task ImportFromJsonAsync(string json)
        {
            // Not implemented in this prototype
            await Task.CompletedTask;
        }

        public async Task DeleteAllAsync()
        {
            await _indexedDb.ClearAllDataAsync();
        }
    }
}
