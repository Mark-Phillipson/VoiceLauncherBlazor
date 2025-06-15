using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Services
{
    public class TalonVoiceCommandDataService
    {
        private readonly ApplicationDbContext _context;
        public TalonVoiceCommandDataService(ApplicationDbContext context)
        {
            _context = context;
        }        /// <summary>        /// <summary>
                 /// Extracts the repository name from a file path by finding the first subdirectory after the 'user' directory
                 /// For example: C:\Users\MPhil\AppData\Roaming\talon\user\community\file.talon -> community
                 /// </summary>
        private string? ExtractRepositoryFromPath(string filePath)
        {
            try
            {
                var normalizedPath = Path.GetFullPath(filePath).Replace('\\', '/');

                // Look for the '/user/' pattern that indicates the talon user directory
                var userDirPattern = "/user/";
                var userIndex = normalizedPath.IndexOf(userDirPattern, StringComparison.OrdinalIgnoreCase);

                if (userIndex >= 0)
                {
                    // Find the path after the user directory
                    var userDirStart = userIndex + userDirPattern.Length;
                    var pathAfterUser = normalizedPath.Substring(userDirStart);

                    // Split the path and get the parts
                    var pathParts = pathAfterUser.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    // The first directory after the user directory is the repository
                    // Format: .../user/community/... 
                    // pathParts would be: ["community", ...]
                    // We want the first part (index 0) which is the repository

                    if (pathParts.Length >= 1 && !string.IsNullOrWhiteSpace(pathParts[0]))
                    {
                        return pathParts[0];
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> ImportFromTalonFilesAsync(string rootFolder)
        {
            // Remove all existing records before importing new ones
            _context.TalonVoiceCommands.RemoveRange(_context.TalonVoiceCommands);
            await _context.SaveChangesAsync();
            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            var commands = new List<TalonVoiceCommand>(); foreach (var file in talonFiles)
            {
                var lines = await File.ReadAllLinesAsync(file);
                string application = "global";
                List<string> modes = new();
                List<string> tags = new();
                string? operatingSystem = null;
                bool inCommandsSection = false;

                // First pass: check if there's a header section (look for delimiter)
                bool hasHeaderSection = lines.Any(line =>
                {
                    var trimmed = line.Trim();
                    var delimiterCheck = new string(trimmed.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    return delimiterCheck == "-";
                });

                // If no header section, start processing commands immediately
                // All commands will be treated as global since there's no application/mode specification
                if (!hasHeaderSection)
                {
                    inCommandsSection = true;
                    // application remains "global" (default value)
                    // modes remains empty (default value)
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    var rawLine = lines[i];
                    var line = rawLine.Trim();
                    Debug.WriteLine($"Line {i}: '{rawLine.Replace("\r", "\\r").Replace("\n", "\\n")}' (trimmed: '{line}')");
                    if (!inCommandsSection)
                    {
                        // Robust delimiter check: ignore all whitespace and carriage returns
                        var delimiterCheck = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());
                        if (delimiterCheck == "-")
                        {
                            Debug.WriteLine($"Delimiter found at line {i}");
                            inCommandsSection = true;
                            continue;
                        }
                        if (line.StartsWith("app:", StringComparison.OrdinalIgnoreCase))
                        {
                            application = line.Substring(4).Trim();
                        }
                        else if (line.StartsWith("application:", StringComparison.OrdinalIgnoreCase))
                        {
                            application = line.Substring(12).Trim();
                        }
                        else if (line.StartsWith("mode:", StringComparison.OrdinalIgnoreCase))
                        {
                            var modeValue = line.Substring(5).Trim();
                            if (!string.IsNullOrEmpty(modeValue))
                            {
                                modes.Add(modeValue);
                            }
                        }
                        else if (line.StartsWith("tag:", StringComparison.OrdinalIgnoreCase))
                        {
                            var tagValue = line.Substring(4).Trim();
                            if (!string.IsNullOrEmpty(tagValue))
                            {
                                tags.Add(tagValue);
                            }
                        }
                        else if (line.StartsWith("os:", StringComparison.OrdinalIgnoreCase))
                        {
                            operatingSystem = line.Substring(3).Trim();
                        }
                        // skip other headers
                        continue;
                    }
                    // After delimiter, skip blank lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;
                    // Multi-line script support
                    if (line.Contains(":"))
                    {
                        var split = line.Split(new[] { ':' }, 2);
                        var command = split[0].Trim();
                        var script = split[1].Trim();
                        // Skip settings() and similar blocks
                        if (command.EndsWith("()"))
                            continue;
                        // Check for indented lines (multi-line script)
                        int j = i + 1;
                        while (j < lines.Length && (lines[j].StartsWith("    ") || lines[j].StartsWith("\t")))
                        {
                            script += "\n" + lines[j].Trim();
                            j++;
                        }
                        i = j - 1; commands.Add(new TalonVoiceCommand
                        {
                            Command = command.Length > 200 ? command.Substring(0, 200) : command,
                            Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                            Application = application.Length > 200 ? application.Substring(0, 200) : application,
                            Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                            OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                            FilePath = file.Length > 500 ? file.Substring(file.Length - 500) : file,
                            Repository = ExtractRepositoryFromPath(file)?.Length > 200 ? ExtractRepositoryFromPath(file)?.Substring(0, 200) : ExtractRepositoryFromPath(file),
                            Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                            CreatedAt = File.GetCreationTimeUtc(file)
                        });
                    }
                }
            }
            await _context.TalonVoiceCommands.AddRangeAsync(commands);
            await _context.SaveChangesAsync();
            return commands.Count;
        }

        public async Task<List<TalonVoiceCommand>> SemanticSearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }
            var lowerTerm = searchTerm.ToLower();
            return await _context.TalonVoiceCommands
                .Where(c => c.Command.ToLower().Contains(lowerTerm) || c.Script.ToLower().Contains(lowerTerm) || c.Application.ToLower().Contains(lowerTerm) || (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)))
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<TalonVoiceCommand>> GetAllCommandsForFiltersAsync()
        {
            // Return ALL commands for building filter dropdowns
            return await _context.TalonVoiceCommands.ToListAsync();
        }
        public async Task<int> ImportTalonFileContentAsync(string fileContent, string fileName)
        {
            // Do NOT clear the table here; only add new commands
            var commands = new List<TalonVoiceCommand>();
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None); string application = "global";
            List<string> modes = new();
            List<string> tags = new();
            string? operatingSystem = null;
            bool inCommandsSection = false;

            // First pass: check if there's a header section (look for delimiter)
            bool hasHeaderSection = lines.Any(line =>
            {
                var trimmed = line.Trim();
                var delimiterCheck = new string(trimmed.Where(c => !char.IsWhiteSpace(c)).ToArray());
                return delimiterCheck == "-";
            });
            // If no header section, start processing commands immediately
            // All commands will be treated as global since there's no application/mode specification
            if (!hasHeaderSection)
            {
                inCommandsSection = true;
                // application remains "global" (default value)
                // modes remains empty (default value)
            }

            for (int i = 0; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var line = rawLine.Trim();
                if (!inCommandsSection)
                {
                    var delimiterCheck = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    if (delimiterCheck == "-")
                    {
                        inCommandsSection = true;
                        continue;
                    }
                    if (line.StartsWith("app:", StringComparison.OrdinalIgnoreCase))
                    {
                        application = line.Substring(4).Trim();
                    }
                    else if (line.StartsWith("application:", StringComparison.OrdinalIgnoreCase))
                    {
                        application = line.Substring(12).Trim();
                    }
                    else if (line.StartsWith("mode:", StringComparison.OrdinalIgnoreCase))
                    {
                        var modeValue = line.Substring(5).Trim();
                        if (!string.IsNullOrEmpty(modeValue))
                        {
                            modes.Add(modeValue);
                        }
                    }
                    else if (line.StartsWith("tag:", StringComparison.OrdinalIgnoreCase))
                    {
                        var tagValue = line.Substring(4).Trim();
                        if (!string.IsNullOrEmpty(tagValue))
                        {
                            tags.Add(tagValue);
                        }
                    }
                    else if (line.StartsWith("os:", StringComparison.OrdinalIgnoreCase))
                    {
                        operatingSystem = line.Substring(3).Trim();
                    }
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                if (line.Contains(":"))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    var command = split[0].Trim();
                    var script = split[1].Trim();
                    if (command.EndsWith("()"))
                        continue;
                    int j = i + 1;
                    while (j < lines.Length && (lines[j].StartsWith("    ") || lines[j].StartsWith("\t")))
                    {
                        script += "\n" + lines[j].Trim();
                        j++;
                    }
                    i = j - 1; commands.Add(new TalonVoiceCommand
                    {
                        Command = command.Length > 200 ? command.Substring(0, 200) : command,
                        Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                        Application = application.Length > 200 ? application.Substring(0, 200) : application,
                        Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                        OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                        FilePath = fileName.Length > 500 ? fileName.Substring(fileName.Length - 500) : fileName,
                        Repository = ExtractRepositoryFromPath(fileName)?.Length > 200 ? ExtractRepositoryFromPath(fileName)?.Substring(0, 200) : ExtractRepositoryFromPath(fileName),
                        Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.TalonVoiceCommands.AddRangeAsync(commands);
            await _context.SaveChangesAsync();
            return commands.Count;
        }

        public async Task<int> ImportAllTalonFilesFromDirectoryAsync(string rootFolder)
        {
            // Remove all existing records before importing new ones
            _context.TalonVoiceCommands.RemoveRange(_context.TalonVoiceCommands);
            await _context.SaveChangesAsync();
            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            int totalImported = 0;
            foreach (var file in talonFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                totalImported += await ImportTalonFileContentAsync(content, file);
            }
            return totalImported;
        }

        public async Task<int> ClearAllCommandsAsync()
        {
            // More robust clearing method
            var allCommands = await _context.TalonVoiceCommands.ToListAsync();
            _context.TalonVoiceCommands.RemoveRange(allCommands);
            await _context.SaveChangesAsync();
            return allCommands.Count;
        }

        public async Task<int> ImportAllTalonFilesWithProgressAsync(string rootFolder, Action<int, int, int>? progressCallback = null)
        {
            // Clear all existing records first
            await ClearAllCommandsAsync();

            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            int totalImported = 0;
            int filesProcessed = 0;

            foreach (var file in talonFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var commandsFromThisFile = await ImportTalonFileContentAsync(content, file);
                totalImported += commandsFromThisFile;
                filesProcessed++;
                // Report progress: (filesProcessed, totalFiles, totalCommandsSoFar)
                progressCallback?.Invoke(filesProcessed, talonFiles.Length, totalImported);
            }
            return totalImported;
        }

        /// <summary>
        /// Imports Talon lists from the TalonLists.txt file
        /// </summary>
        public async Task<int> ImportTalonListsFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"TalonLists.txt file not found at: {filePath}");

            var lines = await File.ReadAllLinesAsync(filePath);
            var talonLists = new List<TalonList>();

            string? currentListName = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines and header
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("All Talon Lists"))
                    continue;

                // Check for list definition: "List: user.list_name"
                if (trimmedLine.StartsWith("List: "))
                {
                    currentListName = trimmedLine.Substring(6); // Remove "List: " prefix
                    continue;
                }

                // Check for list item: "  - spoken_form: list_value"
                if (trimmedLine.StartsWith("- ") && currentListName != null)
                {
                    var itemContent = trimmedLine.Substring(2); // Remove "- " prefix
                    var colonIndex = itemContent.IndexOf(':');

                    if (colonIndex > 0)
                    {
                        var spokenForm = itemContent.Substring(0, colonIndex).Trim();
                        var listValue = itemContent.Substring(colonIndex + 1).Trim();

                        talonLists.Add(new TalonList
                        {
                            ListName = currentListName,
                            SpokenForm = spokenForm,
                            ListValue = listValue
                        });
                    }
                }
            }            // Before calling InsertTalonListsAsync, check for problems:
            var problems = FindOversizedTalonListValues(talonLists);

            if (problems.Any())
            {
                Console.WriteLine("Found oversized values:");
                foreach (var problem in problems)
                {
                    Console.WriteLine(problem);
                }

                // Also get max lengths to determine new column sizes
                var maxLengths = GetTalonListMaxLengths(talonLists);
                Console.WriteLine($"Max ListName length: {maxLengths["ListName"]}");
                Console.WriteLine($"Max SpokenForm length: {maxLengths["SpokenForm"]}");
                Console.WriteLine($"Max ListValue length: {maxLengths["ListValue"]}");
                Console.WriteLine($"Max SourceFile length: {maxLengths["SourceFile"]}");
                
                // Show detailed information about the oversized values
                Console.WriteLine("\n=== DETAILED OVERSIZED VALUE ANALYSIS ===");
                var details = GetOversizedTalonListDetails(talonLists);
                Console.WriteLine(details);
            }
            // Clear existing lists and add new ones
            _context.TalonLists.RemoveRange(_context.TalonLists);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine($"Error clearing TalonLists: {exception.Message}");
                throw;
            }

            _context.TalonLists.AddRange(talonLists);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine($"Error Creating TalonLists: {exception.Message}");
                throw;
            }

            return talonLists.Count;
        }        /// <summary>
                 /// Expands list references in a script, handling both {list_name} and function call patterns like key(arrow_key)
                 /// </summary>
        public async Task<string> ExpandListsInScriptAsync(string script)
        {
            if (string.IsNullOrEmpty(script))
                return script;

            var expandedScript = script;

            // Pattern 1: Handle {list_name} references
            if (script.Contains("{"))
            {
                var curlyBracePattern = @"\{([^}]+)\}";
                var curlyMatches = Regex.Matches(script, curlyBracePattern);

                foreach (Match match in curlyMatches)
                {
                    var listReference = match.Groups[1].Value; // e.g., "user.git_argument"
                    var expandedList = await GetExpandedListString(listReference);
                    expandedScript = expandedScript.Replace(match.Value, expandedList);
                }
            }

            // Pattern 2: Handle function call patterns like key(arrow_key), insert(text), etc.
            var functionPattern = @"(\w+)\(([a-zA-Z_][a-zA-Z0-9_]*)\)";
            var functionMatches = Regex.Matches(expandedScript, functionPattern);

            foreach (Match match in functionMatches)
            {
                var functionName = match.Groups[1].Value; // e.g., "key", "insert"
                var parameter = match.Groups[2].Value; // e.g., "arrow_key", "text"

                // Check if the parameter is a list reference
                var expandedList = await GetExpandedListString(parameter);

                // Only replace if we found a matching list (not a "list not found" message)
                if (!expandedList.Contains("list not found"))
                {
                    var replacement = $"{functionName}({expandedList})";
                    expandedScript = expandedScript.Replace(match.Value, replacement);
                }
            }

            return expandedScript;
        }

        /// <summary>
        /// Helper method to get expanded list string for a given list reference
        /// </summary>
        private async Task<string> GetExpandedListString(string listReference)
        {
            // Try to find the list with exact name match first
            var listValues = await _context.TalonLists
                .Where(l => l.ListName == listReference)
                .Select(l => l.SpokenForm)
                .ToListAsync();

            // If not found and the reference doesn't start with "user.", try adding "user." prefix
            if (!listValues.Any() && !listReference.StartsWith("user."))
            {
                listValues = await _context.TalonLists
                    .Where(l => l.ListName == $"user.{listReference}")
                    .Select(l => l.SpokenForm)
                    .ToListAsync();
            }

            if (listValues.Any())
            {
                // Show first few values with an indication if there are more
                var displayValues = listValues.Take(5).ToList();
                return displayValues.Count < listValues.Count
                    ? $"[{string.Join(" | ", displayValues)} | ... and {listValues.Count - displayValues.Count} more]"
                    : $"[{string.Join(" | ", displayValues)}]";
            }
            else
            {
                // If no list found, indicate this in the expansion
                return $"[{listReference} - list not found]";
            }
        }        /// <summary>        /// Enhanced search that includes list expansions and searches within list values
        /// </summary>
        public async Task<List<TalonVoiceCommand>> SemanticSearchWithListsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }

            var lowerTerm = searchTerm.ToLower();

            // First, get commands that match directly
            var directMatches = await _context.TalonVoiceCommands
                .Where(c => c.Command.ToLower().Contains(lowerTerm) ||
                           c.Script.ToLower().Contains(lowerTerm) ||
                           c.Application.ToLower().Contains(lowerTerm) ||
                           (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)))
                .ToListAsync();

            // Search within list values that might be referenced
            var listMatches = await _context.TalonLists
                .Where(l => l.SpokenForm.ToLower().Contains(lowerTerm) ||
                           l.ListValue.ToLower().Contains(lowerTerm))
                .Select(l => l.ListName)
                .Distinct()
                .ToListAsync();

            System.Console.WriteLine($"[DEBUG] Found {listMatches.Count} list matches for '{searchTerm}': {string.Join(", ", listMatches)}");

            // Find commands that reference these lists in various formats
            var listReferencingCommands = new List<TalonVoiceCommand>();
            foreach (var listName in listMatches)
            {
                var shortListName = listName.Replace("user.", "");
                System.Console.WriteLine($"[DEBUG] Searching for commands referencing list '{listName}' (short: '{shortListName}')");
                  // Search for specific Talon list reference patterns only:
                // 1. {list_name} format in commands - this is the main Talon syntax
                // 2. {short_name} format (without user. prefix)
                // Removed: broad text matching that was causing false positives with "model"
                var commandsWithListRefs = await _context.TalonVoiceCommands
                    .Where(c => c.Command.Contains($"{{{listName}}}") ||
                               c.Command.Contains($"{{{shortListName}}}") ||
                               c.Script.Contains($"{{{listName}}}") ||
                               c.Script.Contains($"{{{shortListName}}}"))
                    .ToListAsync();
                
                System.Console.WriteLine($"[DEBUG] Found {commandsWithListRefs.Count} commands with exact list references for '{listName}'");
                listReferencingCommands.AddRange(commandsWithListRefs);
            }            // Combine and deduplicate results, prioritizing list matches for better relevance
            var allMatches = new List<TalonVoiceCommand>();
            
            // Add list-referencing commands first (higher priority for list-based searches)
            allMatches.AddRange(listReferencingCommands);
            
            // Add direct matches that aren't already included
            foreach (var directMatch in directMatches)
            {
                if (!allMatches.Any(existing => existing.Id == directMatch.Id))
                {
                    allMatches.Add(directMatch);
                }
            }
            
            // Take top results and order by creation date
            var finalResults = allMatches
                .Take(100)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            System.Console.WriteLine($"[DEBUG] Final results: {directMatches.Count} direct matches + {listReferencingCommands.Count} list-referencing commands = {finalResults.Count} total");
            return finalResults;
        }

        // Helper class for deduplicating commands
        private class TalonVoiceCommandComparer : IEqualityComparer<TalonVoiceCommand>
        {
            public bool Equals(TalonVoiceCommand? x, TalonVoiceCommand? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }
            public int GetHashCode(TalonVoiceCommand obj)
            {
                return obj?.Id.GetHashCode() ?? 0;
            }
        }

        /// <summary>
        /// Diagnoses TalonList entries that exceed column size limits
        /// </summary>
        /// <param name="talonLists">The list of TalonList objects to check</param>
        /// <returns>A list of diagnostic messages for oversized values</returns>
        public List<string> DiagnoseTalonListColumnSizes(List<TalonList> talonLists)
        {
            var diagnostics = new List<string>();

            for (int i = 0; i < talonLists.Count; i++)
            {
                var item = talonLists[i];
                var index = i + 1; // 1-based index for user-friendly reporting

                // Check ListName (max 100 chars)
                if (item.ListName?.Length > 100)
                {
                    diagnostics.Add($"Entry #{index}: ListName exceeds 100 characters (actual: {item.ListName.Length}): '{item.ListName}'");
                }

                // Check SpokenForm (max 100 chars)
                if (item.SpokenForm?.Length > 100)
                {
                    diagnostics.Add($"Entry #{index}: SpokenForm exceeds 100 characters (actual: {item.SpokenForm.Length}): '{item.SpokenForm}'");
                }

                // Check ListValue (max 500 chars)
                if (item.ListValue?.Length > 500)
                {
                    diagnostics.Add($"Entry #{index}: ListValue exceeds 500 characters (actual: {item.ListValue.Length}): '{item.ListValue.Substring(0, Math.Min(100, item.ListValue.Length))}...'");
                }

                // Check SourceFile (max 250 chars)
                if (item.SourceFile?.Length > 250)
                {
                    diagnostics.Add($"Entry #{index}: SourceFile exceeds 250 characters (actual: {item.SourceFile.Length}): '{item.SourceFile}'");
                }
            }

            return diagnostics;
        }

        /// <summary>
        /// Gets detailed information about TalonList column violations with suggestions for new column sizes
        /// </summary>
        /// <param name="talonLists">The list of TalonList objects to analyze</param>
        /// <returns>A summary report with column size recommendations</returns>
        public string GetTalonListColumnSizeReport(List<TalonList> talonLists)
        {
            var report = new StringBuilder();
            var maxListName = 0;
            var maxSpokenForm = 0;
            var maxListValue = 0;
            var maxSourceFile = 0;

            var violationsCount = 0;

            foreach (var item in talonLists)
            {
                maxListName = Math.Max(maxListName, item.ListName?.Length ?? 0);
                maxSpokenForm = Math.Max(maxSpokenForm, item.SpokenForm?.Length ?? 0);
                maxListValue = Math.Max(maxListValue, item.ListValue?.Length ?? 0);
                maxSourceFile = Math.Max(maxSourceFile, item.SourceFile?.Length ?? 0);

                if ((item.ListName?.Length ?? 0) > 100 ||
                    (item.SpokenForm?.Length ?? 0) > 100 ||
                    (item.ListValue?.Length ?? 0) > 500 ||
                    (item.SourceFile?.Length ?? 0) > 250)
                {
                    violationsCount++;
                }
            }

            report.AppendLine("TalonList Column Size Analysis Report");
            report.AppendLine("=====================================");
            report.AppendLine($"Total entries analyzed: {talonLists.Count}");
            report.AppendLine($"Entries with violations: {violationsCount}");
            report.AppendLine();

            report.AppendLine("Current vs Actual Maximum Lengths:");
            report.AppendLine($"ListName: Current limit = 100, Actual max = {maxListName} {(maxListName > 100 ? "⚠️ EXCEEDS" : "✅")}");
            report.AppendLine($"SpokenForm: Current limit = 100, Actual max = {maxSpokenForm} {(maxSpokenForm > 100 ? "⚠️ EXCEEDS" : "✅")}");
            report.AppendLine($"ListValue: Current limit = 500, Actual max = {maxListValue} {(maxListValue > 500 ? "⚠️ EXCEEDS" : "✅")}");
            report.AppendLine($"SourceFile: Current limit = 250, Actual max = {maxSourceFile} {(maxSourceFile > 250 ? "⚠️ EXCEEDS" : "✅")}");
            report.AppendLine();

            if (violationsCount > 0)
            {
                report.AppendLine("Recommended Column Size Updates:");
                if (maxListName > 100)
                    report.AppendLine($"ALTER COLUMN ListName to nvarchar({Math.Max(maxListName + 50, 150)})");
                if (maxSpokenForm > 100)
                    report.AppendLine($"ALTER COLUMN SpokenForm to nvarchar({Math.Max(maxSpokenForm + 50, 150)})");
                if (maxListValue > 500)
                    report.AppendLine($"ALTER COLUMN ListValue to nvarchar({Math.Max(maxListValue + 100, 600)})");
                if (maxSourceFile > 250)
                    report.AppendLine($"ALTER COLUMN SourceFile to nvarchar({Math.Max(maxSourceFile + 50, 300)})");
            }
            else
            {
                report.AppendLine("✅ All entries fit within current column size limits.");
            }

            return report.ToString();
        }

        /// <summary>
        /// Exports Talon commands to a CSV file
        /// </summary>
        public async Task<string> ExportCommandsToCsvAsync(string filePath, List<TalonVoiceCommand> commands)
        {
            if (commands == null || commands.Count == 0)
                return "No commands to export.";

            var csv = new StringBuilder();
            // Header
            csv.AppendLine("Command,Script,Application,Mode,OperatingSystem,FilePath,Repository,Tags,CreatedAt");

            foreach (var command in commands)
            {
                var line = $"{EscapeCsvValue(command.Command)}," +
                           $"{EscapeCsvValue(command.Script)}," +
                           $"{EscapeCsvValue(command.Application)}," +
                           $"{EscapeCsvValue(command.Mode)}," +
                           $"{EscapeCsvValue(command.OperatingSystem)}," +
                           $"{EscapeCsvValue(command.FilePath)}," +
                           $"{EscapeCsvValue(command.Repository)}," +
                           $"{EscapeCsvValue(command.Tags)}," +
                           $"{command.CreatedAt:yyyy-MM-dd HH:mm:ss}";
                csv.AppendLine(line);
            }

            // Write to file
            await File.WriteAllTextAsync(filePath, csv.ToString());
            return filePath;
        }

        /// <summary>
        /// Escapes a value for CSV export
        /// </summary>
        private string EscapeCsvValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape double quotes by doubling them
            value = value.Replace("\"", "\"\"");

            // If the value contains commas, newlines, or double quotes, enclose it in double quotes
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

        /// <summary>
        /// Quick diagnostic method to find the specific TalonList values causing database insertion failures
        /// Call this method before calling InsertTalonListsAsync to identify problematic data
        /// </summary>
        /// <param name="talonLists">The TalonList data to check</param>
        /// <returns>List of specific problem descriptions</returns>
        public List<string> FindOversizedTalonListValues(List<TalonList> talonLists)
        {
            var problems = new List<string>();

            for (int i = 0; i < talonLists.Count; i++)
            {
                var item = talonLists[i];
                var itemIndex = i + 1; // 1-based for easier identification

                // Check each field against its database limit
                if (!string.IsNullOrEmpty(item.ListName) && item.ListName.Length > 100)
                {
                    problems.Add($"Item #{itemIndex}: ListName is {item.ListName.Length} characters (limit: 100) - Value: '{item.ListName}'");
                }

                if (!string.IsNullOrEmpty(item.SpokenForm) && item.SpokenForm.Length > 100)
                {
                    problems.Add($"Item #{itemIndex}: SpokenForm is {item.SpokenForm.Length} characters (limit: 100) - Value: '{item.SpokenForm}'");
                }

                if (!string.IsNullOrEmpty(item.ListValue) && item.ListValue.Length > 500)
                {
                    problems.Add($"Item #{itemIndex}: ListValue is {item.ListValue.Length} characters (limit: 500) - First 100 chars: '{item.ListValue.Substring(0, Math.Min(100, item.ListValue.Length))}...'");
                }

                if (!string.IsNullOrEmpty(item.SourceFile) && item.SourceFile.Length > 250)
                {
                    problems.Add($"Item #{itemIndex}: SourceFile is {item.SourceFile.Length} characters (limit: 250) - Value: '{item.SourceFile}'");
                }
            }

            return problems;
        }

        /// <summary>
        /// Gets the maximum length found for each TalonList column to help determine new column sizes
        /// </summary>
        /// <param name="talonLists">The TalonList data to analyze</param>
        /// <returns>Dictionary with column names and their maximum found lengths</returns>
        public Dictionary<string, int> GetTalonListMaxLengths(List<TalonList> talonLists)
        {
            var maxLengths = new Dictionary<string, int>
            {
                ["ListName"] = 0,
                ["SpokenForm"] = 0,
                ["ListValue"] = 0,
                ["SourceFile"] = 0
            };

            foreach (var item in talonLists)
            {
                maxLengths["ListName"] = Math.Max(maxLengths["ListName"], item.ListName?.Length ?? 0);
                maxLengths["SpokenForm"] = Math.Max(maxLengths["SpokenForm"], item.SpokenForm?.Length ?? 0);
                maxLengths["ListValue"] = Math.Max(maxLengths["ListValue"], item.ListValue?.Length ?? 0);
                maxLengths["SourceFile"] = Math.Max(maxLengths["SourceFile"], item.SourceFile?.Length ?? 0);
            }

            return maxLengths;
        }

        /// <summary>
        /// Imports Talon lists from the TalonLists.txt file (alternative method)
        /// This method attempts to auto-resolve column size issues by adjusting the database schema
        /// </summary>
        public async Task<int> ImportTalonListsFromFileWithAutoFixAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"TalonLists.txt file not found at: {filePath}");

            var lines = await File.ReadAllLinesAsync(filePath);
            var talonLists = new List<TalonList>();

            string? currentListName = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip empty lines and header
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("All Talon Lists"))
                    continue;

                // Check for list definition: "List: user.list_name"
                if (trimmedLine.StartsWith("List: "))
                {
                    currentListName = trimmedLine.Substring(6); // Remove "List: " prefix
                    continue;
                }

                // Check for list item: "  - spoken_form: list_value"
                if (trimmedLine.StartsWith("- ") && currentListName != null)
                {
                    var itemContent = trimmedLine.Substring(2); // Remove "- " prefix
                    var colonIndex = itemContent.IndexOf(':');

                    if (colonIndex > 0)
                    {
                        var spokenForm = itemContent.Substring(0, colonIndex).Trim();
                        var listValue = itemContent.Substring(colonIndex + 1).Trim();

                        talonLists.Add(new TalonList
                        {
                            ListName = currentListName,
                            SpokenForm = spokenForm,
                            ListValue = listValue
                        });
                    }
                }
            }

            // Auto-fix column sizes if needed
            var maxLengths = GetTalonListMaxLengths(talonLists);
            var alterStatements = new List<string>();

            if (maxLengths["ListName"] > 100)
                alterStatements.Add($"ALTER TABLE TalonLists ALTER COLUMN ListName nvarchar({Math.Max(maxLengths["ListName"] + 50, 150)})");
            if (maxLengths["SpokenForm"] > 100)
                alterStatements.Add($"ALTER TABLE TalonLists ALTER COLUMN SpokenForm nvarchar({Math.Max(maxLengths["SpokenForm"] + 50, 150)})");
            if (maxLengths["ListValue"] > 500)
                alterStatements.Add($"ALTER TABLE TalonLists ALTER COLUMN ListValue nvarchar({Math.Max(maxLengths["ListValue"] + 100, 600)})");
            if (maxLengths["SourceFile"] > 250)
                alterStatements.Add($"ALTER TABLE TalonLists ALTER COLUMN SourceFile nvarchar({Math.Max(maxLengths["SourceFile"] + 50, 300)})");

            // Apply schema changes if any
            if (alterStatements.Count > 0)
            {
                var sql = string.Join(";", alterStatements);
                await _context.Database.ExecuteSqlRawAsync(sql);
            }

            // Clear existing lists and add new ones
            _context.TalonLists.RemoveRange(_context.TalonLists);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine($"Error clearing TalonLists: {exception.Message}");
                throw;
            }

            _context.TalonLists.AddRange(talonLists);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine($"Error Creating TalonLists: {exception.Message}");
                throw;
            }

            return talonLists.Count;
        }

        /// <summary>
        /// Finds and displays the specific oversized TalonList values with full content for debugging
        /// </summary>
        /// <param name="talonLists">The TalonList data to analyze</param>
        /// <returns>Detailed information about oversized values</returns>
        public string GetOversizedTalonListDetails(List<TalonList> talonLists)
        {
            var details = new StringBuilder();
            
            for (int i = 0; i < talonLists.Count; i++)
            {
                var item = talonLists[i];
                var itemIndex = i + 1;
                
                // Check if this item has any oversized values
                bool hasOversizedValue = false;
                
                if (!string.IsNullOrEmpty(item.ListValue) && item.ListValue.Length > 500)
                {
                    hasOversizedValue = true;
                    details.AppendLine($"=== OVERSIZED ITEM #{itemIndex} ===");
                    details.AppendLine($"List Name: {item.ListName}");
                    details.AppendLine($"Spoken Form: {item.SpokenForm}");
                    details.AppendLine($"ListValue Length: {item.ListValue.Length} characters (limit: 500)");
                    details.AppendLine($"Source File: {item.SourceFile ?? "N/A"}");
                    details.AppendLine("FULL LIST VALUE:");
                    details.AppendLine("================");
                    details.AppendLine(item.ListValue);
                    details.AppendLine("================");
                    details.AppendLine();
                }
                
                // Check other columns too
                if (!string.IsNullOrEmpty(item.ListName) && item.ListName.Length > 100)
                {
                    if (!hasOversizedValue) details.AppendLine($"=== OVERSIZED ITEM #{itemIndex} ===");
                    details.AppendLine($"ListName is too long: {item.ListName.Length} chars (limit: 100)");
                    details.AppendLine($"ListName: {item.ListName}");
                    details.AppendLine();
                }
                
                if (!string.IsNullOrEmpty(item.SpokenForm) && item.SpokenForm.Length > 100)
                {
                    if (!hasOversizedValue) details.AppendLine($"=== OVERSIZED ITEM #{itemIndex} ===");
                    details.AppendLine($"SpokenForm is too long: {item.SpokenForm.Length} chars (limit: 100)");
                    details.AppendLine($"SpokenForm: {item.SpokenForm}");
                    details.AppendLine();
                }
                
                if (!string.IsNullOrEmpty(item.SourceFile) && item.SourceFile.Length > 250)
                {
                    if (!hasOversizedValue) details.AppendLine($"=== OVERSIZED ITEM #{itemIndex} ===");
                    details.AppendLine($"SourceFile is too long: {item.SourceFile.Length} chars (limit: 250)");
                    details.AppendLine($"SourceFile: {item.SourceFile}");
                    details.AppendLine();
                }
            }
              if (details.Length == 0)
            {
                details.AppendLine("No oversized values found!");
            }
            
            return details.ToString();        }        /// <summary>
        /// Gets all contents (spoken forms and list values) for a specific list
        /// </summary>
        public async Task<List<TalonList>> GetListContentsAsync(string listName)
        {
            if (string.IsNullOrWhiteSpace(listName))
                return new List<TalonList>();

            Console.WriteLine($"[DEBUG] Loading list contents for: '{listName}'");

            // Try multiple variations of the list name
            var searchNames = new List<string> { listName };
            
            // If listName doesn't start with "user.", try adding it
            if (!listName.StartsWith("user."))
            {
                searchNames.Add($"user.{listName}");
            }
            
            // If listName starts with "user.", try without it
            if (listName.StartsWith("user."))
            {
                searchNames.Add(listName.Substring(5)); // Remove "user." prefix
            }

            // Try some common variations for modelPrompt
            if (listName.Contains("modelPrompt") || listName.Contains("model"))
            {
                searchNames.Add("user.model");
                searchNames.Add("model");
                searchNames.Add("user.modelPrompt");
                searchNames.Add("modelPrompt");
            }

            Console.WriteLine($"[DEBUG] Searching for list names: {string.Join(", ", searchNames)}");

            // First, let's see what lists actually exist in the database
            var allLists = await _context.TalonLists
                .Select(l => l.ListName)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG] All available lists in database: {string.Join(", ", allLists)}");

            var results = await _context.TalonLists
                .Where(l => searchNames.Contains(l.ListName))
                .OrderBy(l => l.SpokenForm)
                .ThenBy(l => l.ListValue)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {results.Count} list items for any of: {string.Join(", ", searchNames)}");
            
            // Debug: Show what we found
            if (results.Any())
            {
                Console.WriteLine($"[DEBUG] Sample items: {string.Join(", ", results.Take(3).Select(r => $"{r.SpokenForm}→{r.ListValue}"))}");
            }
            else
            {
                // If no results, try a broader search to see if the list exists with any variation
                var partialMatches = await _context.TalonLists
                    .Where(l => searchNames.Any(name => l.ListName.Contains(name) || name.Contains(l.ListName)))
                    .Select(l => l.ListName)
                    .Distinct()
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] Partial matches found: {string.Join(", ", partialMatches)}");
            }
            
            return results;
        }
    }
}
