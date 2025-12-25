using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using RazorClassLibrary.Services;
using TalonVoiceCommandsServer.Services;
using DataAccessLibrary.Models;

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

        // Not used in these tests
        public Task<List<TalonList>> GetListsAsync() => Task.FromResult(new List<TalonList>());
        public Task<TalonVoiceCommand?> GetCommandById(int id) => Task.FromResult<TalonVoiceCommand?>(null);
        public Task<int> SaveCommandAsync(TalonVoiceCommand command) => Task.FromResult(0);
        public Task<int> SaveListAsync(TalonList list) => Task.FromResult(0);
        public Task DeleteCommandAsync(int id) => Task.CompletedTask;
        public Task DeleteListAsync(int id) => Task.CompletedTask;
    }
}