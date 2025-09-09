using RazorClassLibrary.Models;
using DataAccessLibrary.Services;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Services
{
    public class TalonAnalysisService : ITalonAnalysisService
    {
        private readonly DataAccessLibrary.Services.ITalonVoiceCommandDataService _talonDataService;
        private readonly ILogger<TalonAnalysisService> _logger;

        public TalonAnalysisService(DataAccessLibrary.Services.ITalonVoiceCommandDataService talonDataService, ILogger<TalonAnalysisService> logger)
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
            var dbCommands = await _talonDataService.GetAllCommandsForFiltersAsync();            return dbCommands.Select(cmd => new TalonCommand
            {
                Command = cmd.Command ?? string.Empty,
                Mode = cmd.Mode ?? string.Empty,
                Script = cmd.Script ?? string.Empty,
                Application = cmd.Application ?? string.Empty,
                Repository = cmd.Repository ?? string.Empty,
                Title = cmd.Title ?? string.Empty,
                Tags = cmd.Tags ?? string.Empty,
                OperatingSystem = cmd.OperatingSystem ?? string.Empty,
                CodeLanguage = cmd.CodeLanguage ?? string.Empty,
                Language = cmd.Language ?? string.Empty,
                Hostname = cmd.Hostname ?? string.Empty
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
                })                .OrderByDescending(r => r.CommandCount)
                .ToList();            // Application statistics
            result.ApplicationStats = commands
                .GroupBy(c => c.Application, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ApplicationStats
                {
                    Application = g.Key,
                    CommandCount = g.Count(),
                    Percentage = commands.Count > 0 ? (double)g.Count() / commands.Count * 100 : 0
                })
                .OrderByDescending(a => a.CommandCount)
                .ToList();

            // Global conflicts analysis
            var globalConflicts = AnalyzeGlobalConflicts(commands);
            result.GlobalConflictDetails = globalConflicts;
            result.GlobalConflicts = globalConflicts.Count(c => c.IsTrueConflict);

            // Application-specific conflicts
            var appConflicts = AnalyzeApplicationConflicts(commands);
            result.AppConflictDetails = appConflicts;
            result.AppSpecificConflicts = appConflicts.Count(c => c.IsTrueConflict);

            result.TotalConflicts = result.GlobalConflicts + result.AppSpecificConflicts;

            // Update repository conflict counts
            UpdateRepositoryConflictCounts(result);

            return result;
        }        private List<ConflictDetail> AnalyzeGlobalConflicts(List<TalonCommand> commands)
        {
            return commands
                .Where(c => c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
                .GroupBy(c => c.Command.ToLower().Trim())
                .Where(g => g.Select(cmd => cmd.Repository.ToLower().Trim()).Distinct().Count() > 1) // Multiple repositories
                .Select(g => CreateConflictDetail(g.ToList(), "global"))
                .OrderByDescending(c => c.IsTrueConflict) // True conflicts first
                .ThenByDescending(c => c.InstanceCount)
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
            
            // Check for same-repository conflicts (these are always real conflicts)
            var sameRepoConflicts = commandInstances
                .GroupBy(c => c.Repository.ToLower().Trim())
                .Any(g => g.Count() > 1);
            
            if (sameRepoConflicts) return true;
            
            // For cross-repository commands, check if they could be active simultaneously
            if (AreCommandsMutuallyExclusiveByTags(commandInstances))
            {
                return false; // Not a conflict if they can't be active simultaneously
            }
            
            // If commands could be active simultaneously, check if they have similar implementations
            // Different implementations across repositories are expected and not conflicts
            return HasSimilarImplementations(commandInstances);
        }

        private bool HasSimilarImplementations(List<TalonCommand> commands)
        {
            // If there are only 2 commands, check if they're very similar
            if (commands.Count == 2)
            {
                return AreScriptsSimilar(commands[0].Script, commands[1].Script);
            }
            
            // For more than 2 commands, check if any pair has similar scripts
            for (int i = 0; i < commands.Count; i++)
            {
                for (int j = i + 1; j < commands.Count; j++)
                {
                    if (AreScriptsSimilar(commands[i].Script, commands[j].Script))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool AreScriptsSimilar(string script1, string script2)
        {
            if (string.IsNullOrEmpty(script1) || string.IsNullOrEmpty(script2))
                return false;
            
            // Normalize scripts for comparison
            var normalized1 = NormalizeScript(script1);
            var normalized2 = NormalizeScript(script2);
            
            // Exact match
            if (normalized1.Equals(normalized2, StringComparison.OrdinalIgnoreCase))
                return true;
            
            // Very similar (high similarity threshold)
            var similarity = CalculateStringSimilarity(normalized1, normalized2);
            return similarity > 0.8; // 80% similarity threshold
        }

        private string NormalizeScript(string script)
        {
            return script.Trim()
                        .Replace(" ", "")
                        .Replace("\t", "")
                        .Replace("\n", "")
                        .Replace("\r", "");
        }

        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (str1 == str2) return 1.0;
            if (str1.Length == 0 || str2.Length == 0) return 0.0;
            
            var maxLength = Math.Max(str1.Length, str2.Length);
            var distance = LevenshteinDistance(str1, str2);
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string str1, string str2)
        {
            var matrix = new int[str1.Length + 1, str2.Length + 1];
            
            for (int i = 0; i <= str1.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= str2.Length; j++)
                matrix[0, j] = j;
            
            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[str1.Length, str2.Length];
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
        }        private bool CouldCommandsBeActiveSimultaneously(TalonCommand cmd1, TalonCommand cmd2)
        {
            // Check operating system compatibility first
            if (!AreOperatingSystemsCompatible(cmd1.OperatingSystem, cmd2.OperatingSystem))
            {
                return false; // Commands for different OS can't be active simultaneously
            }

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

        private bool AreOperatingSystemsCompatible(string os1, string os2)
        {
            // Normalize OS strings
            var normalizedOs1 = NormalizeOperatingSystem(os1);
            var normalizedOs2 = NormalizeOperatingSystem(os2);
            
            // If either is empty/general, they could be compatible
            if (string.IsNullOrEmpty(normalizedOs1) || string.IsNullOrEmpty(normalizedOs2))
            {
                return true;
            }
            
            // If both specify the same OS, they're compatible
            if (normalizedOs1.Equals(normalizedOs2, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            // Different specific OS - not compatible
            return false;
        }

        private string NormalizeOperatingSystem(string os)
        {
            if (string.IsNullOrWhiteSpace(os))
                return string.Empty;
            
            var normalized = os.Trim().ToLowerInvariant();
            
            // Handle common variations
            if (normalized.Contains("win") || normalized == "windows")
                return "windows";
            if (normalized.Contains("mac") || normalized == "macos" || normalized == "darwin")
                return "mac";
            if (normalized.Contains("linux") || normalized == "unix")
                return "linux";
            
            return normalized;
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
        }        private ConflictDetail CreateConflictDetail(List<TalonCommand> commands, string application)
        {
            var firstCommand = commands.First();
            var repositories = commands.Select(c => c.Repository).Distinct().ToList();
            var scripts = commands.Select(c => c.Script).Distinct().ToList();

            // Determine conflict type
            var conflictType = DetermineConflictType(commands);
            var isTrueConflict = conflictType == ConflictType.TrueConflict;

            return new ConflictDetail
            {
                Command = firstCommand.Command,
                Application = application,
                Title = firstCommand.Title,
                Repositories = repositories,
                InstanceCount = commands.Count,
                HasDifferentImplementations = scripts.Count > 1,
                IsTrueConflict = isTrueConflict,
                ConflictType = conflictType,
                Implementations = repositories.Select(repo =>
                {
                    var repoCommand = commands.First(c => c.Repository.Equals(repo, StringComparison.OrdinalIgnoreCase));
                    var context = !string.IsNullOrEmpty(repoCommand.Title) && !repoCommand.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)
                        ? $"for {repoCommand.Title}" : "global";
                      return new ImplementationDetail
                    {
                        Repository = repo,
                        Script = repoCommand.Script.Length > 100 ? repoCommand.Script.Substring(0, 100) + "..." : repoCommand.Script,
                        Context = context,
                        Mode = repoCommand.Mode ?? string.Empty,
                        Tags = repoCommand.Tags ?? string.Empty,
                        OperatingSystem = repoCommand.OperatingSystem ?? string.Empty,
                        CodeLanguage = repoCommand.CodeLanguage ?? string.Empty,
                        Language = repoCommand.Language ?? string.Empty,
                        Hostname = repoCommand.Hostname ?? string.Empty
                    };
                }).ToList()
            };
        }        private ConflictType DetermineConflictType(List<TalonCommand> commands)
        {
            var repositories = commands.Select(c => c.Repository.ToLower().Trim()).Distinct().ToList();
            
            // Same repository conflicts are always true conflicts
            if (repositories.Count == 1)
                return ConflictType.TrueConflict;
            
            // Check if commands are OS-specific and therefore not conflicting
            if (AreCommandsOperatingSystemSpecific(commands))
                return ConflictType.OperatingSystemSpecific;
            
            // Check if commands are mutually exclusive by tags
            if (AreCommandsMutuallyExclusiveByTags(commands))
                return ConflictType.TagBasedMutuallyExclusive;
            
            // Check if implementations are similar (indicating likely duplication)
            if (HasSimilarImplementations(commands))
                return ConflictType.TrueConflict;
            
            // Different implementations across repos - these are alternatives, not conflicts
            return ConflictType.AlternativeImplementations;
        }

        private bool AreCommandsOperatingSystemSpecific(List<TalonCommand> commands)
        {
            // Get distinct operating systems
            var operatingSystems = commands
                .Select(c => NormalizeOperatingSystem(c.OperatingSystem))
                .Where(os => !string.IsNullOrEmpty(os))
                .Distinct()
                .ToList();
            
            // If we have multiple specific OS but they're different, commands won't conflict
            if (operatingSystems.Count > 1)
                return true;
            
            // If some commands are OS-specific and others are general, they won't conflict
            var hasGeneralCommands = commands.Any(c => string.IsNullOrWhiteSpace(c.OperatingSystem));
            var hasSpecificCommands = operatingSystems.Any();
            
            return hasGeneralCommands && hasSpecificCommands;
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
            }        }
    }
}