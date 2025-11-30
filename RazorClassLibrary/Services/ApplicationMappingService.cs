using System;
using System.Collections.Generic;
using System.Linq;

namespace RazorClassLibrary.Services
{
    public class ApplicationMappingService
    {
        private readonly Dictionary<string, List<string>> _mappings = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _reverseMappings = new(StringComparer.OrdinalIgnoreCase);

        public ApplicationMappingService()
        {
            // Initialize with some default mappings
            // Format: Display Name -> List of actual application names in DB/Talon
            AddMapping("Microsoft Edge", "msedge.exe", "Microsoft Edge", "edge", "msedge", "microsoft_edge");
            AddMapping("Google Chrome", "chrome.exe", "Google Chrome", "chrome");
            AddMapping("Visual Studio Code", "code.exe", "Visual Studio Code", "vscode", "code");
            AddMapping("Visual Studio", "devenv.exe", "Visual Studio", "visual studio");
            AddMapping("Firefox", "firefox.exe", "Firefox", "firefox");
            AddMapping("Notepad", "notepad.exe", "Notepad", "notepad");
            AddMapping("Explorer", "explorer.exe", "Windows Explorer", "explorer");
            AddMapping("Terminal", "windowsterminal.exe", "Windows Terminal", "terminal", "pwsh.exe", "powershell.exe", "cmd.exe", "apple_terminal", "windows_terminal");
            AddMapping("Slack", "slack.exe", "Slack", "slack");
            AddMapping("Discord", "discord.exe", "Discord", "discord");
            AddMapping("Microsoft Teams", "teams.exe", "Microsoft Teams", "teams", "msteams", "com.microsoft.teams", "microsoft_teams");
            AddMapping("Word", "winword.exe", "Microsoft Word", "word");
            AddMapping("Excel", "excel.exe", "Microsoft Excel", "excel");
            AddMapping("PowerPoint", "powerpnt.exe", "Microsoft PowerPoint", "powerpoint");
            AddMapping("Outlook", "outlook.exe", "Microsoft Outlook", "outlook");
        }

        private void AddMapping(string displayName, params string[] aliases)
        {
            if (!_mappings.ContainsKey(displayName))
            {
                _mappings[displayName] = new List<string>();
            }
            
            foreach (var alias in aliases)
            {
                if (!_mappings[displayName].Contains(alias, StringComparer.OrdinalIgnoreCase))
                {
                    _mappings[displayName].Add(alias);
                }
                
                // Reverse mapping for quick lookup
                _reverseMappings[alias] = displayName;
            }
            
            // Ensure the display name itself is in the list if it appears in the DB
            if (!_mappings[displayName].Contains(displayName, StringComparer.OrdinalIgnoreCase))
            {
                _mappings[displayName].Add(displayName);
            }
             _reverseMappings[displayName] = displayName;
        }

        public string GetDisplayName(string appName)
        {
            if (string.IsNullOrWhiteSpace(appName)) return appName;
            
            var trimmed = appName.Trim();
            
            // Try exact match
            if (_reverseMappings.TryGetValue(trimmed, out var displayName))
            {
                return displayName;
            }

            // Try filename match (e.g. if appName is a full path like "C:\Program Files\Edge\msedge.exe")
            try 
            {
                var fileName = System.IO.Path.GetFileName(trimmed);
                if (!string.Equals(fileName, trimmed, StringComparison.OrdinalIgnoreCase) && 
                    _reverseMappings.TryGetValue(fileName, out var displayNameFromFileName))
                {
                    return displayNameFromFileName;
                }
            }
            catch { /* ignore invalid paths */ }
            
            return trimmed;
        }

        public List<string> GetAliases(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return new List<string>();

            if (_mappings.TryGetValue(displayName, out var aliases))
            {
                return aliases;
            }

            return new List<string> { displayName };
        }
        
        public List<string> GetNormalizedList(IEnumerable<string> rawAppNames)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var app in rawAppNames)
            {
                result.Add(GetDisplayName(app));
            }
            
            return result.OrderBy(x => x).ToList();
        }
    }
}
