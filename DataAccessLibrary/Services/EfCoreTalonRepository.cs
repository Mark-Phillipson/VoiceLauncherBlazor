using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RCLTalonSharedModels = RCLTalonShared.Models;
using RCLTalonShared.Services;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public class EfCoreTalonRepository : ITalonRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public EfCoreTalonRepository(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task DeleteAllAsync()
        {
            await using var db = _dbFactory.CreateDbContext();
            db.TalonVoiceCommands.RemoveRange(db.TalonVoiceCommands);
            db.TalonLists.RemoveRange(db.TalonLists);
            await db.SaveChangesAsync();
            return;
        }

        public async Task<IEnumerable<RCLTalonSharedModels.TalonList>> GetListsAsync()
        {
            await using var db = _dbFactory.CreateDbContext();
            var lists = await db.TalonLists.AsNoTracking().ToListAsync();
            return lists.Select(l => new RCLTalonSharedModels.TalonList
            {
                Id = l.Id.ToString(),
                ListName = l.ListName,
                Value = l.ListValue,
                Repository = l.SourceFile ?? string.Empty,
                DateCreated = l.CreatedAt
            }).ToList();
        }

        public async Task<IEnumerable<RCLTalonSharedModels.TalonVoiceCommand>> GetCommandsAsync()
        {
            await using var db = _dbFactory.CreateDbContext();
            var cmds = await db.TalonVoiceCommands.AsNoTracking().ToListAsync();
            return cmds.Select(c => new RCLTalonSharedModels.TalonVoiceCommand
            {
                Id = c.Id.ToString(),
                VoiceCommand = c.Command,
                TalonScript = c.Script,
                Application = c.Application ?? string.Empty,
                Repository = c.Repository ?? string.Empty,
                FilePath = c.FilePath ?? string.Empty,
                DateCreated = c.CreatedAt
            }).ToList();
        }

        public async Task ImportFromJsonAsync(string json)
        {
            var doc = JsonDocument.Parse(json);
            // Expect an object with arrays: commands and lists
            var root = doc.RootElement;
            if (root.TryGetProperty("commands", out var cmds))
            {
                var list = JsonSerializer.Deserialize<List<RCLTalonSharedModels.TalonVoiceCommand>>(cmds.GetRawText());
                if (list?.Any() == true)
                {
                    await SaveCommandsAsync(list);
                }
            }

            if (root.TryGetProperty("lists", out var listsEl))
            {
                var l = JsonSerializer.Deserialize<List<RCLTalonSharedModels.TalonList>>(listsEl.GetRawText());
                if (l?.Any() == true)
                {
                    await SaveListsAsync(l);
                }
            }
            return;
        }

    public async Task<string> ExportAllJsonAsync()
        {
            var commands = await GetCommandsAsync();
            var lists = await GetListsAsync();
            var export = new { commands, lists };
            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

    public async Task SaveCommandsAsync(IEnumerable<RCLTalonSharedModels.TalonVoiceCommand> commands)
        {
            await using var db = _dbFactory.CreateDbContext();
            foreach (var cmd in commands)
            {
        var entity = new DataAccessLibrary.Models.TalonVoiceCommand
                {
                    Command = cmd.VoiceCommand,
                    Script = cmd.TalonScript,
                    Application = string.IsNullOrWhiteSpace(cmd.Application) ? "global" : cmd.Application,
            FilePath = cmd.FilePath ?? string.Empty,
            Repository = cmd.Repository,
                    Title = null,
                    Mode = null,
                    OperatingSystem = null,
                    Tags = null,
                    CodeLanguage = null,
                    Language = null,
                    Hostname = null,
                    CreatedAt = cmd.DateCreated
                };
                db.TalonVoiceCommands.Add(entity);
            }
            await db.SaveChangesAsync();
        return;
        }

    public async Task SaveListsAsync(IEnumerable<RCLTalonSharedModels.TalonList> lists)
        {
            await using var db = _dbFactory.CreateDbContext();
            foreach (var l in lists)
            {
                var entity = new DataAccessLibrary.Models.TalonList
                {
                    ListName = l.ListName,
                    SpokenForm = l.ListName,
                    ListValue = l.Value,
                    SourceFile = l.Repository,
            CreatedAt = l.DateCreated,
            ImportedAt = DateTime.UtcNow
                };
                db.TalonLists.Add(entity);
            }
            await db.SaveChangesAsync();
        return;
        }
    }
}
