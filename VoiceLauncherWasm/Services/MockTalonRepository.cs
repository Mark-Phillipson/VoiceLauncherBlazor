using SharedContracts.Models;
using SharedContracts.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherWasm.Services
{
    public class MockTalonRepository : ITalonVoiceCommandRepository
    {
        private readonly List<TalonVoiceCommandDto> _data = new()
        {
            new TalonVoiceCommandDto { Id = 1, Command = "open file", Application = "code", Script = "open_file" },
            new TalonVoiceCommandDto { Id = 2, Command = "close tab", Application = "browser", Script = "close_tab" },
            new TalonVoiceCommandDto { Id = 3, Command = "run tests", Application = "terminal", Script = "run_tests" }
        };

        public Task DeleteAllAsync()
        {
            _data.Clear();
            return Task.CompletedTask;
        }

        public Task<string> ExportAllJsonAsync()
        {
            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(_data));
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsAsync()
        {
            return Task.FromResult(_data.AsEnumerable());
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsForFiltersAsync()
        {
            return Task.FromResult(_data.AsEnumerable());
        }

        public Task<IEnumerable<TalonListDto>> GetListContentsAsync(string listName)
        {
            return Task.FromResult(Enumerable.Empty<TalonListDto>());
        }

        public Task ImportFromJsonAsync(string json)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchAsync(string searchTerm)
        {
            return SearchAllAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SemanticSearchWithListsAsync(string searchTerm)
        {
            return SearchAllAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchAllAsync(string searchTerm)
        {
            var q = _data.Where(d => d.Command.Contains(searchTerm) || (d.Script ?? "").Contains(searchTerm));
            return Task.FromResult(q.AsEnumerable());
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            var q = _data.Where(d => d.Command.Contains(searchTerm));
            return Task.FromResult(q.AsEnumerable());
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandsAsync(string searchTerm)
        {
            return SearchAllAsync(searchTerm);
        }

        public Task<IEnumerable<TalonVoiceCommandDto>> SearchScriptOnlyAsync(string searchTerm)
        {
            var q = _data.Where(d => (d.Script ?? "").Contains(searchTerm));
            return Task.FromResult(q.AsEnumerable());
        }
    }
}
