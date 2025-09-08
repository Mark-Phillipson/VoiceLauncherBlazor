using VoiceLauncherWasm.Models;
using VoiceLauncherWasm.Repositories;

namespace VoiceLauncherWasm.Services
{
    public class TalonListService
    {
        private readonly ITalonListRepository _repository;

        public TalonListService(ITalonListRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TalonList>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<TalonList>> GetByListNameAsync(string listName)
        {
            return await _repository.GetByListNameAsync(listName);
        }

        public async Task<IEnumerable<TalonList>> SearchAsync(string searchTerm)
        {
            return await _repository.SearchAsync(searchTerm);
        }

        public async Task<int> ImportFromTalonListsFileAsync(string fileContent)
        {
            var talonLists = ParseTalonListsContent(fileContent);
            return await _repository.ImportListsAsync(talonLists);
        }

        public async Task<int> ClearAllListsAsync()
        {
            await _repository.ClearAllAsync();
            return 0;
        }

        public async Task<int> GetCountAsync()
        {
            return await _repository.GetCountAsync();
        }

        private List<TalonList> ParseTalonListsContent(string fileContent)
        {
            var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
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
                            ListValue = listValue,
                            CreatedAt = DateTime.UtcNow,
                            ImportedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            return talonLists;
        }

        public async Task<string> ExpandListsInScriptAsync(string script)
        {
            if (string.IsNullOrEmpty(script))
                return script;

            var expandedScript = script;

            // Pattern 1: Handle {list_name} references
            if (script.Contains("{"))
            {
                var curlyBracePattern = @"\{([^}]+)\}";
                var curlyMatches = System.Text.RegularExpressions.Regex.Matches(script, curlyBracePattern);

                foreach (System.Text.RegularExpressions.Match match in curlyMatches)
                {
                    var listReference = match.Groups[1].Value; // e.g., "user.git_argument"
                    var expandedList = await GetExpandedListString(listReference);
                    expandedScript = expandedScript.Replace(match.Value, expandedList);
                }
            }

            return expandedScript;
        }

        private async Task<string> GetExpandedListString(string listReference)
        {
            // Try to find the list with exact name match first
            var listValues = await GetByListNameAsync(listReference);

            // If not found and the reference doesn't start with "user.", try adding "user." prefix
            if (!listValues.Any() && !listReference.StartsWith("user."))
            {
                listValues = await GetByListNameAsync($"user.{listReference}");
            }

            if (listValues.Any())
            {
                // Show first few values with an indication if there are more
                var displayValues = listValues.Take(5).Select(l => l.SpokenForm).ToList();
                return displayValues.Count < listValues.Count()
                    ? $"[{string.Join(" | ", displayValues)} | ... and {listValues.Count() - displayValues.Count} more]"
                    : $"[{string.Join(" | ", displayValues)}]";
            }
            else
            {
                // If no list found, indicate this in the expansion
                return $"[{listReference} - list not found]";
            }
        }
    }
}