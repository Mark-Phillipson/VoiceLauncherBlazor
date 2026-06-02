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
#if USE_LOCAL_EMBEDDINGS
using SmartComponents.LocalEmbeddings;
#endif

namespace DataAccessLibrary.Services
{
    public class TalonVoiceCommandDataService : ITalonVoiceCommandDataService
    {
        private readonly ApplicationDbContext _context;
    #if USE_LOCAL_EMBEDDINGS
        private static readonly LocalEmbedder _embedder = new LocalEmbedder();
    #endif
        
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
        private static string GetApplicationKey(string? application)
        {
            if (string.IsNullOrWhiteSpace(application)) return "global";
            var lower = application.ToLowerInvariant();
            if (lower.Contains("vscode") || lower.Contains("visual studio code") || lower.Contains("code")) return "vscode";
            if (lower.Contains("visual studio") || lower.Contains("visual_studio") || lower.Contains("visualstudio")) return "visual_studio";
            if (lower.Contains("edge") || lower.Contains("microsoft edge")) return "edge";
            if (lower.Contains("chrome")) return "chrome";
            if (lower.Contains("firefox")) return "firefox";
            if (lower.Contains("spotify")) return "spotify";
            if (lower.Contains("windowsterminal") || lower.Contains("windows terminal") || lower.Contains("windowsterminal.exe")) return "windowsterminal";
            if (lower.Contains("azure data studio") || lower.Contains("azure_data_studio")) return "azure_data_studio";
            if (lower.Contains("sql server") || lower.Contains("ssms")) return "ssms";
            if (lower.Contains("explorer") || lower.Contains("windows_file_browser") || lower.Contains("explorer.exe")) return "explorer";
            if (lower.Contains("terminal") || lower.Contains("bash") || lower.Contains("powershell") || lower.Contains("cmd")) return "terminal";
            return "global";
        }

