using CsvHelper;
using System.Globalization;

namespace TalonDuplicateAnalyzer;

public class TalonCommand
{
    public string Command { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class Program
{
    private static List<TalonCommand> _commands = new();

    public static async Task Main(string[] args)
    {
        try
        {
            // Load the CSV data
            string csvPath = Path.Combine("..", "VoiceLauncher", "TalonCommands.csv");
            await LoadTalonCommands(csvPath);

            Console.WriteLine("TALON COMMANDS CONFLICT ANALYSIS");
            Console.WriteLine("===============================");
            Console.WriteLine($"Loaded {_commands.Count} commands");
            Console.WriteLine("Looking for conflicting duplicates...\n");

            // Run focused conflict analysis
            AnalyzeGlobalCommandConflicts();
            AnalyzeApplicationCommandConflicts();
            ProvideSummaryAndRecommendations();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static async Task LoadTalonCommands(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"CSV file not found: {csvPath}");
        }

        using var reader = new StringReader(await File.ReadAllTextAsync(csvPath));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        _commands = csv.GetRecords<TalonCommand>()
            .Where(cmd => !string.IsNullOrWhiteSpace(cmd.Command))
            .ToList();

        Console.WriteLine($"Successfully loaded {_commands.Count} commands from {csvPath}");
    }    private static void AnalyzeGlobalCommandConflicts()
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("GLOBAL COMMAND CONFLICTS");
        Console.WriteLine("(Same global command from different repositories - excluding app-specific titles)");
        Console.WriteLine(new string('=', 70));

        var globalConflicts = _commands
            .Where(c => c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => c.Command.ToLower().Trim())
            .Where(g => {
                // Check if this is a true conflict - exclude commands that are app-specific via Title
                var commandInstances = g.ToList();
                var repositories = commandInstances.Select(c => c.Repository.ToLower().Trim()).Distinct().ToList();
                
                // If only one repository, not a conflict
                if (repositories.Count <= 1) return false;
                
                // Check if commands have specific app titles that make them non-conflicting
                var trueGlobalCommands = commandInstances.Where(c => 
                    string.IsNullOrEmpty(c.Title) || 
                    c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
                var appSpecificCommands = commandInstances.Where(c => 
                    !string.IsNullOrEmpty(c.Title) && 
                    !c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
                
                // If all commands are app-specific with different titles, not a true global conflict
                if (trueGlobalCommands.Count == 0 && appSpecificCommands.Count > 0)
                {
                    var uniqueTitles = appSpecificCommands.Select(c => c.Title.ToLower().Trim()).Distinct().ToList();
                    return uniqueTitles.Count < appSpecificCommands.Count; // Only conflict if same title from different repos
                }
                
                // If we have mix of true global and app-specific, or multiple true globals, it's a conflict
                return trueGlobalCommands.GroupBy(c => c.Repository.ToLower().Trim()).Count() > 1 ||
                       (trueGlobalCommands.Any() && appSpecificCommands.Any());
            })
            .OrderByDescending(g => g.Count())
            .ToList();        if (globalConflicts.Any())
        {
            Console.WriteLine($"Found {globalConflicts.Count} global command conflicts:\n");
            
            foreach (var conflict in globalConflicts)
            {
                var commands = conflict.ToList();
                var repositories = commands.Select(c => c.Repository).Distinct().ToList();
                var scripts = commands.Select(c => c.Script).Distinct().ToList();
                
                // Separate true global from app-specific
                var trueGlobal = commands.Where(c => 
                    string.IsNullOrEmpty(c.Title) || 
                    c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
                var appSpecific = commands.Where(c => 
                    !string.IsNullOrEmpty(c.Title) && 
                    !c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase)).ToList();
                
                Console.WriteLine($"🚨 GLOBAL CONFLICT: '{commands.First().Command}'");
                Console.WriteLine($"  Repositories: {string.Join(" vs ", repositories)}");
                Console.WriteLine($"  Instances: {commands.Count}");
                
                if (trueGlobal.Any())
                {
                    Console.WriteLine($"  True Global: {string.Join(", ", trueGlobal.Select(c => c.Repository).Distinct())}");
                }
                
                if (appSpecific.Any())
                {
                    var titleGroups = appSpecific.GroupBy(c => c.Title).ToList();
                    Console.WriteLine($"  App-Specific: {string.Join(", ", titleGroups.Select(g => $"{g.Key} ({string.Join(", ", g.Select(c => c.Repository).Distinct())})"))}");
                }
                
                if (scripts.Count > 1)
                {
                    Console.WriteLine($"  ⚠️  CONFLICTING IMPLEMENTATIONS:");
                    foreach (var repo in repositories)
                    {
                        var repoCommands = commands.Where(c => c.Repository.Equals(repo, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (repoCommands.Any())
                        {
                            var repoCommand = repoCommands.First();
                            var repoScript = repoCommand.Script;
                            var titleInfo = !string.IsNullOrEmpty(repoCommand.Title) && !repoCommand.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase) 
                                ? $" (for {repoCommand.Title})" : " (global)";
                            Console.WriteLine($"    [{repo}]{titleInfo}: {(repoScript.Length > 80 ? repoScript.Substring(0, 80) + "..." : repoScript)}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"  ✓ Same implementation (may still cause conflicts)");
                }
                
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("No global command conflicts found.");
        }
        Console.WriteLine();
    }    private static void AnalyzeApplicationCommandConflicts()
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("APPLICATION-SPECIFIC COMMAND CONFLICTS");
        Console.WriteLine("(Same command+app from different repositories, considering titles)");
        Console.WriteLine(new string('=', 70));

        var appConflicts = _commands
            .Where(c => !c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => new { 
                Command = c.Command.ToLower().Trim(), 
                Application = c.Application.ToLower().Trim(),
                Title = string.IsNullOrEmpty(c.Title) || c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase) 
                    ? "" : c.Title.ToLower().Trim()
            })
            .Where(g => g.Select(c => c.Repository.ToLower().Trim()).Distinct().Count() > 1)
            .OrderByDescending(g => g.Count())
            .Take(20) // Show top 20
            .ToList();

        if (appConflicts.Any())
        {
            Console.WriteLine($"Found {appConflicts.Count} application-specific conflicts (showing top 20):\n");
            
            foreach (var conflict in appConflicts)
            {
                var commands = conflict.ToList();
                var repositories = commands.Select(c => c.Repository).Distinct().ToList();
                var scripts = commands.Select(c => c.Script).Distinct().ToList();
                  Console.WriteLine($"🔥 APP CONFLICT: '{commands.First().Command}' in '{commands.First().Application}'");
                if (!string.IsNullOrEmpty(conflict.Key.Title))
                {
                    Console.WriteLine($"  Title: '{commands.First().Title}'");
                }
                Console.WriteLine($"  Repositories: {string.Join(" vs ", repositories)}");
                Console.WriteLine($"  Instances: {commands.Count}");
                
                if (scripts.Count > 1)
                {
                    Console.WriteLine($"  ⚠️  Different implementations detected");
                }
                
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("No application-specific command conflicts found.");
        }
        Console.WriteLine();
    }

    private static void ProvideSummaryAndRecommendations()
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("CONFLICT ANALYSIS SUMMARY & RECOMMENDATIONS");
        Console.WriteLine(new string('=', 70));        // Calculate conflict statistics with Title consideration
        var totalConflicts = _commands
            .GroupBy(c => new { 
                Command = c.Command.ToLower().Trim(), 
                Application = c.Application.ToLower().Trim(),
                Title = string.IsNullOrEmpty(c.Title) || c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase) 
                    ? "" : c.Title.ToLower().Trim()
            })
            .Count(g => g.Select(c => c.Repository.ToLower().Trim()).Distinct().Count() > 1);

        var globalConflicts = _commands
            .Where(c => c.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => c.Command.ToLower().Trim())
            .Count(g => {
                var commandInstances = g.ToList();
                var repositories = commandInstances.Select(c => c.Repository.ToLower().Trim()).Distinct().ToList();
                
                if (repositories.Count <= 1) return false;
                
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
            });

        var appConflicts = totalConflicts - globalConflicts;

        Console.WriteLine($"📊 CONFLICT STATISTICS:");
        Console.WriteLine($"  Total conflicting commands: {totalConflicts}");
        Console.WriteLine($"  Global command conflicts: {globalConflicts}");
        Console.WriteLine($"  App-specific conflicts: {appConflicts}");
        Console.WriteLine();

        Console.WriteLine($"🔧 RECOMMENDATIONS:");
        Console.WriteLine($"  1. Review global conflicts first - these are most critical");
        Console.WriteLine($"  2. For conflicts with different scripts, decide which implementation to keep");
        Console.WriteLine($"  3. Consider renaming commands or using different voice triggers");
        Console.WriteLine($"  4. Consolidate similar functionality across repositories");
        Console.WriteLine($"  5. Use application-specific commands instead of global when possible");
        Console.WriteLine();        // Repository conflict analysis with Title consideration
        var repoConflictAnalysis = _commands
            .GroupBy(c => new { 
                Command = c.Command.ToLower().Trim(), 
                Application = c.Application.ToLower().Trim(),
                Title = string.IsNullOrEmpty(c.Title) || c.Title.Equals("NULL", StringComparison.OrdinalIgnoreCase) 
                    ? "" : c.Title.ToLower().Trim()
            })
            .Where(g => g.Select(c => c.Repository.ToLower().Trim()).Distinct().Count() > 1)
            .SelectMany(g => g.Select(c => c.Repository))
            .GroupBy(repo => repo)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .ToList();

        if (repoConflictAnalysis.Any())
        {
            Console.WriteLine($"🏆 REPOSITORIES WITH MOST CONFLICTS:");
            foreach (var repo in repoConflictAnalysis)
            {
                Console.WriteLine($"  {repo.Key}: {repo.Count()} conflicting commands");
            }
        }

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine("ANALYSIS COMPLETE");
        Console.WriteLine(new string('=', 70));
    }
}
