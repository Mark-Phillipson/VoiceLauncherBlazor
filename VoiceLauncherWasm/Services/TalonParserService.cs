using VoiceLauncherWasm.Models;
using System.Text.RegularExpressions;

namespace VoiceLauncherWasm.Services
{
    public class TalonParserService
    {
    public List<TalonVoiceCommand> ParseTalonFile(string content, string filePath, string? repository)
        {
            var commands = new List<TalonVoiceCommand>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            string? currentApplication = null;
            bool inCommandBlock = false;
            string? currentVoiceCommand = null;
            var scriptLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip comments and empty lines
                if (trimmedLine.StartsWith("#") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Check for application context
                if (trimmedLine.StartsWith("app:") || trimmedLine.StartsWith("app."))
                {
                    currentApplication = ExtractApplication(trimmedLine);
                    continue;
                }

                // Check for voice command (text in quotes followed by colon)
                var voiceCommandMatch = Regex.Match(trimmedLine, @"^['""]([^'""]+)['""]:");
                if (voiceCommandMatch.Success)
                {
                    // Save previous command if exists
                    if (currentVoiceCommand != null && scriptLines.Any())
                    {
                        commands.Add(CreateCommand(currentVoiceCommand, scriptLines, currentApplication, repository, filePath));
                    }

                    // Start new command
                    currentVoiceCommand = voiceCommandMatch.Groups[1].Value;
                    scriptLines.Clear();
                    inCommandBlock = true;
                    
                    // Check if there's script on the same line
                    var scriptPart = trimmedLine.Substring(voiceCommandMatch.Length).Trim();
                    if (!string.IsNullOrEmpty(scriptPart))
                    {
                        scriptLines.Add(scriptPart);
                    }
                    continue;
                }

                // Collect script lines
                if (inCommandBlock && !string.IsNullOrEmpty(trimmedLine))
                {
                    scriptLines.Add(trimmedLine);
                }
            }

            // Add the last command
            if (currentVoiceCommand != null && scriptLines.Any())
            {
                commands.Add(CreateCommand(currentVoiceCommand, scriptLines, currentApplication, repository, filePath));
            }

            return commands;
        }

    public List<TalonList> ParseTalonListsFile(string content, string? repository)
        {
            var lists = new List<TalonList>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            string? currentListName = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Check for list definition (line ending with colon)
                if (trimmedLine.EndsWith(":") && !trimmedLine.Contains(" "))
                {
                    currentListName = trimmedLine.TrimEnd(':');
                    continue;
                }

                // Add list item
                if (currentListName != null && !string.IsNullOrEmpty(trimmedLine))
                {
                    lists.Add(new TalonList
                    {
                        ListName = currentListName,
                        Value = trimmedLine,
                        Repository = repository ?? string.Empty
                    });
                }
            }

            return lists;
        }

        private string ExtractApplication(string line)
        {
            var match = Regex.Match(line, @"app[:\.](.+)");
            return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
        }

    private TalonVoiceCommand CreateCommand(string voiceCommand, List<string> scriptLines, string? application, string? repository, string filePath)
        {
            return new TalonVoiceCommand
            {
                Command = voiceCommand,
                Script = string.Join("\n", scriptLines),
                Application = application ?? "General",
                Repository = repository ?? string.Empty,
                FileName = filePath
            };
        }
    }
}