        private static bool ShouldPreferCommandIntentForShortcut(string? command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;

            var trimmed = command.Trim();
            if (trimmed.StartsWith("^")) trimmed = trimmed.Substring(1).Trim();

            // Keep literal press-style commands as press-style.
            if (trimmed.StartsWith("press ", StringComparison.OrdinalIgnoreCase)) return false;
            if (trimmed.StartsWith("press<", StringComparison.OrdinalIgnoreCase)) return false;

            var intent = RephraseCommandForPrompt(command);
            if (string.IsNullOrWhiteSpace(intent)) return false;
            if (string.Equals(intent, "perform that action", StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        private static bool ShouldPreferCommandIntent(string? command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            var intent = RephraseCommandForPrompt(command);
            if (string.IsNullOrWhiteSpace(intent)) return false;
            if (string.Equals(intent, "perform that action", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static bool IsFootSwitchFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            var fileName = Path.GetFileName(filePath);
            return fileName.IndexOf("foot_switch", StringComparison.OrdinalIgnoreCase) >= 0
                || fileName.IndexOf("footswitch", StringComparison.OrdinalIgnoreCase) >= 0
                || fileName.IndexOf("foot_pedal", StringComparison.OrdinalIgnoreCase) >= 0
                || fileName.IndexOf("footpedal", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string? DescribeFootSwitchTrigger(string? command, string? script, string? filePath)
        {
            if (!IsFootSwitchFile(filePath)) return null;
            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            if (script != null && script.IndexOf("mouse_click(0)", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"click the left mouse button with {fileName}";

            if (script != null && script.IndexOf("mouse_click(1)", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"click the right mouse button with {fileName}";

            if (script != null && script.IndexOf("mouse_click(2)", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"click the middle mouse button with {fileName}";

            if (script != null && script.IndexOf("speech.toggle", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"toggle speech with {fileName}";

            if (script != null && script.IndexOf("mouse_click", StringComparison.OrdinalIgnoreCase) >= 0)
                return $"click the mouse with {fileName}";

            return null;
        }

        private static bool IsRawHardwareTrigger(string? command, string? script, string? filePath)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;

            if (IsFootSwitchFile(filePath)) return false;

            var normalizedCommand = command.Trim();
            if (System.Text.RegularExpressions.Regex.IsMatch(normalizedCommand, "^key\\(f\\d+\\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return true;

            // Treat footpedal-style bindings and other raw function-key triggers as non-quiz material.
            if (string.Equals(normalizedCommand, "key(f15)", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedCommand, "key(f14)", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedCommand, "key(f13)", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedCommand, "key(f9)", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(script)
                && (script.IndexOf("mouse_click", StringComparison.OrdinalIgnoreCase) >= 0
                    || script.IndexOf("speech.toggle", StringComparison.OrdinalIgnoreCase) >= 0)
                && System.Text.RegularExpressions.Regex.IsMatch(normalizedCommand, "^key\\(f\\d+\\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        public async Task<int> ImportFromTalonFilesAsync(string rootFolder)
        {
            // Preserve any existing descriptions so they can be re-applied after refresh
            var existingDescMap = await GetExistingDescriptionMapAsync();

            // Remove all existing records before importing new ones
            _context.TalonVoiceCommands.RemoveRange(_context.TalonVoiceCommands);
            await _context.SaveChangesAsync();
            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            var commands = new List<TalonVoiceCommand>();
            foreach (var file in talonFiles)
            {
                var lines = await File.ReadAllLinesAsync(file);
                List<string> applications = new();
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
                            applications.Add(parsedApp);
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
                        i = j - 1;
                        var appStr = applications.Count > 0 ? string.Join(", ", applications) : "global";
                        var derivedDesc = await DerivePlainLanguageDescriptionAsync(script, title, command, appStr, file);
                        var key = ComputeMergeKey(command, appStr);
                        string? finalDesc = null;
                        if (existingDescMap != null && existingDescMap.TryGetValue(key, out var prevDesc) && !string.IsNullOrWhiteSpace(prevDesc))
                        {
                            finalDesc = prevDesc;
                        }
                        else
                        {
                            finalDesc = derivedDesc;
                        }
                        if (finalDesc != null && finalDesc.Length > 2000) finalDesc = finalDesc.Substring(0, 2000);

                        commands.Add(new TalonVoiceCommand
                        {
                            Command = command.Length > 200 ? command.Substring(0, 200) : command,
                            Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                            Application = appStr.Length > 200 ? appStr.Substring(0, 200) : appStr,
                            Title = title != null && title.Length > 200 ? title.Substring(0, 200) : title,
                            Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                            OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                            FilePath = file.Length > 500 ? file.Substring(file.Length - 500) : file,
                            Repository = ExtractRepositoryFromPath(file)?.Length > 200 ? ExtractRepositoryFromPath(file)?.Substring(0, 200) : ExtractRepositoryFromPath(file),
                            Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                            CodeLanguage = codeLanguages.Count > 0 ? string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Substring(0, Math.Min(300, string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Length)) : null,
                            Language = languages.Count > 0 ? string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Substring(0, Math.Min(300, string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Length)) : null,
                            Hostname = hostnames.Count > 0 ? string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Substring(0, Math.Min(300, string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Length)) : null,
                            Description = finalDesc,
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

#if USE_LOCAL_EMBEDDINGS
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
#else
                // Local embeddings disabled: fallback to basic text search
                var lowerTerm = searchTerm.ToLower();
                var fallback = allCommands
                    .Where(c => (!string.IsNullOrEmpty(c.Title) && c.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(c.Command) && c.Command.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(c.Script) && c.Script.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(c.Application) && c.Application.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(c.Mode) && c.Mode.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(100)
                    .ToList();
                Console.WriteLine($"[DEBUG] Semantic search disabled; using fallback text search for '{searchTerm}': {fallback.Count} results");
                return fallback;
#endif
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
        
        public async Task<TalonVoiceCommand?> GetCommandByIdAsync(int id)
        {
            return await _context.TalonVoiceCommands.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<DataAccessLibrary.DTO.QuizPackDTO> GenerateQuizPackAsync(string? applicationFilter = null, int questionCount = 10, int distractors = 3)
        {
            // Perform database-side filtering for simple predicates, then apply
            // the more complex IsRawHardwareTrigger check in-memory to avoid
            // EF Core translation errors for custom methods.
            var preFiltered = await _context.TalonVoiceCommands
                .Where(c => !string.IsNullOrWhiteSpace(c.Description))
                .Where(c => c.Command != null && !c.Command.Contains("<") && !c.Command.Contains("{"))
                .Where(c => c.Description != null && !c.Description.Contains("list+not+found"))
                .ToListAsync();

            var all = preFiltered.Where(c => !IsRawHardwareTrigger(c.Command, c.Script, c.FilePath)).ToList();

            if (!string.IsNullOrWhiteSpace(applicationFilter) && !applicationFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                all = all.Where(c => c.Application != null && c.Application.IndexOf(applicationFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            // Shuffle
            var rng = new Random();
            all = all.OrderBy(x => rng.Next()).ToList();

            var selected = all.Take(questionCount).ToList();

            var pack = new DataAccessLibrary.DTO.QuizPackDTO
            {
                Source = "talon-voice-command",
                ApplicationFilter = applicationFilter,
                QuestionCount = selected.Count
            };

            // Prebuild candidate map for distractors by application
            var byApp = all.GroupBy(c => c.Application ?? "global").ToDictionary(g => g.Key, g => g.ToList());

            foreach (var cmd in selected)
            {
                var q = new DataAccessLibrary.DTO.QuizQuestionDTO
                {
                    SourceCommandId = cmd.Id,
                    RelatedCommandId = cmd.Id
                };

                // Build prompt: prefer a clean Description, but if it's ambiguous fall back to rephrasing the spoken `Command`.
                string prompt;
                // Extract primary tag if present
                string? primaryTag = null;
                if (!string.IsNullOrWhiteSpace(cmd.Tags))
                {
                    primaryTag = System.Text.RegularExpressions.Regex.Split(cmd.Tags ?? string.Empty, "[,;|]+")
                        .Select(t => t.Trim())
                        .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                }

                if (!string.IsNullOrWhiteSpace(cmd.Description) && !IsAmbiguousDescription(cmd.Description, cmd.Script ?? string.Empty, cmd.Command))
                {
                    prompt = cmd.Description!;
                    // Ensure application, tag or code language context is present
                    if (!string.IsNullOrWhiteSpace(cmd.Application) && !cmd.Application.Equals("global", StringComparison.OrdinalIgnoreCase) && !prompt.Contains(cmd.Application))
                        prompt += $" in {cmd.Application}";
                    if (!string.IsNullOrWhiteSpace(primaryTag) && !prompt.Contains(primaryTag))
                        prompt += $" (tag: {primaryTag})";
                    else if (!string.IsNullOrWhiteSpace(cmd.CodeLanguage) && !prompt.Contains(cmd.CodeLanguage))
                        prompt += $" when working in {cmd.CodeLanguage}";
                }
                else
                {
                    // First try to derive a plain-language action from the script/title/command
                    var derivedAction = await DerivePlainLanguageDescriptionAsync(cmd.Script ?? string.Empty, cmd.Title, cmd.Command, null, cmd.FilePath);
                    var action = !string.IsNullOrWhiteSpace(derivedAction) ? derivedAction : RephraseCommandForPrompt(cmd.Command);

                    var appPart = !string.IsNullOrWhiteSpace(cmd.Application) && !cmd.Application.Equals("global", StringComparison.OrdinalIgnoreCase)
                        ? $"in {FormatAppForPrompt(cmd.Application)}"
                        : string.Empty;
                    var langPart = !string.IsNullOrWhiteSpace(cmd.CodeLanguage) ? $"when working in {cmd.CodeLanguage}" : string.Empty;
                    var tagPart = !string.IsNullOrWhiteSpace(primaryTag) ? $"with tag '{primaryTag}' active" : string.Empty;
                    var osPart = !string.IsNullOrWhiteSpace(cmd.OperatingSystem) ? $"on {cmd.OperatingSystem}" : string.Empty;

                    var parts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(appPart)) parts.Add(appPart);
                    if (!string.IsNullOrWhiteSpace(tagPart)) parts.Add(tagPart);
                    if (!string.IsNullOrWhiteSpace(osPart)) parts.Add(osPart);
                    if (!string.IsNullOrWhiteSpace(langPart)) parts.Add(langPart);

                    var context = parts.Count > 0 ? (" (" + string.Join("; ", parts) + ")") : string.Empty;
                    prompt = $"What voice command would you use to {action}{context}?";
                }

                q.Prompt = prompt;

                // Correct choice text: prefer spoken `Command`, cleaned for readability
                var cmdScript = cmd.Script ?? string.Empty;
                string correctText = !string.IsNullOrWhiteSpace(cmd.Command)
                    ? CleanSpokenForm(cmd.Command)
                    : (!string.IsNullOrWhiteSpace(cmd.Title)
                        ? cmd.Title
                        : (cmdScript.Length > 80 ? cmdScript.Substring(0, 80) + "..." : cmdScript));

                var choices = new List<DataAccessLibrary.DTO.ChoiceDTO>();
                choices.Add(new DataAccessLibrary.DTO.ChoiceDTO { Text = correctText, CommandId = cmd.Id });

                // Find distractors: prefer same application
                var candidates = new List<TalonVoiceCommand>();
                if (!string.IsNullOrWhiteSpace(cmd.Application) && byApp.TryGetValue(cmd.Application, out var appList))
                {
                    candidates.AddRange(appList.Where(c => c.Id != cmd.Id));
                }

                // Add global fallback candidates
                candidates.AddRange(all.Where(c => c.Id != cmd.Id && !candidates.Any(x => x.Id == c.Id)));

                // Score candidates by token overlap to pick similar but not identical distractors
                var targetTokens = TokenizeForMatching(correctText);
                var scored = candidates.Select(cand => new { Cand = cand, Score = TokenOverlapScore(targetTokens, TokenizeForMatching(cand.Command ?? cand.Title ?? cand.Script ?? string.Empty)) })
                    .OrderByDescending(x => x.Score).ThenBy(x => rng.Next()).ToList();

                foreach (var s in scored.Take(distractors))
                {
                    var candScript = s.Cand.Script ?? string.Empty;
                    var text = !string.IsNullOrWhiteSpace(s.Cand.Command)
                        ? CleanSpokenForm(s.Cand.Command)
                        : (!string.IsNullOrWhiteSpace(s.Cand.Title)
                            ? s.Cand.Title
                            : (candScript.Length > 80 ? candScript.Substring(0, 80) + "..." : candScript));
                    if (!choices.Any(c => c.Text == text))
                        choices.Add(new DataAccessLibrary.DTO.ChoiceDTO { Text = text, CommandId = s.Cand.Id });
                }

                // If not enough choices, fill with random commands
                var idx = 0;
                while (choices.Count < Math.Max(4, distractors + 1) && idx < all.Count)
                {
                    var cand = all[idx];
                    if (!choices.Any(c => c.CommandId == cand.Id) && cand.Id != cmd.Id)
                    {
                        var text = !string.IsNullOrWhiteSpace(cand.Command) ? CleanSpokenForm(cand.Command) : (!string.IsNullOrWhiteSpace(cand.Title) ? cand.Title : (cand.Script.Length > 80 ? cand.Script.Substring(0, 80) + "..." : cand.Script));
                        if (!choices.Any(c => c.Text == text))
                            choices.Add(new DataAccessLibrary.DTO.ChoiceDTO { Text = text, CommandId = cand.Id });
                    }
                    idx++;
                }

                // Shuffle choices and set correct index
                choices = choices.OrderBy(x => rng.Next()).ToList();
                q.Choices = choices;
                q.CorrectChoiceIndex = choices.FindIndex(c => c.CommandId == cmd.Id);

                // Determine category: prefer primary tag, then application, then code language, else 'global'
                var category = "global";
                if (!string.IsNullOrWhiteSpace(cmd.Tags))
                {
                    var firstTag = System.Text.RegularExpressions.Regex.Split(cmd.Tags ?? string.Empty, "[,;|]+")
                        .Select(t => t.Trim())
                        .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                    if (!string.IsNullOrWhiteSpace(firstTag)) category = firstTag!;
                }
                else if (!string.IsNullOrWhiteSpace(cmd.Application) && !cmd.Application.Equals("global", StringComparison.OrdinalIgnoreCase))
                {
                    category = cmd.Application;
                }
                else if (!string.IsNullOrWhiteSpace(cmd.CodeLanguage))
                {
                    category = cmd.CodeLanguage;
                }

                q.Category = category;

                pack.Questions.Add(q);
            }

            return pack;
        }

        private static string[] TokenizeForMatching(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return new string[0];
            var cleaned = s.ToLowerInvariant();
            var tokens = System.Text.RegularExpressions.Regex.Split(cleaned, "[^a-z0-9]+").Where(t => t.Length > 0).ToArray();
            return tokens;
        }

        private static int TokenOverlapScore(string[] a, string[] b)
        {
            if (a == null || b == null) return 0;
            var setA = new HashSet<string>(a);
            var setB = new HashSet<string>(b);
            setA.IntersectWith(setB);
            return setA.Count;
        }

        private static bool IsAmbiguousDescription(string? desc, string script, string? command)
        {
            if (string.IsNullOrWhiteSpace(desc)) return true;
            var d = desc.Trim();
            // If description is identical to script or contains many code-like tokens, treat as ambiguous
            if (!string.IsNullOrWhiteSpace(script) && d.Equals(script.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            var codeChars = new[] { '{', '}', '<', '>', '(', ')', '[', ']', ';', ':', '=', '"', '\'' };
            var nonAlpha = d.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            if (nonAlpha > d.Length / 6) return true; // too many non-alphanumeric chars
            if (d.IndexOf("->", StringComparison.OrdinalIgnoreCase) >= 0 || d.IndexOf("|", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            // If description contains common talon placeholder prefixes, treat as ambiguous
            if (d.IndexOf("user.", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (d.IndexOf("\\u003c", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            // Detect dot-separated code-like identifiers
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "\\b[a-z0-9_]+\\.[a-z0-9_]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return true;
            // Detect underscored identifiers and coordinate-like output that read like raw script instead of intent
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "\\b[a-z0-9]+_[a-z0-9_]+\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "\\b\\d{2,}\\s*,\\s*\\d{2,}\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "\\b(mouse|key|insert|sleep|edit|clip|wheel|drag)_", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return true;
            // If description starts with a raw 'press <modifier>' style phrase, treat as ambiguous
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "\\bpress\\s+(ctrl|control|alt|shift|cmd|win|meta|super)\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return true;

            // If description is extremely short (1-2 words) it's likely ambiguous
            var wordCount = d.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount <= 2) return true;
            return false;
        }
        // stray placeholder removed

        private static string RephraseCommandForPrompt(string? command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "perform that action";
            var s = command.Trim();
            // Remove anchors and leading/trailing punctuation
            s = System.Text.RegularExpressions.Regex.Replace(s, "^[\\^\\$\\s]+|[\\s\\^\\$]+$", "");
            // Replace common separators with spaces and trim
            s = s.Replace('_', ' ').Replace('.', ' ').Replace(':', ' ').Replace("  ", " ").Trim();

            // Drop common namespace prefixes (e.g. 'user', 'global', 'cursorless') so prompts use the meaningful part
            var firstToken = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? string.Empty;
            var prefixes = new[] { "user", "global", "cursorless", "vscode", "code", "talon", "app" };
            if (prefixes.Contains(firstToken))
            {
                var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                    s = string.Join(' ', parts.Skip(1));
            }

            // Handle optional bracketed groups like [prev | previous] -> pick the longest alternative (prefer readability)
                s = System.Text.RegularExpressions.Regex.Replace(s, @"\[([^\]]+)\]", m =>
            {
                var opts = m.Groups[1].Value.Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                if (!opts.Any()) return "";
                // prefer longest (more descriptive) and skip bare 'the'
                var best = opts.Where(o => !o.Equals("the", StringComparison.OrdinalIgnoreCase)).OrderByDescending(o => o.Length).FirstOrDefault() ?? opts.First();
                return best;
            });

            // Replace capture/list placeholders {user.foo} and <user.foo> with a readable placeholder
            s = System.Text.RegularExpressions.Regex.Replace(s, "[\\{<]([^>\\}]+)[\\}>]", m =>
            {
                var token = m.Groups[1].Value;
                // take last segment after '.' or '_' if present
                var parts = token.Split(new[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var last = parts.LastOrDefault() ?? token;
                last = last.Replace("user", "").Trim();
                if (string.IsNullOrWhiteSpace(last)) return "a value";
                // turn camelCase/underscores into words
                last = System.Text.RegularExpressions.Regex.Replace(last, "([a-z])([A-Z])", "$1 $2");
                last = last.Replace("-", " ").Replace("_", " ");
                return last.Length <= 2 ? last : last;
            });

            // Clean leftover punctuation
            s = System.Text.RegularExpressions.Regex.Replace(s, "[\\(\\)\\\"\\']", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ").Trim();

            // Lowercase start for natural continuation in prompts
            if (s.Length > 0) s = char.ToLowerInvariant(s[0]) + s.Substring(1);
            return s;
        }

        private static string CleanSpokenForm(string? spoken)
        {
            if (string.IsNullOrWhiteSpace(spoken)) return string.Empty;
            var s = spoken.Trim();
            // Normalize escaped angle brackets sometimes present in serialized values
            s = s.Replace("\\u003c", "<").Replace("\\u003e", ">");
            // Normalize dotted prefixes and drop common leading namespace tokens
            s = s.Replace('.', ' ');
            var first = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? string.Empty;
            var commonPrefixes = new[] { "user", "global", "cursorless", "vscode", "code", "talon", "app" };
            if (commonPrefixes.Contains(first))
            {
                var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1) s = string.Join(' ', parts.Skip(1));
            }
            // Remove caret/anchors
            s = System.Text.RegularExpressions.Regex.Replace(s, "^[\\^\\$]+|[\\^\\$]+$", "");
            // Expand optional groups similar to RephraseCommandForPrompt
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\[([^\]]+)\]", m =>
            {
                var opts = m.Groups[1].Value.Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                if (!opts.Any()) return "";
                // pick the most natural looking option (prefer longest)
                var best = opts.OrderByDescending(o => o.Length).First();
                return best;
            });
            // Replace list/capture placeholders
            s = System.Text.RegularExpressions.Regex.Replace(s, "[\\{<]([^>\\}]+)[\\}>]", m =>
            {
                var token = m.Groups[1].Value;
                var parts = token.Split(new[] { '.', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var last = parts.LastOrDefault() ?? token;
                last = last.Replace("user", "").Trim();
                last = last.Replace("-", " ").Replace("_", " ");
                return last;
            });
            // Remove remaining punctuation that's not helpful
            s = System.Text.RegularExpressions.Regex.Replace(s, "[\\(\\)\\\"\\'{}<>]", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ").Trim();
            return s;
        }

        private static string FormatAppForPrompt(string app)
        {
            if (string.IsNullOrWhiteSpace(app)) return "global";
            var t = app.Trim();
            if (string.Equals(t, "global", StringComparison.OrdinalIgnoreCase)) return "global";
            var lower = t.ToLowerInvariant();
            if (lower.Contains("edge")) return "Microsoft Edge";
            if (lower.Contains("chrome")) return "Google Chrome";
            if (lower.Contains("vscode") || lower.Contains("visual studio code") || lower.Contains("code")) return "Visual Studio Code";
            if (lower.Contains("obs")) return "OBS";
            try
            {
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lower);
            }
            catch
            {
                return t;
            }
        }
        public async Task<int> ImportTalonFileContentAsync(string fileContent, string fileName, System.Collections.Generic.Dictionary<string, string>? preservedDescriptions = null)
        {
            // Do NOT clear the table here; only add new commands
            var commands = new List<TalonVoiceCommand>();            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None); 
            List<string> applications = new();
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
                        applications.Add(parsedApp);
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
                    i = j - 1;
                    var appStr = applications.Count > 0 ? string.Join(", ", applications) : "global";
                    var derivedDesc = await DerivePlainLanguageDescriptionAsync(script, title, command, appStr, fileName);
                    var key = ComputeMergeKey(command, appStr);
                    string? finalDesc = null;
                    if (preservedDescriptions != null && preservedDescriptions.TryGetValue(key, out var prevDesc) && !string.IsNullOrWhiteSpace(prevDesc))
                    {
                        finalDesc = prevDesc;
                    }
                    else
                    {
                        finalDesc = derivedDesc;
                    }
                    if (finalDesc != null && finalDesc.Length > 2000) finalDesc = finalDesc.Substring(0, 2000);

                    commands.Add(new TalonVoiceCommand
                    {
                        Command = command.Length > 200 ? command.Substring(0, 200) : command,
                        Script = script.Length > 2000 ? script.Substring(0, 2000) : script,
                        Application = appStr.Length > 200 ? appStr.Substring(0, 200) : appStr,
                        Title = title != null && title.Length > 200 ? title.Substring(0, 200) : title,
                        Mode = modes.Count > 0 ? string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Substring(0, Math.Min(300, string.Join(", ", modes.Select(m => m.Length > 100 ? m.Substring(0, 100) : m)).Length)) : null,
                        OperatingSystem = operatingSystem != null && operatingSystem.Length > 100 ? operatingSystem.Substring(0, 100) : operatingSystem,
                        FilePath = fileName.Length > 500 ? fileName.Substring(fileName.Length - 500) : fileName,
                        Repository = ExtractRepositoryFromPath(fileName)?.Length > 200 ? ExtractRepositoryFromPath(fileName)?.Substring(0, 200) : ExtractRepositoryFromPath(fileName),
                        Tags = tags.Count > 0 ? string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Substring(0, Math.Min(500, string.Join(", ", tags.Select(t => t.Length > 50 ? t.Substring(0, 50) : t)).Length)) : null,
                        CodeLanguage = codeLanguages.Count > 0 ? string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Substring(0, Math.Min(300, string.Join(", ", codeLanguages.Select(cl => cl.Length > 100 ? cl.Substring(0, 100) : cl)).Length)) : null,
                        Language = languages.Count > 0 ? string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Substring(0, Math.Min(300, string.Join(", ", languages.Select(l => l.Length > 100 ? l.Substring(0, 100) : l)).Length)) : null,
                        Hostname = hostnames.Count > 0 ? string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Substring(0, Math.Min(300, string.Join(", ", hostnames.Select(h => h.Length > 100 ? h.Substring(0, 100) : h)).Length)) : null,
                        Description = finalDesc,
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
            // Preserve existing descriptions
            var existingDescMap = await GetExistingDescriptionMapAsync();

            // Remove all existing records before importing new ones
            _context.TalonVoiceCommands.RemoveRange(_context.TalonVoiceCommands);
            await _context.SaveChangesAsync();
            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            int totalImported = 0;
            foreach (var file in talonFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                totalImported += await ImportTalonFileContentAsync(content, file, existingDescMap);
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
            // Preserve existing descriptions
            var existingDescMap = await GetExistingDescriptionMapAsync();

            // Clear all existing records first
            await ClearAllCommandsAsync();

            var talonFiles = Directory.GetFiles(rootFolder, "*.talon", SearchOption.AllDirectories);
            int totalImported = 0;
            int filesProcessed = 0;

            foreach (var file in talonFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var commandsFromThisFile = await ImportTalonFileContentAsync(content, file, existingDescMap);
                totalImported += commandsFromThisFile;
                filesProcessed++;
                // Report progress: (filesProcessed, totalFiles, totalCommandsSoFar)
                progressCallback?.Invoke(filesProcessed, talonFiles.Length, totalImported);
            }
            return totalImported;
        }

        public async Task<int> BackfillDescriptionsAsync()
        {
            var all = await _context.TalonVoiceCommands.ToListAsync();
            int updated = 0;
            foreach (var cmd in all)
            {
                try
                {
                    var newDesc = await DerivePlainLanguageDescriptionAsync(cmd.Script ?? string.Empty, cmd.Title, cmd.Command, cmd.Application, cmd.FilePath);
                    if (!string.IsNullOrWhiteSpace(newDesc) && newDesc != cmd.Description)
                    {
                        cmd.Description = newDesc;
                        updated++;
                    }
                }
                catch
                {
                    // ignore per-row errors
                }
            }
            if (updated > 0)
                await _context.SaveChangesAsync();
            return updated;
        }

        public async Task<int> RecreateAllDescriptionsAsync()
        {
            var all = await _context.TalonVoiceCommands.ToListAsync();
            int updated = 0;
            foreach (var cmd in all)
            {
                try
                {
                    if (IsRawHardwareTrigger(cmd.Command, cmd.Script, cmd.FilePath))
                    {
                        if (!string.IsNullOrWhiteSpace(cmd.Description))
                        {
                            cmd.Description = null;
                            updated++;
                        }
                        continue;
                    }

                    var newDesc = await DerivePlainLanguageDescriptionAsync(cmd.Script ?? string.Empty, cmd.Title, cmd.Command, cmd.Application, cmd.FilePath);
                    if (string.IsNullOrWhiteSpace(newDesc))
                    {
                        newDesc = RephraseCommandForPrompt(cmd.Command);
                    }

                    if (!string.IsNullOrWhiteSpace(newDesc))
                    {
                        if (!string.IsNullOrWhiteSpace(cmd.OperatingSystem) && !newDesc.Contains(cmd.OperatingSystem, StringComparison.OrdinalIgnoreCase))
                        {
                            newDesc = $"{newDesc} (on {cmd.OperatingSystem})";
                        }

                        // Update when the derived description differs, or when the existing description
                        // is considered ambiguous (force-rewrite ambiguous entries to the derived text).
                        var shouldUpdate = !string.Equals(newDesc, cmd.Description, StringComparison.Ordinal);
                        if (!shouldUpdate && IsAmbiguousDescription(cmd.Description, cmd.Script ?? string.Empty, cmd.Command))
                        {
                            shouldUpdate = true;
                        }

                        if (shouldUpdate)
                        {
                            cmd.Description = newDesc.Length > 2000 ? newDesc.Substring(0, 2000) : newDesc;
                            updated++;
                        }
                    }
                }
                catch
                {
                    // ignore per-row errors
                }
            }
            if (updated > 0)
                await _context.SaveChangesAsync();
            return updated;
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
#if USE_LOCAL_EMBEDDINGS
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
#else
                // Local embeddings disabled: simple fallback using substring matching
                var literalMatches = allCommands.Where(c =>
                    (!string.IsNullOrEmpty(c.Title) && c.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(c.Command) && c.Command.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(c.Script) && c.Script.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                ).OrderByDescending(c => c.CreatedAt).ToList();

                var semanticMatches = literalMatches;
#endif

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
        /// Derives a plain-language description from a talon script line.
        /// Prefers the provided title when available. Expands lists before attempting heuristics.
        /// Returns null when no useful description can be derived (caller may skip such entries).
        /// </summary>
        private async Task<string?> DerivePlainLanguageDescriptionAsync(string script, string? title, string? command, string? application = null, string? filePath = null)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return title.Trim();
            }

            if (string.IsNullOrWhiteSpace(script)) return null;

            string expanded = script;
            try
            {
                expanded = await ExpandListsInScriptAsync(script);
            }
            catch
            {
                expanded = script;
            }

            // Keep original line breaks for sequence analysis
            var sWithNewlines = expanded.Replace("\r\n", "\n").Replace('\r', '\n');
            var s = System.Text.RegularExpressions.Regex.Replace(expanded, "\\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(s)) return null;

            var footSwitchDescription = DescribeFootSwitchTrigger(command, s, filePath);
            if (!string.IsNullOrWhiteSpace(footSwitchDescription))
            {
                if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                {
                    footSwitchDescription += $" in {FormatAppForPrompt(application)}";
                }
                return footSwitchDescription;
            }

            // If this is fundamentally a shortcut wrapper, prefer the spoken command text as the intent.
            // This avoids useless prompts like "press H" when the command is actually "next heading".
            if (ShouldPreferCommandIntentForShortcut(command))
            {
                var shortcutLikeScript = System.Text.RegularExpressions.Regex.IsMatch(
                    s,
                    "^(key\\(|user\\.[\\w_]*press\\(|user\\.with_[\\w_]*press\\()",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (shortcutLikeScript)
                {
                    var shortcutIntent = RephraseCommandForPrompt(command);
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                    {
                        shortcutIntent += $" in {FormatAppForPrompt(application)}";
                    }
                    return shortcutIntent;
                }
            }

            // More generally, if the script already looks like low-level code or automation internals,
            // prefer the spoken command meaning over exposing raw script details in quiz prompts.
            if (ShouldPreferCommandIntent(command))
            {
                var codeLikeScript = System.Text.RegularExpressions.Regex.IsMatch(
                    s,
                    "(mouse_|key\\(|insert\\(|sleep\\(|user\\.|edit\\.|clip\\.|wheel_|drag_|\\b\\d{2,}\\s*,\\s*\\d{2,}\\b)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (codeLikeScript)
                {
                    var commandIntent = RephraseCommandForPrompt(command);
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                    {
                        commandIntent += $" in {FormatAppForPrompt(application)}";
                    }
                    return commandIntent;
                }
            }

            // Normalize application into a short key so we can apply app-specific combo mappings
            var appKey = GetApplicationKey(application);
            var appOverrides = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "vscode", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+n", "create a new file or item" },
                        { "ctrl+shift+p", "open the command palette" },
                        { "ctrl+p", "quick open file" },
                        { "ctrl+b", "toggle the sidebar" },
                        { "ctrl+shift+f", "search across files" },
                        { "ctrl+tab", "switch to the next editor tab" }
                    }
                },
                { "chrome", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+n", "open a new browser window" },
                        { "ctrl+t", "open a new tab" },
                        { "ctrl+w", "close the current tab" },
                        { "ctrl+shift+t", "reopen the last closed tab" },
                        { "ctrl+f", "find on the page" },
                        { "ctrl+l", "focus the address bar" }
                    }
                },
                { "edge", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+n", "open a new browser window" },
                        { "ctrl+t", "open a new tab" },
                        { "ctrl+w", "close the current tab" },
                        { "ctrl+f", "find on the page" }
                    }
                },
                { "windowsterminal", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+c", "interrupt the current process" },
                        { "ctrl+v", "paste from the clipboard" },
                        { "ctrl+shift+c", "copy the selection" }
                    }
                },
                { "visual_studio", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+n", "create a new file" },
                        { "ctrl+shift+n", "open a new project window" },
                        { "ctrl+shift+f", "find in files" }
                    }
                },
                { "explorer", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "ctrl+n", "create a new folder" }
                    }
                }
            };

            // Sequence detection: look for ordered key(...) and insert(...) tokens and produce combined plain-language descriptions
            try
            {
                var tokenPattern = new System.Text.RegularExpressions.Regex(
                    "key\\(\\s*['\"]?([^'\")]+)['\"]?\\s*\\)|insert\\(\\s*['\"](?<insert>.+?)['\"]\\s*\\)|mouse\\.(?<mouse>\\w+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var matches = tokenPattern.Matches(sWithNewlines);
                if (matches.Count > 0)
                {
                    var tokens = new List<(string Type, string Value)>();
                    foreach (System.Text.RegularExpressions.Match m in matches)
                    {
                        if (m.Groups[1].Success)
                        {
                            tokens.Add(("key", m.Groups[1].Value.Trim()));
                        }
                        else if (m.Groups["insert"].Success)
                        {
                            tokens.Add(("insert", m.Groups["insert"].Value));
                        }
                        else if (m.Groups["mouse"].Success)
                        {
                            tokens.Add(("mouse", m.Groups["mouse"].Value));
                        }
                    }

                    // Helper to normalize key names
                    static string NormalizeKeyForSeq(string key)
                    {
                        var k = System.Text.RegularExpressions.Regex.Replace(key.ToLowerInvariant(), "[\"'\\s]+", "");
                        if (k == "win" || k == "windows" || k == "super" || k == "meta" || k == "command") return "super";
                        if (k == "return") return "enter";
                        return k;
                    }

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        var t = tokens[i];
                        if (t.Type == "key")
                        {
                            var nk = NormalizeKeyForSeq(t.Value);
                            // Super/Windows key + insert("text") => open Start and type
                            if (nk == "super")
                            {
                                string? inserted = null;
                                bool hasEnter = false;
                                if (i + 1 < tokens.Count && tokens[i + 1].Type == "insert")
                                {
                                    inserted = tokens[i + 1].Value;
                                }
                                if (i + 2 < tokens.Count && tokens[i + 2].Type == "key")
                                {
                                    var nextKey = NormalizeKeyForSeq(tokens[i + 2].Value);
                                    if (nextKey == "enter") hasEnter = true;
                                }

                                if (!string.IsNullOrWhiteSpace(inserted))
                                {
                                    var preview = inserted.Length > 120 ? inserted.Substring(0, 120) + "..." : inserted;
                                    var desc = $"open Start and type \"{preview}\"";
                                    if (hasEnter) desc += ", then press Enter";
                                    return desc;
                                }
                            }

                            // Win+R (super + r) -> open Run dialog
                            if (nk == "super")
                            {
                                if (i + 1 < tokens.Count && tokens[i + 1].Type == "key")
                                {
                                    var next = NormalizeKeyForSeq(tokens[i + 1].Value);
                                    if (next == "r") return "open the Run dialog";
                                }
                            }
                        }
                    }

                    // Build a full-sequence description for the whole script (e.g., "press Ctrl+B then press Q")
                    try
                    {
                        var phraseParts = new List<string>();
                        foreach (var tk in tokens)
                        {
                            if (tk.Type == "key")
                            {
                                var raw = tk.Value;
                                var parts = System.Text.RegularExpressions.Regex.Split(raw, "[+\\s-]+");
                                var mapped = parts.Select(p =>
                                {
                                    var pp = p.ToLowerInvariant();
                                    return pp switch
                                    {
                                        "ctrl" or "control" => "Ctrl",
                                        "alt" => "Alt",
                                        "shift" => "Shift",
                                        "super" or "meta" or "win" or "windows" or "command" => "Win",
                                        _ => p.Length == 1 ? p.ToUpperInvariant() : p
                                    };
                                }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                                var keyPhrase = mapped.Length > 1 ? string.Join("+", mapped) : mapped.FirstOrDefault() ?? raw;
                                phraseParts.Add($"press {keyPhrase}");
                            }
                            else if (tk.Type == "insert")
                            {
                                var preview = tk.Value.Length > 120 ? tk.Value.Substring(0, 120) + "..." : tk.Value;
                                phraseParts.Add($"type \"{preview}\"");
                            }
                            else if (tk.Type == "mouse")
                            {
                                // Try to find a numeric mouse arg nearby in the expanded script
                                var mouseNumM = System.Text.RegularExpressions.Regex.Match(sWithNewlines, "mouse[_\\.]?click\\s*\\(?\\s*['\"]?(?<num>\\d+)['\"]?\\s*\\)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (mouseNumM.Success)
                                {
                                    var num = mouseNumM.Groups["num"].Value;
                                    var btn = num == "0" ? "left mouse button" : num == "1" ? "right mouse button" : num == "2" ? "middle mouse button" : $"mouse button {num}";
                                    phraseParts.Add($"click the {btn}");
                                }
                                else if (sWithNewlines.IndexOf("mouse.left", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    phraseParts.Add("click the left mouse button");
                                }
                                else if (sWithNewlines.IndexOf("mouse.right", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    phraseParts.Add("click the right mouse button");
                                }
                                else
                                {
                                    phraseParts.Add("mouse action");
                                }
                            }
                        }

                        if (phraseParts.Count > 0)
                        {
                            var combined = string.Join(", then ", phraseParts);
                            var keysOnly = tokens.Count > 0 && tokens.All(tk => string.Equals(tk.Type, "key", StringComparison.OrdinalIgnoreCase));
                            var commandIntent = RephraseCommandForPrompt(command);
                            var preferCommandIntent = keysOnly && ShouldPreferCommandIntentForShortcut(command);

                            if (ShouldPreferCommandIntentForShortcut(command))
                            {
                                var looksLikeRawAutomation = combined.StartsWith("press ", StringComparison.OrdinalIgnoreCase)
                                    && (combined.Contains(", then ", StringComparison.OrdinalIgnoreCase)
                                        || combined.Contains("list+not+found", StringComparison.OrdinalIgnoreCase)
                                        || combined.Length > 80);

                                if (looksLikeRawAutomation)
                                {
                                    var intent = commandIntent!;
                                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                                    {
                                        intent += $" in {FormatAppForPrompt(application)}";
                                    }
                                    return intent;
                                }
                            }

                            // Map common keyboard combos that the sequence parser returned as "press ..." into plain-language intents
                            var comboMapSeq = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                { "ctrl+a", "select all text" },
                                { "control+a", "select all text" },
                                { "cmd+a", "select all text" },
                                { "meta+a", "select all text" },

                                { "ctrl+n", "create a new file or item" },
                                { "control+n", "create a new file or item" },
                                { "cmd+n", "create a new file or item" },

                                { "ctrl+o", "open a file" },
                                { "control+o", "open a file" },
                                { "cmd+o", "open a file" },

                                { "ctrl+p", "open quick open" },
                                { "ctrl+shift+p", "open the command palette" },
                                { "ctrl+shift+n", "open a new window" },
                                { "ctrl+shift+f", "search across files" },
                                { "ctrl+f", "open the find dialog" },

                                { "ctrl+s", "save the file" },
                                { "control+s", "save the file" },
                                { "cmd+s", "save the file" },
                                { "meta+s", "save the file" },
                                { "ctrl+shift+s", "save the file as" },

                                { "ctrl+w", "close the current tab" },
                                { "alt+f4", "close the window" },
                                { "ctrl+shift+i", "show the developer tools" },

                                { "ctrl+c", "copy the selection" },
                                { "ctrl+v", "paste from the clipboard" },
                                { "ctrl+x", "cut the selection" },
                                { "ctrl+z", "undo the last action" },
                                { "ctrl+y", "redo the last action" },

                                { "ctrl+tab", "switch to the next tab" },
                                { "ctrl+shift+tab", "switch to the previous tab" },
                                { "ctrl+enter", "execute or run the current selection" }
                            };

                            var pressMatch = System.Text.RegularExpressions.Regex.Match(combined, "press\\s+([A-Za-z0-9\\+\\- ]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (pressMatch.Success)
                            {
                                var comboRaw = pressMatch.Groups[1].Value;
                                var norm = System.Text.RegularExpressions.Regex.Replace(comboRaw.ToLowerInvariant(), "[\\s\\-]+", "+");
                                norm = System.Text.RegularExpressions.Regex.Replace(norm, "[^\\w\\+]", "");

                                // For shortcut-only scripts, the spoken command usually carries the real intent.
                                // Example: key(h) with command "next heading" should produce "next heading",
                                // not "press H".
                                if (preferCommandIntent)
                                {
                                    var intent = commandIntent!;
                                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                                    {
                                        intent += $" in {FormatAppForPrompt(application)}";
                                    }
                                    return intent;
                                }

                                // Prefer application-specific mapping first
                                if (appOverrides.TryGetValue(appKey, out var appMap) && appMap.TryGetValue(norm, out var appMapped))
                                {
                                    var seqMappedApp = appMapped;
                                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                                    {
                                        seqMappedApp += $" in {FormatAppForPrompt(application)}";
                                    }
                                    return seqMappedApp;
                                }

                                if (comboMapSeq.TryGetValue(norm, out var seqMappedGlobal))
                                {
                                    var seqMapped = seqMappedGlobal;
                                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                                    {
                                        seqMapped += $" in {FormatAppForPrompt(application)}";
                                    }
                                    return seqMapped;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                            {
                                combined += $" in {application}";
                            }
                            return combined;
                        }
                    }
                    catch
                    {
                        // ignore and continue
                    }
                }
            }
            catch
            {
                // Ignore sequence parsing errors and continue with fallback heuristics
            }

            // key(...) single-pattern fallback
            var keyMatch = System.Text.RegularExpressions.Regex.Match(s, "key\\(\\s*['\"]?([A-Za-z0-9\\-_+ ]+)['\"]?\\s*\\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (keyMatch.Success)
            {
                var keySpec = keyMatch.Groups[1].Value.Trim();
                var normalized = System.Text.RegularExpressions.Regex.Replace(keySpec.ToLowerInvariant(), "[\"'\\s]+", "");
                var parts = System.Text.RegularExpressions.Regex.Split(normalized, "[+\\s-]+");
                var mainKey = parts.LastOrDefault() ?? string.Empty;
                var modifiers = parts.Take(parts.Length - 1).ToArray();
                var combo = (modifiers.Length > 0 ? string.Join("+", modifiers) + "+" + mainKey : mainKey).ToLowerInvariant();
                var commandIntent = RephraseCommandForPrompt(command);

                if (ShouldPreferCommandIntentForShortcut(command))
                {
                    var intent = commandIntent!;
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                    {
                        intent += $" in {FormatAppForPrompt(application)}";
                    }
                    return intent;
                }

                var comboMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "ctrl+s", "save the file" },
                    { "control+s", "save the file" },
                    { "cmd+s", "save the file" },
                    { "meta+s", "save the file" },

                    { "ctrl+a", "select all text" },
                    { "control+a", "select all text" },
                    { "cmd+a", "select all text" },
                    { "meta+a", "select all text" },

                    { "ctrl+f", "open the find dialog" },
                    { "ctrl+t", "open a new tab" },
                    { "ctrl+n", "create a new file or item" },
                    { "control+n", "create a new file or item" },
                    { "cmd+n", "create a new file or item" },
                    { "ctrl+o", "open a file" },
                    { "control+o", "open a file" },
                    { "cmd+o", "open a file" },
                    { "ctrl+p", "open quick open" },
                    { "ctrl+shift+p", "open the command palette" },
                    { "ctrl+shift+n", "open a new window" },
                    { "ctrl+shift+f", "search across files" },
                    { "ctrl+shift+s", "save the file as" },

                    { "ctrl+w", "close the current tab" },
                    { "alt+f4", "close the window" },
                    { "ctrl+shift+i", "show the developer tools" },

                    { "ctrl+c", "copy the selection" },
                    { "ctrl+v", "paste from the clipboard" },
                    { "ctrl+x", "cut the selection" },
                    { "ctrl+z", "undo the last action" },
                    { "ctrl+y", "redo the last action" },

                    { "ctrl+tab", "switch to the next tab" },
                    { "ctrl+shift+tab", "switch to the previous tab" },
                    { "ctrl+enter", "execute or run the current selection" }
                };

                // Prefer application-specific mapping when available
                if (appOverrides.TryGetValue(appKey, out var appMap) && appMap.TryGetValue(combo, out var appMapped))
                {
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                        return appMapped + $" in {FormatAppForPrompt(application)}";
                    return appMapped;
                }

                if (comboMap.TryGetValue(combo, out var mapped))
                {
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                        return mapped + $" in {FormatAppForPrompt(application)}";
                    return mapped;
                }

                var bareKeyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "tab", "move focus to the next element" },
                    { "enter", "insert a new line or confirm" },
                    { "newline", "move the cursor to the next line" },
                    { "space", "insert a space" },
                    { "escape", "cancel the current operation" },
                    { "backspace", "delete the previous character" }
                };

                if (modifiers.Length == 0 && bareKeyMap.TryGetValue(mainKey, out var bareMapped))
                    return bareMapped;

                // Ambiguous bare single keys are not useful as verbal quiz prompts
                if (modifiers.Length == 0 && mainKey.Length <= 2)
                    return null;

                // Avoid storing vague 'bound to key' descriptions; prefer null so import skips these rows
                return null;
            }

            // insert("text") -> type "text"
            var insertMatch = System.Text.RegularExpressions.Regex.Match(s, "insert\\(\\s*['\"](.+?)['\"]\\s*\\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (insertMatch.Success)
            {
                var text = insertMatch.Groups[1].Value;
                return $"type \"{text}\"";
            }

            // mouse_click or mouse.click with numeric arg -> map 1/2/3 to left/middle/right
            var mouseClickNumMatch = System.Text.RegularExpressions.Regex.Match(s, "mouse[_\\.]click\\s*\\(?\\s*['\"]?(?<num>\\d+)['\"]?\\s*\\)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mouseClickNumMatch.Success)
            {
                var num = mouseClickNumMatch.Groups["num"].Value;
                // Talon uses 0/1/2 for left/right/middle in practice
                if (num == "0" || num == "1" || num == "2")
                {
                    var btn = num == "0" ? "left mouse button" : num == "1" ? "right mouse button" : "middle mouse button";
                    return $"click the {btn}";
                }
                return $"click the mouse button {num}";
            }

            // mouse patterns
            if (s.IndexOf("mouse.left", StringComparison.OrdinalIgnoreCase) >= 0)
                return "click the left mouse button";
            if (s.IndexOf("mouse.right", StringComparison.OrdinalIgnoreCase) >= 0)
                return "click the right mouse button";

            // function-like script starting with a dotted or underscored name followed by args
            var funcMatch = System.Text.RegularExpressions.Regex.Match(s, "^([\\w\\.]+)\\s+(.+)$");
            if (funcMatch.Success)
            {
                var name = funcMatch.Groups[1].Value;
                var args = funcMatch.Groups[2].Value.Trim();

                var shortName = name.Contains('.') ? name.Split('.').Last() : name;
                var nameWords = shortName.Replace('_', ' ').Trim();
                // If the function ends with ' change' put 'change' first
                if (nameWords.EndsWith(" change", StringComparison.OrdinalIgnoreCase))
                {
                    nameWords = "change " + nameWords.Substring(0, nameWords.Length - " change".Length).Trim();
                }

                // Handle simple mouse click numeric args: e.g. "mouse_click 2" -> "click the middle mouse button"
                    try
                    {
                        var mouseNumberMatch = System.Text.RegularExpressions.Regex.Match(args, "\\b([0-2])\\b");
                        if (mouseNumberMatch.Success && shortName.IndexOf("mouse", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var num = mouseNumberMatch.Groups[1].Value;
                            var btn = num == "0" ? "left mouse button" : num == "1" ? "right mouse button" : "middle mouse button";
                            return $"click the {btn}";
                        }
                    }
                catch
                {
                    // ignore and continue to other parsing
                }

                var argsMatch = System.Text.RegularExpressions.Regex.Match(args, "^['\"]?([^\\'\",]+)['\"]?\\s*,\\s*([0-9]+)");
                if (argsMatch.Success)
                {
                    var param = argsMatch.Groups[1].Value.Trim();
                    var val = argsMatch.Groups[2].Value.Trim();
                    return $"{nameWords}: set \"{param}\" to {val}";
                }

                // Short fallback showing the function intent and a truncated arg preview
                var preview = args.Length > 120 ? args.Substring(0, 120) + "..." : args;
                return $"{nameWords}: {preview}";
            }

            // Fallback: provide a neutral action description with truncated script
            var cleaned = System.Text.RegularExpressions.Regex.Replace(s, "[{}<>();]", " ");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, "\\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(cleaned)) return null;

            // If the cleaned fallback still looks like a code/command token (e.g., contains 'user.' or dot-separated ids),
            // prefer rephrasing from the `command` name so prompts come from the meaningful command text.
            if (IsAmbiguousDescription(cleaned, script, command))
            {
                var alt = RephraseCommandForPrompt(command);
                if (!string.IsNullOrWhiteSpace(alt))
                {
                    if (!string.IsNullOrWhiteSpace(application) && !application.Equals("global", StringComparison.OrdinalIgnoreCase))
                        alt = alt + " in " + application;
                    return alt.Length > 200 ? alt.Substring(0, 200) + "..." : alt;
                }
            }

            return cleaned.Length > 200 ? cleaned.Substring(0, 200) + "..." : cleaned;
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
        /// Computes a stable merge key for a command + application pair.
        /// This is used to match existing rows to newly-imported rows across refreshes.
        /// </summary>
        private static string ComputeMergeKey(string? command, string? application)
        {
            var cmd = (command ?? string.Empty).Trim();
            // remove leading anchors and trailing anchors
            cmd = System.Text.RegularExpressions.Regex.Replace(cmd, "^[\\^]+|[\\$]+$", "");
            // collapse whitespace
            cmd = System.Text.RegularExpressions.Regex.Replace(cmd, "\\s+", " ");
            cmd = cmd.Trim().Trim('"', '\'');
            cmd = cmd.ToLowerInvariant();

            var app = string.IsNullOrWhiteSpace(application) ? "global" : application.Trim();
            app = System.Text.RegularExpressions.Regex.Replace(app, "\\s+", " ");
            app = app.ToLowerInvariant();

            return cmd + "|" + app;
        }

        /// <summary>
        /// Reads existing commands and builds a map from merge-key -> Description
        /// Only includes non-empty descriptions and uses AsNoTracking to reduce tracking overhead.
        /// </summary>
        private async Task<Dictionary<string, string>> GetExistingDescriptionMapAsync()
        {
            var rows = await _context.TalonVoiceCommands
                .AsNoTracking()
                .Where(c => !string.IsNullOrWhiteSpace(c.Command) && !string.IsNullOrWhiteSpace(c.Description))
                .Select(c => new { c.Command, c.Application, c.Description })
                .ToListAsync();

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows)
            {
                var key = ComputeMergeKey(r.Command, r.Application);
                if (!dict.ContainsKey(key) && !string.IsNullOrWhiteSpace(r.Description))
                {
                    dict[key] = r.Description!;
                }
            }

            return dict;
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

        public async Task<List<TalonVoiceCommand>> GetRandomCommandsAsync(int count, string os)
        {
            var query = _context.TalonVoiceCommands.AsQueryable();

            if (!string.IsNullOrEmpty(os))
            {
                // Filter by OS: include commands where OS is null (all OS) or matches the requested OS
                query = query.Where(c => c.OperatingSystem == null || c.OperatingSystem == "" || c.OperatingSystem.Contains(os));
            }

            // Randomize on the client side because Guid.NewGuid() cannot be translated by EF Core
            var list = await query.ToListAsync();
            return list.OrderBy(c => Guid.NewGuid()).Take(count).ToList();
        }
    }
}
