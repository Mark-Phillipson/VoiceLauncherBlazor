using VoiceLauncherWasm.Models;
using VoiceLauncherWasm.Repositories;
using System.Text.RegularExpressions;

namespace VoiceLauncherWasm.Services
{
    public class TalonVoiceCommandService
    {
        private readonly ITalonVoiceCommandRepository _repository;

        public TalonVoiceCommandService(ITalonVoiceCommandRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TalonVoiceCommand>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchAsync(string searchTerm)
        {
            return await _repository.SearchAsync(searchTerm);
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchCommandNamesOnlyAsync(string searchTerm)
        {
            return await _repository.SearchCommandNamesOnlyAsync(searchTerm);
        }

        public async Task<IEnumerable<TalonVoiceCommand>> SearchScriptOnlyAsync(string searchTerm)
        {
            return await _repository.SearchScriptOnlyAsync(searchTerm);
        }

        public async Task<int> ImportFromTalonFileContentAsync(string fileContent, string fileName)
        {
            var commands = ParseTalonFileContent(fileContent, fileName);
            return await _repository.ImportCommandsAsync(commands);
        }

        public async Task<int> ClearAllCommandsAsync()
        {
            await _repository.ClearAllAsync();
            return 0;
        }

        public async Task<int> GetCountAsync()
        {
            return await _repository.GetCountAsync();
        }

        private List<TalonVoiceCommand> ParseTalonFileContent(string fileContent, string fileName)
        {
            var commands = new List<TalonVoiceCommand>();
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            string application = "global";
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
            if (!hasHeaderSection)
            {
                inCommandsSection = true;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var line = rawLine.Trim();

                if (!inCommandsSection)
                {
                    // Robust delimiter check: ignore all whitespace and carriage returns
                    var delimiterCheck = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    if (delimiterCheck == "-")
                    {
                        inCommandsSection = true;
                        continue;
                    }

                    // Parse header information
                    if (line.StartsWith("app:", StringComparison.OrdinalIgnoreCase))
                    {
                        application = line.Substring(4).Trim();
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
                    else if (line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))
                    {
                        title = line.Substring(6).Trim();
                    }
                    // Add other header parsing as needed
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
                    i = j - 1;

                    commands.Add(new TalonVoiceCommand
                    {
                        Command = command.Length > 200 ? command.Substring(0, 200) : command,
                        Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                        Application = application.Length > 200 ? application.Substring(0, 200) : application,
                        Title = title != null && title.Length > 200 ? title.Substring(0, 200) : title,
                        Mode = modes.Count > 0 ? string.Join(", ", modes).Substring(0, Math.Min(300, string.Join(", ", modes).Length)) : null,
                        OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                        FilePath = fileName.Length > 500 ? fileName.Substring(fileName.Length - 500) : fileName,
                        Repository = ExtractRepositoryFromPath(fileName),
                        Tags = tags.Count > 0 ? string.Join(", ", tags).Substring(0, Math.Min(500, string.Join(", ", tags).Length)) : null,
                        CodeLanguage = codeLanguages.Count > 0 ? string.Join(", ", codeLanguages).Substring(0, Math.Min(300, string.Join(", ", codeLanguages).Length)) : null,
                        Language = languages.Count > 0 ? string.Join(", ", languages).Substring(0, Math.Min(300, string.Join(", ", languages).Length)) : null,
                        Hostname = hostnames.Count > 0 ? string.Join(", ", hostnames).Substring(0, Math.Min(300, string.Join(", ", hostnames).Length)) : null,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return commands;
        }

        private string? ExtractRepositoryFromPath(string filePath)
        {
            try
            {
                var normalizedPath = filePath.Replace('\\', '/');

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
    }
}