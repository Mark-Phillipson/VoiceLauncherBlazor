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

        public async Task<int> ImportFromTalonFilesAsync(string rootFolder)
        {
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
                            Command = command.Length > 100 ? command.Substring(0, 100) : command,
                            Script = script.Length > 1000 ? script.Substring(0, 1000) : script,
                            Application = application.Length > 100 ? application.Substring(0, 100) : application,
                            Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)) : null,
                            OperatingSystem = operatingSystem != null && operatingSystem.Length > 50 ? operatingSystem.Substring(0, 50) : operatingSystem,
                            FilePath = file.Length > 250 ? file.Substring(file.Length - 250) : file,
                            Repository = ExtractRepositoryFromPath(file),
                            Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)) : null,
                            CreatedAt = File.GetCreationTimeUtc(file)                        });
                        
                        // Debug logging - remove after testing
                        Debug.WriteLine($"File: {file}");
                        Debug.WriteLine($"Extracted Repository: {ExtractRepositoryFromPath(file)}");
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
        }        public async Task<int> ImportTalonFileContentAsync(string fileContent, string fileName)
        {
            // Do NOT clear the table here; only add new commands
            var commands = new List<TalonVoiceCommand>();
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);            string application = "global";
            List<string> modes = new();
            List<string> tags = new();
            string? operatingSystem = null;
            bool inCommandsSection = false;
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
                        Command = command.Length > 100 ? command.Substring(0, 100) : command,
                        Script = script.Length > 1000 ? script.Substring(0, 1000) : script,
                        Application = application.Length > 100 ? application.Substring(0, 100) : application,
                        Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)) : null,
                        OperatingSystem = operatingSystem != null && operatingSystem.Length > 50 ? operatingSystem.Substring(0, 50) : operatingSystem,
                        FilePath = fileName,
                        Repository = ExtractRepositoryFromPath(fileName),
                        Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)) : null,
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
    }
}
