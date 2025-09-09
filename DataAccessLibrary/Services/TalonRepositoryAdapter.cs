using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedContracts.Models;
using SharedContracts.Services;

namespace DataAccessLibrary.Services
{
    // Adapter that exposes the existing TalonVoiceCommandDataService via the shared interface
    public class TalonRepositoryAdapter : ITalonVoiceCommandRepository
    {
        private readonly TalonVoiceCommandDataService _inner;

        public TalonRepositoryAdapter(TalonVoiceCommandDataService inner)
        {
            _inner = inner;
        }

        public async Task DeleteAllAsync()
        {
            // Not supported here; could call import with empty data
            await Task.CompletedTask;
        }

        public async Task ImportFromJsonAsync(string json)
        {
            // Not implemented
            await Task.CompletedTask;
        }

        public async Task<string> ExportAllJsonAsync()
        {
            // Not implemented
            return string.Empty;
        }

        public async Task<IEnumerable<TalonListDto>> GetListContentsAsync(string listName)
        {
            var lists = await _inner.GetListContentsAsync(listName);
            return lists.Select(l => new TalonListDto
            {
                Id = l.Id,
                ListName = l.ListName,
                SpokenForm = l.SpokenForm,
                ListValue = l.ListValue,
                SourceFile = l.SourceFile,
                CreatedAt = l.CreatedAt
            });
        }

        public async Task<IEnumerable<TalonVoiceCommandDto>> GetAllCommandsAsync()
        {
            var cmds = await _inner.GetAllCommandsForFiltersAsync();
            return cmds.Select(c => new TalonVoiceCommandDto
            {
                Id = c.Id,
                Command = c.Command,
                Script = c.Script,
                Application = c.Application,
                Title = c.Title,
                Mode = c.Mode,
                OperatingSystem = c.OperatingSystem,
                FilePath = c.FilePath,
                Repository = c.Repository,
                Tags = c.Tags,
                CodeLanguage = c.CodeLanguage,
                Language = c.Language,
                Hostname = c.Hostname,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<IEnumerable<TalonVoiceCommandDto>> SearchCommandsAsync(string searchTerm)
        {
            var results = await _inner.SearchAllAsync(searchTerm);
            return results.Select(c => new TalonVoiceCommandDto
            {
                Id = c.Id,
                Command = c.Command,
                Script = c.Script,
                Application = c.Application,
                Title = c.Title,
                Mode = c.Mode,
                OperatingSystem = c.OperatingSystem,
                FilePath = c.FilePath,
                Repository = c.Repository,
                Tags = c.Tags,
                CodeLanguage = c.CodeLanguage,
                Language = c.Language,
                Hostname = c.Hostname,
                CreatedAt = c.CreatedAt
            });
        }
    }
}
