using RCLTalonShared.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RCLTalonShared.Services
{
    public class TalonImportService
    {
        public List<TalonVoiceCommand> ParseTalonFile(string content, string filePath, string? repository)
        {
            var commands = new List<TalonVoiceCommand>();
            var lines = content.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
            
            string? currentApplication = null;
            bool inCommandBlock = false;
            string? currentVoiceCommand = null;
            var scriptLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("#") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                if (trimmedLine.StartsWith("app:") || trimmedLine.StartsWith("app."))
                {
                    currentApplication = ExtractApplication(trimmedLine);
                    continue;
                }

                var voiceCommandMatch = Regex.Match(trimmedLine, "^[\'\"]([^\'\"]+)[\'\"]:");
                if (voiceCommandMatch.Success)
                {
                    if (currentVoiceCommand != null && scriptLines.Count > 0)
                    {
                        commands.Add(CreateCommand(currentVoiceCommand, scriptLines, currentApplication, repository, filePath));
                    }

                    currentVoiceCommand = voiceCommandMatch.Groups[1].Value;
                    scriptLines.Clear();
                    inCommandBlock = true;

                    var scriptPart = trimmedLine.Substring(voiceCommandMatch.Length).Trim();
                    if (!string.IsNullOrEmpty(scriptPart))
                    {
                        scriptLines.Add(scriptPart);
                    }
                    continue;
                }

                if (inCommandBlock && !string.IsNullOrEmpty(trimmedLine))
                {
                    scriptLines.Add(trimmedLine);
                }
            }

            if (currentVoiceCommand != null && scriptLines.Count > 0)
            {
                commands.Add(CreateCommand(currentVoiceCommand, scriptLines, currentApplication, repository, filePath));
            }

            return commands;
        }

        public List<TalonList> ParseTalonListsFile(string content, string? repository)
        {
            var lists = new List<TalonList>();
            var lines = content.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
            
            string? currentListName = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.EndsWith(":") && !trimmedLine.Contains(" "))
                {
                    currentListName = trimmedLine.TrimEnd(':');
                    continue;
                }

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
                VoiceCommand = voiceCommand,
                TalonScript = string.Join("\n", scriptLines),
                Application = application ?? "General",
                Repository = repository ?? string.Empty,
                FilePath = filePath
            };
        }
    }
}
