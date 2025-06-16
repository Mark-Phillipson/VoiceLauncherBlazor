using RazorClassLibrary.Models;
using DataAccessLibrary.Services;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Services
{
    public interface ITalonAnalysisService
    {
        Task<TalonAnalysisResult> AnalyzeCommandsAsync();
    }

    public class TalonAnalysisService : ITalonAnalysisService
    {
        private readonly TalonVoiceCommandDataService _talonDataService;
        private readonly ILogger<TalonAnalysisService> _logger;

        public TalonAnalysisService(TalonVoiceCommandDataService talonDataService, ILogger<TalonAnalysisService> logger)
        {
            _talonDataService = talonDataService;
            _logger = logger;
        }

        public async Task<TalonAnalysisResult> AnalyzeCommandsAsync()
        {
            try
            {
                var commands = await LoadTalonCommandsFromDatabaseAsync();
                return AnalyzeCommands(commands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing Talon commands");
                return new TalonAnalysisResult();
            }
        }

        private async Task<List<TalonCommand>> LoadTalonCommandsFromDatabaseAsync()
        {
            // Get all Talon voice commands from the database
            var dbCommands = await _talonDataService.GetAllCommandsForFiltersAsync();
              return dbCommands.Select(cmd => new TalonCommand
            {
                Command = cmd.Command ?? string.Empty,
                Mode = cmd.Mode ?? string.Empty,
                Script = cmd.Script ?? string.Empty,
                Application = cmd.Application ?? string.Empty,
                Repository = cmd.Repository ?? string.Empty,
                Title = cmd.Title ?? string.Empty,
                Tags = cmd.Tags ?? string.Empty
            }).ToList();
        }

        private TalonAnalysisResult AnalyzeCommands(List<TalonCommand> commands)
        {
            var result = new TalonAnalysisResult
            {
                TotalCommands = commands.Count,
                UniqueCommands = commands.Select(c => c.Command).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            };

            // Repository statistics
            result.RepositoryStats = commands
                .GroupBy(c => c.Repository, StringComparer.OrdinalIgnoreCase)
                .Select(g => new RepositoryStats
                {
                    Repository = g.Key,
                    CommandCount = g.Count()
                })
                .OrderByDescending(r => r.CommandCount)
                .ToList();

            // Global conflicts analysis
            var globalConflicts = AnalyzeGlobalConflicts(commands);
            result.GlobalConflictDetails = globalConflicts;
            result.GlobalConflicts = globalConflicts.Count;

            // Application-specific conflicts
            var appConflicts = AnalyzeApplicationConflicts(commands);
            result.AppConflictDetails = appConflicts;
            result.AppSpecificConflicts = appConflicts.Count;

            result.TotalConflicts = result.GlobalConflicts + result.AppSpecificConflicts;

            // Update repository conflict counts
            UpdateRepositoryConflictCounts(result);

            return result;
        }

        private List<ConflictDetail> AnalyzeGlobalConflicts(List<TalonCommand> commands)
        {
            return commands
                .Where(c => c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
                .GroupBy(c => c.Command.ToLower().Trim())
                .Where(g => HasTrueConflict(g.ToList()))
                .Select(g => CreateConflictDetail(g.ToList(), "global"))
                .OrderByDescending(c => c.InstanceCount)
                .ToList();
        }

        private List<ConflictDetail> AnalyzeApplicationConflicts(List<TalonCommand> commands)
        {
            return commands
                .Where(c => !c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
                .GroupBy(c => new { 
                    Command = c.Command.ToLower().Trim(), 
                    Application = c.Application.ToLower().Trim(),
                    Title = string.IsNullOrEmpty(c.Title) || c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase) 
                        ? "" : c.Title.ToLower().Trim()
                })
                .Where(g => g.Select(c => c.Repository.ToLower().Trim()).Distinct().Count() > 1)
                .Select(g => CreateConflictDetail(g.ToList(), g.First().Application))
                .OrderByDescending(c => c.InstanceCount)
                .Take(20)
                .ToList();
        }        private bool HasTrueConflict(List<TalonCommand> commandInstances)
        {
            var repositories = commandInstances.Select(c => c.Repository.ToLower().Trim()).Distinct().ToList();
            
            if (repositories.Count <= 1) return false;
            
            // Check if commands are mutually exclusive due to tags
            if (AreCommandsMutuallyExclusiveByTags(commandInstances))
            {
                return false; // Not a conflict if they can't be active simultaneously
            }
            
            var trueGlobalCommands = commandInstances.Where(c => 
                string.IsNullOrEmpty(c.Title) || 
                c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
            var appSpecificCommands = commandInstances.Where(c => 
                !string.IsNullOrEmpty(c.Title) && 
                !c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (trueGlobalCommands.Count == 0 && appSpecificCommands.Count > 0)
            {
                var uniqueTitles = appSpecificCommands.Select(c => c.Title.ToLower().Trim()).Distinct().ToList();
                return uniqueTitles.Count < appSpecificCommands.Count;
            }
            
            return trueGlobalCommands.GroupBy(c => c.Repository.ToLower().Trim()).Count() > 1 ||
                   (trueGlobalCommands.Any() && appSpecificCommands.Any());
        }

        private bool AreCommandsMutuallyExclusiveByTags(List<TalonCommand> commands)
        {
            // Group commands by repository to analyze tag conflicts
            var commandsByRepo = commands.GroupBy(c => c.Repository.ToLower().Trim()).ToList();
            
            // If all commands are from the same repository, no conflict
            if (commandsByRepo.Count <= 1) return true;
            
            // Check if any two commands from different repositories could be active simultaneously
            for (int i = 0; i < commandsByRepo.Count; i++)
            {
                for (int j = i + 1; j < commandsByRepo.Count; j++)
                {
                    var repo1Commands = commandsByRepo[i].ToList();
                    var repo2Commands = commandsByRepo[j].ToList();
                    
                    // Check if any command from repo1 could conflict with any command from repo2
                    foreach (var cmd1 in repo1Commands)
                    {
                        foreach (var cmd2 in repo2Commands)
                        {
                            if (CouldCommandsBeActiveSimultaneously(cmd1, cmd2))
                            {
                                return false; // Found a potential conflict
                            }
                        }
                    }
                }
            }
            
            return true; // All commands are mutually exclusive
        }

        private bool CouldCommandsBeActiveSimultaneously(TalonCommand cmd1, TalonCommand cmd2)
        {
            var tags1 = ParseTags(cmd1.Tags);
            var tags2 = ParseTags(cmd2.Tags);
            
            // If either command has no tags, they could potentially be active simultaneously
            if (!tags1.Any() || !tags2.Any())
            {
                return true;
            }
            
            // If the commands share any common tags, they could be active simultaneously
            // If they have completely different tag sets, they're mutually exclusive
            return tags1.Intersect(tags2, StringComparer.OrdinalIgnoreCase).Any();
        }

        private List<string> ParseTags(string tagString)
        {
            if (string.IsNullOrWhiteSpace(tagString))
            {
                return new List<string>();
            }
            
            // Tags are typically comma-separated in the database
            return tagString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrEmpty(t))
                           .ToList();
        }

        private ConflictDetail CreateConflictDetail(List<TalonCommand> commands, string application)
        {
            var firstCommand = commands.First();
            var repositories = commands.Select(c => c.Repository).Distinct().ToList();
            var scripts = commands.Select(c => c.Script).Distinct().ToList();

            return new ConflictDetail
            {
                Command = firstCommand.Command,
                Application = application,
                Title = firstCommand.Title,
                Repositories = repositories,
                InstanceCount = commands.Count,
                HasDifferentImplementations = scripts.Count > 1,
                Implementations = repositories.Select(repo =>
                {
                    var repoCommand = commands.First(c => c.Repository.Equals(repo, StringComparison.OrdinalIgnoreCase));
                    var context = !string.IsNullOrEmpty(repoCommand.Title) && !repoCommand.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)
                        ? $"for {repoCommand.Title}" : "global";
                    
                    return new ImplementationDetail
                    {
                        Repository = repo,
                        Script = repoCommand.Script.Length > 100 ? repoCommand.Script.Substring(0, 100) + "..." : repoCommand.Script,
                        Context = context
                    };
                }).ToList()
            };
        }

        private void UpdateRepositoryConflictCounts(TalonAnalysisResult result)
        {
            var conflictsByRepo = result.GlobalConflictDetails
                .Concat(result.AppConflictDetails)
                .SelectMany(c => c.Repositories)
                .GroupBy(repo => repo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (var repoStat in result.RepositoryStats)
            {
                if (conflictsByRepo.TryGetValue(repoStat.Repository, out var conflictCount))
                {
                    repoStat.ConflictCount = conflictCount;
                }
            }
        }
    }
}
