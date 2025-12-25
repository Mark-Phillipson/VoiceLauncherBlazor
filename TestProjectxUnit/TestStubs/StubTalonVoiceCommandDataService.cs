using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using DataAccessLibrary.Services;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTO;




namespace TestProjectxUnit.TestStubs
{
    public class StubTalonVoiceCommandDataService : ITalonVoiceCommandDataService
    {
        private readonly List<TalonVoiceCommand> _commands = new();

        public void SetCommands(IEnumerable<TalonVoiceCommand> commands)
        {
            _commands.Clear();
            _commands.AddRange(commands);
        }

        public Task InitializeAsync(IJSRuntime? jsRuntime = null)
        {
            return Task.CompletedTask;
        }

        public Task<List<TalonVoiceCommand>> GetFilteredCommandsInMemory(string? searchTerm = null, string? application = null, string? mode = null, string? operatingSystem = null, string? repository = null, string? tags = null, string? title = null, string? codeLanguage = null, bool useSemanticMatching = false, int searchScope = 0)
        {
            var q = _commands.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm)) q = q.Where(c => c.Command.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(application)) q = q.Where(c => c.Application == application);
            if (!string.IsNullOrWhiteSpace(title)) q = q.Where(c => c.Title == title);
            return Task.FromResult(q.ToList());
        }

        // Other interface methods can be no-op or simple implementations
        public Task<int> SearchAndDisplayDirectlyAsync(string? searchTerm = null, string? application = null, string? mode = null, string? operatingSystem = null, string? repository = null, string? tags = null, string? title = null, string? codeLanguage = null, bool useSemanticMatching = false, int searchScope = 0, int maxResults = 500)
        {
            var results = _commands.Where(c => string.IsNullOrEmpty(searchTerm) || c.Command.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(results.Count());
        }

        public Task<List<TalonVoiceCommand>> SearchFilteredCommandsSimpleAsync(string? searchTerm = null, string? application = null, string? mode = null, string? operatingSystem = null, string? repository = null, string? tags = null, string? title = null, string? codeLanguage = null, bool useSemanticMatching = false, int searchScope = 0, int maxResults = 500)
        {
            var list = _commands.Where(c => (string.IsNullOrEmpty(searchTerm) || c.Command.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(application) || c.Application == application) && (string.IsNullOrEmpty(title) || c.Title == title)).Take(maxResults).ToList();
            return Task.FromResult(list);
        }

        // Minimal implementations for the rest of the interface to avoid test failures
        public Task<List<TalonVoiceCommand>> GetFilteredCommandsInMemory(string? searchTerm = null, string? application = null, string? mode = null, string? operatingSystem = null, string? repository = null, string? tags = null, string? title = null, string? codeLanguage = null, bool useSemanticMatching = false, int searchScope = 0, bool includeHidden = false)
        {
            return GetFilteredCommandsInMemory(searchTerm, application, mode, operatingSystem, repository, tags, title, codeLanguage, useSemanticMatching, searchScope);
        }

        public Task EnsureLoadedFromIndexedDBAsync(IJSRuntime? jsRuntime = null)
        {
            return Task.CompletedTask;
        }

        // Not used in these tests (generic helpers)
        public Task<List<TalonList>> GetListsAsync() => Task.FromResult(new List<TalonList>());
        public Task<TalonVoiceCommand?> GetCommandById(int id) => Task.FromResult<TalonVoiceCommand?>(null);
        public Task<int> SaveCommandAsync(TalonVoiceCommand command) => Task.FromResult(0);
        public Task<int> SaveListAsync(TalonList list) => Task.FromResult(0);
        public Task DeleteCommandAsync(int id) => Task.CompletedTask;
        public Task DeleteListAsync(int id) => Task.CompletedTask;

        // Implement DataAccessLibrary.ITalonVoiceCommandDataService methods used by the Razor component
        public Task<int> ImportFromTalonFilesAsync(string rootFolder) => Task.FromResult(0);

        public Task<List<TalonVoiceCommand>> GetAllCommandsForFiltersAsync()
        {
            return Task.FromResult(_commands.ToList());
        }

        public Task<List<TalonVoiceCommand>> SemanticSearchAsync(string searchTerm)
        {
            var results = _commands.Where(c => c.Command.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(results);
        }

        public Task<List<TalonVoiceCommand>> SemanticSearchWithListsAsync(string searchTerm)
        {
            return SemanticSearchAsync(searchTerm);
        }

        public Task<List<TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            var results = _commands.Where(c => c.Command.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(results);
        }

        public Task<List<TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm)
        {
            var results = _commands.Where(c => c.Script != null && c.Script.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(results);
        }

        public Task<List<TalonVoiceCommand>> SearchAllAsync(string searchTerm)
        {
            var results = _commands.Where(c => (c.Command != null && c.Command.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase)) || (c.Script != null && c.Script.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase)) || (c.Title != null && c.Title.Contains(searchTerm ?? string.Empty, StringComparison.OrdinalIgnoreCase))).ToList();
            return Task.FromResult(results);
        }

        public Task<List<TalonList>> GetListContentsAsync(string listName)
        {
            return Task.FromResult(new List<TalonList>());
        }

        public Task<int> ImportTalonFileContentAsync(string fileContent, string fileName) => Task.FromResult(0);

        public Task<int> ImportAllTalonFilesWithProgressAsync(string rootFolder, Action<int, int, int>? progressCallback = null) => Task.FromResult(0);

        public Task<int> ImportTalonListsFromFileAsync(string filePath) => Task.FromResult(0);

        public Task<List<CommandsBreakdown>> GetTalonCommandsBreakdownAsync() => Task.FromResult(new List<CommandsBreakdown>());

        public Task<List<TalonVoiceCommand>> GetRandomCommandsAsync(int count, string os)
        {
            return Task.FromResult(_commands.Take(count).ToList());
        }
    }
}