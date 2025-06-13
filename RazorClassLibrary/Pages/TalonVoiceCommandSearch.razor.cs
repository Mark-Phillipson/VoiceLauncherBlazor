using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DataAccessLibrary.Models;
using SmartComponents.LocalEmbeddings;
using System.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace RazorClassLibrary.Pages
{
    public partial class TalonVoiceCommandSearch : ComponentBase
    {
        private ElementReference searchInput;
        
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }
        public bool UseSemanticMatching { get; set; } = true;
          // Filter properties
        public string SelectedApplication { get; set; } = string.Empty;
        public string SelectedMode { get; set; } = string.Empty;
        public string SelectedOperatingSystem { get; set; } = string.Empty;
        public List<string> AvailableApplications { get; set; } = new();
        public List<string> AvailableModes { get; set; } = new();
        public List<string> AvailableOperatingSystems { get; set; } = new();
        
        private int maxResults = 20;

        [Inject]
        public DataAccessLibrary.Services.TalonVoiceCommandDataService? TalonService { get; set; }        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        private List<TalonVoiceCommand>? _allCommandsCache;
        private bool _isLoadingFilters = false;

        protected override async Task OnInitializedAsync()
        {
            Results = new List<TalonVoiceCommand>();
            await LoadFilterOptions();
        }        private async Task LoadFilterOptions()
        {
            if (_isLoadingFilters || TalonService is null) return;
            
            _isLoadingFilters = true;
            
            try
            {
                // Cache all commands to avoid multiple database calls
                _allCommandsCache = await TalonService.GetAllCommandsForFiltersAsync();
                
                Console.WriteLine($"Total commands loaded: {_allCommandsCache.Count}");
                
                AvailableApplications = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Application))
                    .Select(c => c.Application!)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                Console.WriteLine($"Available Applications: {string.Join(", ", AvailableApplications)}");

                var modesWithValues = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Mode))
                    .ToList();
                    
                Console.WriteLine($"Commands with non-null modes: {modesWithValues.Count}");
                foreach (var cmd in modesWithValues.Take(5))
                {
                    Console.WriteLine($"Mode example: '{cmd.Mode}' for command: '{cmd.Command}'");
                }                AvailableModes = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Mode))
                    .Select(c => c.Mode!)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                Console.WriteLine($"Available Modes: {string.Join(", ", AvailableModes)}");

                AvailableOperatingSystems = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.OperatingSystem))
                    .Select(c => c.OperatingSystem!)
                    .Distinct()
                    .OrderBy(os => os)
                    .ToList();

                Console.WriteLine($"Available Operating Systems: {string.Join(", ", AvailableOperatingSystems)}");
                
                StateHasChanged();
            }
            finally
            {
                _isLoadingFilters = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await searchInput.FocusAsync();
            }
        }        protected async Task OnSearch()
        {
            IsLoading = true;
            HasSearched = true;
            StateHasChanged();
            
            if (TalonService is not null)
            {
                // Use cached data if available, otherwise load it
                var allCommands = _allCommandsCache ?? await TalonService.GetAllCommandsForFiltersAsync();
                
                // Apply filters first
                var filteredCommands = allCommands.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SelectedApplication))
                {
                    filteredCommands = filteredCommands.Where(c => c.Application == SelectedApplication);
                }
                  if (!string.IsNullOrWhiteSpace(SelectedMode))
                {
                    filteredCommands = filteredCommands.Where(c => c.Mode == SelectedMode);
                }
                
                if (!string.IsNullOrWhiteSpace(SelectedOperatingSystem))
                {
                    filteredCommands = filteredCommands.Where(c => c.OperatingSystem == SelectedOperatingSystem);
                }
                
                if (UseSemanticMatching && !string.IsNullOrWhiteSpace(SearchTerm) && SearchTerm.Length > 2)
                {
                    var candidates = filteredCommands.Select((c, index) => (Item: c, Text: c.Command + " " + c.Script, Index: index)).ToList();
                    using var embedder = new LocalEmbedder();
                    var matchedResults = embedder.EmbedRange(candidates.Select(x => x.Text).ToList());
                    var results = LocalEmbedder.FindClosestWithScore(embedder.Embed(SearchTerm), matchedResults, maxResults: maxResults);
                    
                    // Create a lookup that can handle duplicate keys
                    var scoreLookup = results
                        .GroupBy(r => r.Item)
                        .ToDictionary(g => g.Key, g => g.Max(x => x.Similarity));
                    
                    var resultTexts = results.Select(r => r.Item).ToHashSet();
                      Results = candidates
                        .Where(c => resultTexts.Contains(c.Text))
                        .OrderByDescending(c => scoreLookup.GetValueOrDefault(c.Text, 0))
                        .ThenBy(c => c.Item.Mode ?? "")
                        .ThenBy(c => c.Item.Application)
                        .ThenBy(c => c.Item.Command)
                        .Select(c => c.Item)
                        .ToList();
                }
                else                {
                    // Apply text search on filtered results
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        Results = filteredCommands
                            .Where(c => c.Command.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                       c.Script.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(c => c.Mode ?? "")
                            .ThenBy(c => c.Application)
                            .ThenBy(c => c.Command)
                            .ToList();
                    }
                    else
                    {
                        Results = filteredCommands
                            .OrderBy(c => c.Mode ?? "")
                            .ThenBy(c => c.Application)
                            .ThenBy(c => c.Command)
                            .ToList();
                    }
                }
            }
            else
            {
                Results = new List<TalonVoiceCommand>();
            }
            IsLoading = false;
            StateHasChanged();
        }        public async Task ClearFilters()
        {
            SelectedApplication = string.Empty;
            SelectedMode = string.Empty;
            SelectedOperatingSystem = string.Empty;
            await OnSearch();
        }

        public async Task OpenFileInVSCode(string filePath)
        {
            if (JSRuntime != null && !string.IsNullOrWhiteSpace(filePath))
            {
                // This will attempt to open the file in VS Code using the vscode://file URI scheme
                var uri = $"vscode://file/{filePath.Replace("\\", "/")}";
                await JSRuntime.InvokeVoidAsync("window.open", uri, "_blank");
            }
        }

        public async Task OnFilePathClick(MouseEventArgs e, string filePath)
        {
            // PreventDefault is not available in Blazor, but using @onclick on <a href="#"> avoids navigation
            await OpenFileInVSCode(filePath);
        }        public string GetFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;
            
            return System.IO.Path.GetFileName(filePath);
        }
    }
}
