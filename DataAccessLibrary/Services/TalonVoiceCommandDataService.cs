using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using SmartComponents.LocalEmbeddings;

namespace DataAccessLibrary.Services
{
    public class TalonVoiceCommandDataService : ITalonVoiceCommandDataService
    {
        private readonly ApplicationDbContext _context;
        private static readonly LocalEmbedder _embedder = new LocalEmbedder();
        
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
            {                var lines = await File.ReadAllLinesAsync(file);                string application = "global";
                List<string> modes = new();
                List<string> tags = new();
                List<string> codeLanguages = new();
                List<string> languages = new();
                List<string> hostnames = new();
                string? operatingSystem = null;
                string? title = null;
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
                        var delimiterCheck = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());                        if (delimiterCheck == "-")
                        {
                            Debug.WriteLine($"Delimiter found at line {i}");
                            inCommandsSection = true;
                            continue;
                        }
                        
                        // Try to parse application using the helper method
                        var parsedApp = ParseApplicationFromHeaderLine(line);
                        if (parsedApp != null)
                        {
                            application = parsedApp;
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
                            }                        }
                        else if (line.StartsWith("os:", StringComparison.OrdinalIgnoreCase))
                        {
                            operatingSystem = line.Substring(3).Trim();
                        }
                        else if (line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                        {
                            title = line.Substring(6).Trim();
                        }
                        else if (line.StartsWith("code.language:", StringComparison.OrdinalIgnoreCase))
                        {
                            var codeLanguageValue = line.Substring(14).Trim();
                            if (!string.IsNullOrEmpty(codeLanguageValue))
                            {
                                codeLanguages.Add(codeLanguageValue);
                            }
                        }
                        else if (line.StartsWith("language:", StringComparison.OrdinalIgnoreCase))
                        {
                            var languageValue = line.Substring(9).Trim();
                            if (!string.IsNullOrEmpty(languageValue))
                            {
                                languages.Add(languageValue);
                            }
                        }
                        else if (line.StartsWith("hostname:", StringComparison.OrdinalIgnoreCase))
                        {
                            var hostnameValue = line.Substring(9).Trim();
                            if (!string.IsNullOrEmpty(hostnameValue))
                            {
                                hostnames.Add(hostnameValue);
                            }
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
                        i = j - 1;                        commands.Add(new TalonVoiceCommand
                        {
                            Command = command.Length > 200 ? command.Substring(0, 200) : command,
                            Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                            Application = application.Length > 200 ? application.Substring(0, 200) : application,
                            Title = title != null && title.Length > 200 ? title.Substring(0, 200) : title,
                            Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                            OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                            FilePath = file.Length > 500 ? file.Substring(file.Length - 500) : file,
                            Repository = ExtractRepositoryFromPath(file)?.Length > 200 ? ExtractRepositoryFromPath(file)?.Substring(0, 200) : ExtractRepositoryFromPath(file),
                            Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                            CodeLanguage = codeLanguages.Count > 0 ? string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Substring(0, Math.Min(300, string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Length)) : null,
                            Language = languages.Count > 0 ? string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Substring(0, Math.Min(300, string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Length)) : null,
                            Hostname = hostnames.Count > 0 ? string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Substring(0, Math.Min(300, string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Length)) : null,
                            CreatedAt = File.GetCreationTimeUtc(file)
                        });
                    }
                }
            }
            await _context.TalonVoiceCommands.AddRangeAsync(commands);
            await _context.SaveChangesAsync();
            return commands.Count;
        }        public async Task<List<TalonVoiceCommand>> SemanticSearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }

            try
            {
                // Get all commands from database in a single call
                var allCommands = await _context.TalonVoiceCommands.ToListAsync();
                
                if (allCommands.Count == 0)
                {
                    return new List<TalonVoiceCommand>();
                }

                // If any exact substring matches exist in Title, Command, or Script, prefer those literal matches
                var literalMatches = allCommands.Where(c =>
                    (!string.IsNullOrEmpty(c.Title) && c.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(c.Command) && c.Command.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(c.Script) && c.Script.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).OrderByDescending(c => c.CreatedAt).ToList();

                if (literalMatches.Any())
                {
                    Console.WriteLine($"[DEBUG] Semantic search for '{searchTerm}': using literal substring matches: {literalMatches.Count} results");
                    return literalMatches;
                }

                // Create embeddings for search term
                var searchEmbedding = _embedder.Embed(searchTerm);
                
                // Calculate similarity scores and get top results
                var commandsWithScores = allCommands.Select(cmd =>
                {
                    // Create a combined text for semantic comparison
                    var combinedText = $"{cmd.Command} {cmd.Script} {cmd.Title ?? ""} {cmd.Application}".Trim();
                    
                    // Get embedding for this command
                    var commandEmbedding = _embedder.Embed(combinedText);
                    
                    // Calculate cosine similarity
                    var similarity = LocalEmbedder.Similarity(searchEmbedding, commandEmbedding);
                    
                    return new { Command = cmd, Similarity = similarity };
                })
                .Where(x => x.Similarity > 0.3f) // Filter by minimum similarity threshold
                .OrderByDescending(x => x.Similarity)
                .Take(100)
                .Select(x => x.Command)
                .ToList();

                Console.WriteLine($"[DEBUG] Semantic search for '{searchTerm}': found {commandsWithScores.Count} results");
                return commandsWithScores;
            }
            catch (Exception ex)
            {
                // Fallback to basic text search if semantic search fails
                Console.WriteLine($"[DEBUG] Semantic search failed, falling back to text search: {ex.Message}");
                var lowerTerm = searchTerm.ToLower();
                return await _context.TalonVoiceCommands
                    .Where(c => c.Command.ToLower().Contains(lowerTerm) || 
                               c.Script.ToLower().Contains(lowerTerm) || 
                               c.Application.ToLower().Contains(lowerTerm) || 
                               (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)) || 
                               (c.Title != null && c.Title.ToLower().Contains(lowerTerm)))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(100)
                    .ToListAsync();
            }
        }

        public async Task<List<TalonVoiceCommand>> GetAllCommandsForFiltersAsync()
        {
            // Return ALL commands for building filter dropdowns
            return await _context.TalonVoiceCommands.ToListAsync();
        }
        public async Task<int> ImportTalonFileContentAsync(string fileContent, string fileName)
        {
            // Do NOT clear the table here; only add new commands
            var commands = new List<TalonVoiceCommand>();            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None); string application = "global";
            List<string> modes = new();
            List<string> tags = new();
            List<string> codeLanguages = new();
            List<string> languages = new();
            List<string> hostnames = new();
            string? operatingSystem = null;
            string? title = null;
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
                {                    var delimiterCheck = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    if (delimiterCheck == "-")
                    {
                        inCommandsSection = true;
                        continue;
                    }
                    
                    // Try to parse application using the helper method
                    var parsedApp = ParseApplicationFromHeaderLine(line);
                    if (parsedApp != null)
                    {
                        application = parsedApp;
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
                    }                    else if (line.StartsWith("os:", StringComparison.OrdinalIgnoreCase))
                    {
                        operatingSystem = line.Substring(3).Trim();
                    }                    else if (line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                    {
                        title = line.Substring(6).Trim();
                    }
                    else if (line.StartsWith("code.language:", StringComparison.OrdinalIgnoreCase))
                    {
                        var codeLanguageValue = line.Substring(14).Trim();
                        if (!string.IsNullOrEmpty(codeLanguageValue))
                        {
                            codeLanguages.Add(codeLanguageValue);
                        }
                    }
                    else if (line.StartsWith("language:", StringComparison.OrdinalIgnoreCase))
                    {
                        var languageValue = line.Substring(9).Trim();
                        if (!string.IsNullOrEmpty(languageValue))
                        {
                            languages.Add(languageValue);
                        }
                    }
                    else if (line.StartsWith("hostname:", StringComparison.OrdinalIgnoreCase))
                    {
                        var hostnameValue = line.Substring(9).Trim();
                        if (!string.IsNullOrEmpty(hostnameValue))
                        {
                            hostnames.Add(hostnameValue);
                        }
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
                    i = j - 1;                    commands.Add(new TalonVoiceCommand
                    {
                        Command = command.Length > 200 ? command.Substring(0, 200) : command,
                        Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                        Application = application.Length > 200 ? application.Substring(0, 200) : application,
                        Title = title != null && title.Length > 200 ? title.Substring(0, 200) : title,
                        Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                        OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                        FilePath = fileName.Length > 500 ? fileName.Substring(fileName.Length - 500) : fileName,
                        Repository = ExtractRepositoryFromPath(fileName)?.Length > 200 ? ExtractRepositoryFromPath(fileName)?.Substring(0, 200) : ExtractRepositoryFromPath(fileName),
                        Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                        CodeLanguage = codeLanguages.Count > 0 ? string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Substring(0, Math.Min(300, string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Length)) : null,
                        Language = languages.Count > 0 ? string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Substring(0, Math.Min(300, string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Length)) : null,
                        Hostname = hostnames.Count > 0 ? string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Substring(0, Math.Min(300, string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Length)) : null,
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

            try
            {
                // Get all required data in single database calls to avoid DbContext concurrency issues
                var allCommands = await _context.TalonVoiceCommands.ToListAsync();
                var allLists = await _context.TalonLists.ToListAsync();
                
                if (allCommands.Count == 0)
                {
                    return new List<TalonVoiceCommand>();
                }

                // Perform semantic search on the in-memory data
                var searchEmbedding = _embedder.Embed(searchTerm);
                var semanticMatches = allCommands.Select(cmd =>
                {
                    // Create a combined text for semantic comparison
                    var combinedText = $"{cmd.Command} {cmd.Script} {cmd.Title ?? ""} {cmd.Application}".Trim();
                    
                    // Get embedding for this command
                    var commandEmbedding = _embedder.Embed(combinedText);
                    
                    // Calculate cosine similarity
                    var similarity = LocalEmbedder.Similarity(searchEmbedding, commandEmbedding);
                    
                    return new { Command = cmd, Similarity = similarity };
                })
                .Where(x => x.Similarity > 0.3f) // Filter by minimum similarity threshold
                .OrderByDescending(x => x.Similarity)
                .Select(x => x.Command)
                .ToList();

                // Find matching lists from in-memory data
                var listMatches = allLists
                    .Where(l => l.SpokenForm.ToLower().Contains(lowerTerm) ||
                               l.ListValue.ToLower().Contains(lowerTerm))
                    .Select(l => l.ListName)
                    .Distinct()
                    .ToList();

                System.Console.WriteLine($"[DEBUG] Found {listMatches.Count} list matches for '{searchTerm}': {string.Join(", ", listMatches)}");

                // Find commands that reference these lists from in-memory data
                var listReferencingCommands = new List<TalonVoiceCommand>();
                foreach (var listName in listMatches)
                {
                    var shortListName = listName.Replace("user.", "");
                    
                    var commandsWithListRefs = allCommands
                        .Where(c =>
                                   // {list} references
                                   c.Command.Contains($"{{{listName}}}") ||
                                   c.Command.Contains($"{{{shortListName}}}") ||
                                   c.Script.Contains($"{{{listName}}}") ||
                                   c.Script.Contains($"{{{shortListName}}}") ||
                                   // <capture> references - e.g. <user.arrow_key> or <arrow_key>
                                   c.Command.Contains($"<{listName}>") ||
                                   c.Command.Contains($"<{shortListName}>") ||
                                   c.Script.Contains($"<{listName}>") ||
                                   c.Script.Contains($"<{shortListName}>")
                                   )
                        .ToList();
                    
                    listReferencingCommands.AddRange(commandsWithListRefs);
                }

                // Combine semantic matches and list-referencing commands, removing duplicates
                var allMatches = new List<TalonVoiceCommand>();
                allMatches.AddRange(semanticMatches);
                
                foreach (var listCommand in listReferencingCommands)
                {
                    if (!allMatches.Any(existing => existing.Id == listCommand.Id))
                    {
                        allMatches.Add(listCommand);
                    }
                }

                var finalResults = allMatches
                    .Take(100)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                System.Console.WriteLine($"[DEBUG] Final results: {semanticMatches.Count} semantic matches + {listReferencingCommands.Count} list-referencing commands = {finalResults.Count} total");
                return finalResults;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DEBUG] SemanticSearchWithListsAsync failed, falling back to basic search: {ex.Message}");
                // Fallback to the original text-based implementation
                var directMatches = await _context.TalonVoiceCommands
                    .Where(c => c.Command.ToLower().Contains(lowerTerm) ||
                               c.Script.ToLower().Contains(lowerTerm) ||
                               c.Application.ToLower().Contains(lowerTerm) ||
                               (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)) ||
                               (c.Title != null && c.Title.ToLower().Contains(lowerTerm)))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(100)
                    .ToListAsync();
                
                return directMatches;
            }
        }

        /// <summary>
        /// Searches only command names
        /// </summary>
        public async Task<List<TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }
            // Tokenize search term into words and match as whole words.
            var tokens = Regex.Split(searchTerm.Trim(), "\\s+")
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            if (!tokens.Any())
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }

            // Load commands and lists into memory to allow combined token matching across command text and list spoken forms
            var allCommands = await _context.TalonVoiceCommands.ToListAsync();
            var allLists = await _context.TalonLists.ToListAsync();

            Console.WriteLine($"[DEBUG] SearchCommandNamesOnlyAsync - tokenized '{searchTerm}' -> [{string.Join(", ", tokens)}]");

            bool MatchesWholeWord(string sourceLower, string token)
            {
                if (string.IsNullOrEmpty(sourceLower)) return false;
                // Use simple word boundary matching
                return Regex.IsMatch(sourceLower, $"\\b{Regex.Escape(token)}\\b", RegexOptions.CultureInvariant);
            }

            // Prebuild map of listName -> list of spoken forms (lowercase)
            var listSpokenMap = allLists
                .GroupBy(l => l.ListName)
                .ToDictionary(g => g.Key, g => g.Select(x => (x.SpokenForm ?? "").ToLower()).ToList());

            // Also build maps for short names (without user. prefix)
            var shortToFullListNames = new Dictionary<string, List<string>>(); // short -> full names
            foreach (var fullName in listSpokenMap.Keys)
            {
                var shortName = fullName.StartsWith("user.") ? fullName.Substring("user.".Length) : fullName;
                if (!shortToFullListNames.TryGetValue(shortName, out var list))
                {
                    list = new List<string>();
                    shortToFullListNames[shortName] = list;
                }
                list.Add(fullName);
            }

            // Identify tokens that look like list names (either full or short). For such tokens we will require
            // that candidate commands reference the corresponding list(s).
            var listNameTokens = new Dictionary<string, List<string>>(); // token -> matching full list names
            foreach (var token in tokens)
            {
                var tokenAsFull = token;
                var tokenAsShort = token;
                var matches = new List<string>();
                // direct full-name match
                if (listSpokenMap.ContainsKey(tokenAsFull)) matches.Add(tokenAsFull);
                // token might equal short name; map to full names
                if (shortToFullListNames.TryGetValue(tokenAsShort, out var fulls)) matches.AddRange(fulls);

                if (matches.Any())
                {
                    listNameTokens[token] = matches.Distinct().ToList();
                }
            }

            // Helper to get referenced list names from a command string (both {name} and <name> patterns)
            List<string> GetReferencedListNames(string cmd)
            {
                var referenced = new List<string>();
                if (string.IsNullOrEmpty(cmd)) return referenced;

                // {list} patterns
                var curly = Regex.Matches(cmd, "\\{([^}]+)\\}");
                foreach (Match m in curly)
                {
                    var name = m.Groups[1].Value;
                    referenced.Add(name);
                    if (!name.StartsWith("user.")) referenced.Add("user." + name);
                }

                // <capture> patterns (treat capture names as potential list names)
                var angle = Regex.Matches(cmd, "<([^>]+)>");
                foreach (Match m in angle)
                {
                    var name = m.Groups[1].Value;
                    referenced.Add(name);
                    if (!name.StartsWith("user.")) referenced.Add("user." + name);
                }

                return referenced.Distinct().ToList();
            }

            var results = new List<TalonVoiceCommand>();

            foreach (var cmd in allCommands)
            {
                var cmdLower = (cmd.Command ?? string.Empty).ToLower();

                // Get referenced lists for this command
                var referencedLists = GetReferencedListNames(cmd.Command ?? string.Empty);

                // For each token, check if token is satisfied by either command text or any referenced list's spoken forms
                bool allTokensMatch = true;
                foreach (var token in tokens)
                {
                    bool tokenMatched = false;

                    // Check command text
                    if (MatchesWholeWord(cmdLower, token)) tokenMatched = true;

                    // Check referenced lists' spoken forms
                    if (!tokenMatched && referencedLists.Any())
                    {
                        foreach (var listName in referencedLists)
                        {
                            if (listSpokenMap.TryGetValue(listName, out var spokenForms))
                            {
                                if (spokenForms.Any(sf => MatchesWholeWord(sf, token)))
                                {
                                    tokenMatched = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!tokenMatched)
                    {
                        allTokensMatch = false;
                        break;
                    }
                }

                // If there were list-name tokens in the query, ensure this command references at least one of the
                // matching lists for each such token. This prevents returning commands that don't reference the
                // intended list even if they match other tokens.
                if (allTokensMatch && listNameTokens.Any())
                {
                    bool listTokensSatisfied = true;
                    foreach (var kv in listNameTokens)
                    {
                        var token = kv.Key;
                        var matchingFullNames = kv.Value; // full list names that the token could refer to

                        // Command must reference at least one of matchingFullNames
                        if (!referencedLists.Any(r => matchingFullNames.Contains(r)))
                        {
                            listTokensSatisfied = false;
                            break;
                        }
                    }

                    if (!listTokensSatisfied)
                    {
                        allTokensMatch = false;
                    }
                }

                if (allTokensMatch)
                {
                    results.Add(cmd);
                }
            }

            // As a fallback include any direct command substring matches (existing behavior), but keep uniqueness
            var lowerTerm = searchTerm.ToLower();
            var fallback = allCommands.Where(c => (c.Command ?? string.Empty).ToLower().Contains(lowerTerm)).ToList();
            foreach (var f in fallback)
            {
                if (!results.Any(r => r.Id == f.Id)) results.Add(f);
            }

            Console.WriteLine($"[DEBUG] Command names only search for '{searchTerm}': {results.Count} results (word-matching + fallback)");

            var final = results
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .ToList();

            return final;
        }

        /// <summary>
        /// Searches only script content
        /// </summary>
        public async Task<List<TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.TalonVoiceCommands.OrderByDescending(c => c.CreatedAt).Take(100).ToListAsync();
            }

            var lowerTerm = searchTerm.ToLower();
            
            // Only search within script content
            var scriptMatches = await _context.TalonVoiceCommands
                .Where(c => c.Script.ToLower().Contains(lowerTerm))
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .ToListAsync();

            System.Console.WriteLine($"[DEBUG] Script only search for '{searchTerm}': {scriptMatches.Count} results");
            return scriptMatches;
        }

        /// <summary>
        /// Searches all fields (commands, scripts, applications, modes, titles) with list support
        /// </summary>
        public async Task<List<TalonVoiceCommand>> SearchAllAsync(string searchTerm)
        {
            // Use the existing comprehensive search method
            return await SemanticSearchWithListsAsync(searchTerm);
        }

        // ...existing code...

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
        /// <summary>
        /// Gets the contents of a specific Talon list by exact name match
        /// </summary>
        public async Task<List<TalonList>> GetListContentsAsync(string listName)
        {
            if (string.IsNullOrWhiteSpace(listName))
                return new List<TalonList>();

            Console.WriteLine($"[DEBUG] Loading list contents for: '{listName}'");

            // Start with exact match search
            var results = await _context.TalonLists
                .Where(l => l.ListName == listName)
                .OrderBy(l => l.SpokenForm)
                .ThenBy(l => l.ListValue)
                .ToListAsync();

            // If no exact match found, try with/without "user." prefix as fallback
            if (results.Count == 0)
            {
                string alternateListName;
                if (listName.StartsWith("user."))
                {
                    // Try without "user." prefix
                    alternateListName = listName.Substring(5);
                }
                else
                {
                    // Try with "user." prefix
                    alternateListName = $"user.{listName}";
                }

                Console.WriteLine($"[DEBUG] No exact match for '{listName}', trying alternate: '{alternateListName}'");

                results = await _context.TalonLists
                    .Where(l => l.ListName == alternateListName)
                    .OrderBy(l => l.SpokenForm)
                    .ThenBy(l => l.ListValue)
                    .ToListAsync();
            }

            Console.WriteLine($"[DEBUG] Found {results.Count} list items for '{listName}'");
            
            // Debug: Show what we found
            if (results.Any())
            {
                Console.WriteLine($"[DEBUG] Sample items: {string.Join(", ", results.Take(3).Select(r => $"{r.SpokenForm}→{r.ListValue}"))}");
                Console.WriteLine($"[DEBUG] Actual list name in results: '{results.First().ListName}'");
            }
            else
            {
                // For debugging: show what list names actually exist that are similar
                var similarLists = await _context.TalonLists
                    .Where(l => l.ListName.Contains(listName) || listName.Contains(l.ListName))
                    .Select(l => l.ListName)
                    .Distinct()
                    .Take(5)
                    .ToListAsync();
                
                Console.WriteLine($"[DEBUG] No results found. Similar list names: {string.Join(", ", similarLists)}");
            }
            
            return results;
        }        /// <summary>
        /// Parses a header line to extract application name, handling complex patterns like "and app.name:"
        /// </summary>
        private string? ParseApplicationFromHeaderLine(string line)
        {
            // Handle direct patterns
            if (line.StartsWith("app:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(4).Trim();
            }
            if (line.StartsWith("application:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(12).Trim();
            }
            
            // Handle "app.exe:" pattern - common in Talon files
            if (line.StartsWith("app.exe:", StringComparison.OrdinalIgnoreCase))
            {
                return line.Substring(8).Trim();
            }
            
            // Handle "and app.name:" pattern - extract the application name after the colon
            if (line.Contains("app.name:", StringComparison.OrdinalIgnoreCase))
            {
                var appNameIndex = line.IndexOf("app.name:", StringComparison.OrdinalIgnoreCase);
                if (appNameIndex >= 0)
                {
                    var colonIndex = line.IndexOf(':', appNameIndex + "app.name".Length);
                    if (colonIndex >= 0 && colonIndex + 1 < line.Length)
                    {
                        return line.Substring(colonIndex + 1).Trim();
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Get breakdown of Talon commands by repository
        /// </summary>
        public async Task<List<CommandsBreakdown>> GetTalonCommandsBreakdownAsync()
        {
            var commands = await _context.TalonVoiceCommands.ToListAsync();
            
            var breakdown = commands
                .GroupBy(c => c.Repository ?? "Unknown")
                .Select(g => new CommandsBreakdown
                {
                    ApplicationName = g.Key, // Using ApplicationName field for repository name
                    AutoCreated = false, // Talon commands are not auto-created
                    Number = g.Count()
                })
                .OrderByDescending(b => b.Number)
                .ToList();
                
            return breakdown;
        }
    }
}
