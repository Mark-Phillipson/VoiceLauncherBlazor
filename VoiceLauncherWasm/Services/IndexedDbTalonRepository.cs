using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RCLTalonShared.Models;
using RCLTalonShared.Services;
using VoiceLauncherWasm.Models;

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

        public async Task<IEnumerable<RCLTalonShared.Models.TalonList>> GetListsAsync()
        {
            var lists = await _indexedDb.GetAllListsAsync();
            // _indexedDb returns VoiceLauncherWasm.Models.TalonList; map to RCLTalonShared.Models.TalonList
            return lists.Select(l => new RCLTalonShared.Models.TalonList
            {
                Id = l.Id,
                ListName = l.ListName,
                Value = l.Value,
                Repository = l.Repository,
                DateCreated = l.DateCreated
            }).ToList();
        }

        public async Task<IEnumerable<RCLTalonShared.Models.TalonVoiceCommand>> GetCommandsAsync()
        {
            var commands = await _indexedDb.GetAllCommandsAsync();
            return commands.Select(c => new RCLTalonShared.Models.TalonVoiceCommand
            {
                Id = c.Id.ToString(),
                VoiceCommand = c.Command,
                TalonScript = c.Script,
                Application = c.Application,
                Repository = c.Repository,
                FilePath = c.FileName,
                DateCreated = c.DateCreated
            }).ToList();
        }

        public async Task SaveCommandsAsync(IEnumerable<RCLTalonShared.Models.TalonVoiceCommand> commands)
        {
            foreach (var c in commands)
            {
                // Map RCLTalonShared.Models.TalonVoiceCommand to VoiceLauncherWasm.Models.TalonVoiceCommand
                var voice = new VoiceLauncherWasm.Models.TalonVoiceCommand
                {
                    Id = int.TryParse(c.Id, out var idVal) ? idVal : 0,
                    Command = c.VoiceCommand,
                    Script = c.TalonScript,
                    Application = c.Application,
                    Repository = c.Repository,
                    FileName = c.FilePath,
                    DateCreated = c.DateCreated
                };
                await _indexedDb.AddCommandAsync(voice);
            }
        }

        public async Task SaveListsAsync(IEnumerable<RCLTalonShared.Models.TalonList> lists)
        {
            foreach (var l in lists)
            {
                var list = new VoiceLauncherWasm.Models.TalonList
                {
                    Id = l.Id,
                    ListName = l.ListName,
                    Value = l.Value,
                    Repository = l.Repository,
                    DateCreated = l.DateCreated
                };
                await _indexedDb.AddListAsync(list);
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
