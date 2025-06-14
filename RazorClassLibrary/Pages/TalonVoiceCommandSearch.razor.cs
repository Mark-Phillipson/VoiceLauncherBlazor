using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DataAccessLibrary.Models;
using SmartComponents.LocalEmbeddings;
using System.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Threading;
using System;

namespace RazorClassLibrary.Pages
{
    public partial class TalonVoiceCommandSearch : ComponentBase, IDisposable
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
        public string SelectedRepository { get; set; } = string.Empty;
        public string SelectedTags { get; set; } = string.Empty;
        public List<string> AvailableApplications { get; set; } = new();
        public List<string> AvailableModes { get; set; } = new();
        public List<string> AvailableOperatingSystems { get; set; } = new();
        public List<string> AvailableRepositories { get; set; } = new();
        public List<string> AvailableTags { get; set; } = new();        
        private int maxResults = 100;

        [Inject]
        public DataAccessLibrary.Services.TalonVoiceCommandDataService? TalonService { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        private List<TalonVoiceCommand>? _allCommandsCache;
        private bool _isLoadingFilters = false;
        private CancellationTokenSource? _searchCancellationTokenSource;

        private static bool _staticFiltersLoaded = false;
        private static List<string> _staticAvailableApplications = new();
        private static List<string> _staticAvailableModes = new();
        private static List<string> _staticAvailableOperatingSystems = new();
        private static List<string> _staticAvailableRepositories = new();
        private static List<string> _staticAvailableTags = new();
        private static readonly object _filterLock = new object();

        /// <summary>
        /// Clears the static filter cache to force reload after data changes
        /// </summary>
        public static void InvalidateFilterCache()
        {
            lock (_filterLock)
            {
                _staticFiltersLoaded = false;
                _staticAvailableApplications.Clear();
                _staticAvailableModes.Clear();
                _staticAvailableOperatingSystems.Clear();
                _staticAvailableRepositories.Clear();
                _staticAvailableTags.Clear();
            }
        }

        /// <summary>
        /// Forces a refresh of filter options from the database
        /// </summary>
        public async Task RefreshFiltersAsync()
        {
            lock (_filterLock)
            {
                _staticFiltersLoaded = false;
                _staticAvailableApplications.Clear();
                _staticAvailableModes.Clear();
                _staticAvailableOperatingSystems.Clear();
                _staticAvailableRepositories.Clear();
                _staticAvailableTags.Clear();
            }
            
            await LoadFilterOptions();
        }

        protected override async Task OnInitializedAsync()
        {
            Results = new List<TalonVoiceCommand>();
            
            // Always load filter options to ensure fresh data
            await LoadFilterOptions();
        }

        private async Task LoadFilterOptions()
        {
            if (_isLoadingFilters || TalonService is null) return;
            
            lock (_filterLock)
            {
                if (_staticFiltersLoaded)
                {
                    AvailableApplications = _staticAvailableApplications;
                    AvailableModes = _staticAvailableModes;
                    AvailableOperatingSystems = _staticAvailableOperatingSystems;
                    AvailableRepositories = _staticAvailableRepositories;
                    AvailableTags = _staticAvailableTags;
                    return;
                }
                
                if (_isLoadingFilters) return;
                _isLoadingFilters = true;
            }
            
            try
            {
                // Cache all commands to avoid multiple database calls
                _allCommandsCache = await TalonService.GetAllCommandsForFiltersAsync();
                
                var applications = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Application))
                    .Select(c => c.Application!)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                var modes = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Mode))
                    .Select(c => c.Mode!)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var operatingSystems = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.OperatingSystem))
                    .Select(c => c.OperatingSystem!)
                    .Distinct()
                    .OrderBy(os => os)
                    .ToList();

                var repositories = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Repository))
                    .Select(c => c.Repository!)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                var tags = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Tags))
                    .SelectMany(c => c.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
                
                // Debug output
                Console.WriteLine($"LoadFilterOptions: Found {repositories.Count} repositories: {string.Join(", ", repositories)}");
                Console.WriteLine($"LoadFilterOptions: Found {tags.Count} tags: {string.Join(", ", tags)}");
                
                // Update both instance and static collections
                lock (_filterLock)
                {
                    AvailableApplications = _staticAvailableApplications = applications;
                    AvailableModes = _staticAvailableModes = modes;
                    AvailableOperatingSystems = _staticAvailableOperatingSystems = operatingSystems;
                    AvailableRepositories = _staticAvailableRepositories = repositories;
                    AvailableTags = _staticAvailableTags = tags;
                    _staticFiltersLoaded = true;
                }
                
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
            // Note: We avoid calling EnsureSearchFocus on every render to prevent 
            // performance issues and potential infinite loops
        }

        protected async Task OnSearchInputKeyUp(KeyboardEventArgs e)
        {
            // Only trigger search on Enter key
            if (e.Key == "Enter")
            {
                await OnSearch();
            }
        }

        protected async Task OnSearchInputBlur()
        {
            // Trigger search when the search input loses focus
            await OnSearch();
        }

        protected async Task OnSearch()
        {
            // Don't search if no criteria are specified - check for default filter states
            bool hasSearchTerm = !string.IsNullOrWhiteSpace(SearchTerm);
            bool hasApplicationFilter = !string.IsNullOrWhiteSpace(SelectedApplication);
            bool hasModeFilter = !string.IsNullOrWhiteSpace(SelectedMode);
            bool hasOSFilter = !string.IsNullOrWhiteSpace(SelectedOperatingSystem);
            bool hasRepositoryFilter = !string.IsNullOrWhiteSpace(SelectedRepository);
            bool hasTagsFilter = !string.IsNullOrWhiteSpace(SelectedTags);
            
            if (!hasSearchTerm && !hasApplicationFilter && !hasModeFilter && !hasOSFilter && !hasRepositoryFilter && !hasTagsFilter)
            {
                Results = new List<TalonVoiceCommand>();
                HasSearched = false;
                await EnsureSearchFocus();
                return;
            }

            IsLoading = true;
            HasSearched = true;
            StateHasChanged();
            
            // Small delay to ensure spinner is visible
            await Task.Delay(100);
            
            if (TalonService is not null)
            {
                // Use cached data if available, otherwise load it
                var allCommands = _allCommandsCache ?? await TalonService.GetAllCommandsForFiltersAsync();
                
                // Apply filters first
                var filteredCommands = allCommands.AsEnumerable();
                
                if (hasApplicationFilter)
                {
                    filteredCommands = filteredCommands.Where(c => c.Application == SelectedApplication);
                }
                
                if (hasModeFilter)
                {
                    filteredCommands = filteredCommands.Where(c => c.Mode == SelectedMode);
                }
                
                if (hasOSFilter)
                {
                    filteredCommands = filteredCommands.Where(c => c.OperatingSystem == SelectedOperatingSystem);
                }
                
                if (hasRepositoryFilter)
                {
                    filteredCommands = filteredCommands.Where(c => c.Repository == SelectedRepository);
                }

                if (hasTagsFilter)
                {
                    filteredCommands = filteredCommands.Where(c => 
                        !string.IsNullOrWhiteSpace(c.Tags) && 
                        c.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Any(tag => tag.Trim().Equals(SelectedTags, StringComparison.OrdinalIgnoreCase)));
                }
                
                if (UseSemanticMatching && hasSearchTerm && SearchTerm.Length > 2)
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
                }                else
                {
                    // Apply text search on filtered results
                    if (hasSearchTerm)
                    {
                        Results = filteredCommands
                            .Where(c => c.Command.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                       c.Script.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(c => c.Mode ?? "")
                            .ThenBy(c => c.Application)
                            .ThenBy(c => c.Command)
                            .Take(maxResults)
                            .ToList();
                    }
                    else
                    {
                        Results = filteredCommands
                            .OrderBy(c => c.Mode ?? "")
                            .ThenBy(c => c.Application)
                            .ThenBy(c => c.Command)
                            .Take(maxResults)
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
            
            // Only restore focus if the search was triggered intentionally
            // (not during debounced typing to avoid interfering with user input)
            if (!string.IsNullOrWhiteSpace(SearchTerm) || hasApplicationFilter || hasModeFilter || hasOSFilter)
            {
                await EnsureSearchFocus();
            }
        }

        public async Task ClearFilters()
        {
            SelectedApplication = string.Empty;
            SelectedMode = string.Empty;
            SelectedOperatingSystem = string.Empty;
            SelectedRepository = string.Empty;
            SelectedTags = string.Empty;
            // Don't automatically search after clearing - let user type in search box
            Results = new List<TalonVoiceCommand>();
            HasSearched = false;
            StateHasChanged();
            
            // Restore focus to search input after clearing
            await EnsureSearchFocus();
        }

        protected void OnApplicationFilterChange(ChangeEventArgs e)
        {
            SelectedApplication = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnModeFilterChange(ChangeEventArgs e)
        {
            SelectedMode = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnOSFilterChange(ChangeEventArgs e)
        {
            SelectedOperatingSystem = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnRepositoryFilterChange(ChangeEventArgs e)
        {
            SelectedRepository = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnTagsFilterChange(ChangeEventArgs e)
        {
            SelectedTags = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnSemanticToggleChange(ChangeEventArgs e)
        {
            UseSemanticMatching = e.Value != null && (bool)e.Value;
            // Don't auto-search, let user control when to search
        }

        private async Task EnsureSearchFocus()
        {
            if (JSRuntime != null)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("setTimeout", 
                        "() => { const searchInput = document.querySelector('input[type=\"text\"]'); if (searchInput) searchInput.focus(); }", 
                        10);
                }
                catch
                {
                    // Fallback to direct focus if JS fails
                    try
                    {
                        await searchInput.FocusAsync();
                    }
                    catch
                    {
                        // Silent fail - focus is best effort
                    }
                }
            }
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
        }

        public string GetFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;
            
            return System.IO.Path.GetFileName(filePath);
        }

        public string GetTrimmedScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return string.Empty;
            
            // Split into lines, remove leading and trailing empty lines
            var lines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            
            // Find first non-empty line
            int firstNonEmptyLine = 0;
            while (firstNonEmptyLine < lines.Length && string.IsNullOrWhiteSpace(lines[firstNonEmptyLine]))
            {
                firstNonEmptyLine++;
            }
            
            // Find last non-empty line
            int lastNonEmptyLine = lines.Length - 1;
            while (lastNonEmptyLine >= 0 && string.IsNullOrWhiteSpace(lines[lastNonEmptyLine]))
            {
                lastNonEmptyLine--;
            }
            
            // If all lines are empty, return empty string
            if (firstNonEmptyLine > lastNonEmptyLine)
                return string.Empty;
            
            // Extract non-empty lines and rejoin
            var trimmedLines = lines.Skip(firstNonEmptyLine).Take(lastNonEmptyLine - firstNonEmptyLine + 1);
            return string.Join(Environment.NewLine, trimmedLines);
        }

        public void Dispose()
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
        }
    }
}
