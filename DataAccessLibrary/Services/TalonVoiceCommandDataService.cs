using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary.Services
{    public class TalonVoiceCommandDataService
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

        public async Task<int> ImportFromTalonFilesAsync(string rootFolder)        {
            // Remove all existing records before importing new ones
            _context.TalonVoiceCommands.RemoveRange(_context.TalonVoiceCommands);
            await _context.SaveChangesAsync();
            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            var commands = new List<TalonVoiceCommand>();            foreach (var file in talonFiles)
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
                        }                        else if (line.StartsWith("mode:", StringComparison.OrdinalIgnoreCase))
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
                        i = j - 1;                        commands.Add(new TalonVoiceCommand
                        {
                            Command = command.Length > 200 ? command.Substring(0, 200) : command,
                            Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                            Application = application.Length > 200 ? application.Substring(0, 200) : application,
                            Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                            OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                            FilePath = file.Length > 500 ? file.Substring(file.Length - 500) : file,
                            Repository = ExtractRepositoryFromPath(file)?.Length > 200 ? ExtractRepositoryFromPath(file)?.Substring(0, 200) : ExtractRepositoryFromPath(file),
                            Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                            CreatedAt = File.GetCreationTimeUtc(file)                        });
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
        }        public async Task<int> ImportTalonFileContentAsync(string fileContent, string fileName)        {
            // Do NOT clear the table here; only add new commands
            var commands = new List<TalonVoiceCommand>();
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);            string application = "global";
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
                        application = line.Substring(12).Trim();                    }                    else if (line.StartsWith("mode:", StringComparison.OrdinalIgnoreCase))
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
                    i = j - 1;                    commands.Add(new TalonVoiceCommand
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
                }            }
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
            }
            
            // Clear existing lists and add new ones
            _context.TalonLists.RemoveRange(_context.TalonLists);
            await _context.SaveChangesAsync();
            
            _context.TalonLists.AddRange(talonLists);
            await _context.SaveChangesAsync();
            
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
        }

        /// <summary>
        /// Enhanced search that includes list expansions and searches within list values
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

            // Also search within list values that might be referenced
            var listMatches = await _context.TalonLists
                .Where(l => l.SpokenForm.ToLower().Contains(lowerTerm) || 
                           l.ListValue.ToLower().Contains(lowerTerm))
                .Select(l => l.ListName)
                .Distinct()
                .ToListAsync();

            // Find commands that reference these lists
            var listReferencingCommands = new List<TalonVoiceCommand>();
            foreach (var listName in listMatches)
            {
                var shortListName = listName.Replace("user.", "");
                var commandsWithListRefs = await _context.TalonVoiceCommands
                    .Where(c => c.Script.Contains($"{{{listName}}}") || 
                               c.Script.Contains($"{{{shortListName}}}"))
                    .ToListAsync();
                listReferencingCommands.AddRange(commandsWithListRefs);
            }

            // Combine and deduplicate results
            var allMatches = directMatches.Union(listReferencingCommands, new TalonVoiceCommandComparer())
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .ToList();

            return allMatches;
        }

        // Helper class for deduplicating commands
        private class TalonVoiceCommandComparer : IEqualityComparer<TalonVoiceCommand>
        {
            public bool Equals(TalonVoiceCommand? x, TalonVoiceCommand? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }            public int GetHashCode(TalonVoiceCommand obj)
            {
                return obj?.Id.GetHashCode() ?? 0;            }
        }
    }
}
