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
using System.Text.RegularExpressions;
using RazorClassLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public enum SearchScope
    {
        CommandNamesOnly,
        Script,
        All
    }

    public partial class TalonVoiceCommandSearch : ComponentBase, IDisposable
    {
        private ElementReference searchInput;
        
        [Parameter] public string InitialSearchTerm { get; set; } = string.Empty;
        [Parameter] public bool IsBlazorHybrid { get; set; } = false; // Used to detect if running in Blazor Hybrid mode
        
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }
        public bool UseSemanticMatching { get; set; } = false;
        public SearchScope SelectedSearchScope { get; set; } = SearchScope.CommandNamesOnly; // Default to command names only
        
        // Filter properties
        public string SelectedApplication { get; set; } = string.Empty;
        public string SelectedMode { get; set; } = string.Empty;
        public string SelectedOperatingSystem { get; set; } = string.Empty;
        public string SelectedRepository { get; set; } = string.Empty;
        public string SelectedTags { get; set; } = string.Empty;
        public string SelectedTitle { get; set; } = string.Empty;
        public string SelectedCodeLanguage { get; set; } = string.Empty;
        public List<string> AvailableApplications { get; set; } = new();
        public List<string> AvailableModes { get; set; } = new();
        public List<string> AvailableOperatingSystems { get; set; } = new();
        public List<string> AvailableRepositories { get; set; } = new();
        public List<string> AvailableTags { get; set; } = new();
        public List<string> AvailableTitles { get; set; } = new();
        public List<string> AvailableCodeLanguages { get; set; } = new();
        private int maxResults = 100;

        [Inject]
        public DataAccessLibrary.Services.TalonVoiceCommandDataService? TalonService { get; set; }        [Inject]
        public IJSRuntime? JSRuntime { get; set; }
        [Inject]
        public IWindowsService? WindowsService { get; set; }

        public string CurrentApplication { get; set; } = string.Empty;
        
        private List<TalonVoiceCommand>? _allCommandsCache;
        private bool _isLoadingFilters = false;        private CancellationTokenSource? _searchCancellationTokenSource;
        private Timer? _refreshTimer;

        public bool AutoFilterByCurrentApp { get; set; } = true;
        /// <summary>
        /// Toggles displaying full card body or just header
        /// </summary>
        public bool ShowFullCards { get; set; } = false;

        private string _lastAutoFilteredAppName = string.Empty;
        private string _lastAutoRefreshedAppName = string.Empty;

        private string? MapProcessToApplication(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName) || AvailableApplications == null) return null;
            var exact = AvailableApplications.FirstOrDefault(a => a.Equals(processName, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            var partial = AvailableApplications.FirstOrDefault(a =>
                a.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                processName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
            return partial;
        }

        // For list display functionality
        private Dictionary<string, List<TalonList>> _listContentsCache = new();
        private HashSet<string> _expandedLists = new();

        // For focused card functionality
        private TalonVoiceCommand? _focusedCommand = null;
        private bool _isFocusMode = false;
        private static bool _staticFiltersLoaded = false;        private static List<string> _staticAvailableApplications = new();
        private static List<string> _staticAvailableModes = new();
        private static List<string> _staticAvailableOperatingSystems = new();
        private static List<string> _staticAvailableRepositories = new();
        private static List<string> _staticAvailableTags = new();
        private static List<string> _staticAvailableTitles = new();
        private static List<string> _staticAvailableCodeLanguages = new();private static readonly object _filterLock = new object();

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
                _staticAvailableTitles.Clear();
            }
        }        /// <summary>
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
                _staticAvailableTitles.Clear();
            }
            
            await LoadFilterOptions();
        }        protected override async Task OnInitializedAsync()
        {
            Results = new List<TalonVoiceCommand>();
            
            // Debug: Log the initial search term
            System.Diagnostics.Debug.WriteLine($"TalonVoiceCommandSearch - InitialSearchTerm: '{InitialSearchTerm}'");
            
            // Always load filter options to ensure fresh data
            await LoadFilterOptions();

            // start auto-refresh every 30 seconds
            StartAutoRefresh();
            System.Diagnostics.Debug.WriteLine("Auto-refresh timer started (30s interval)");

            // initial application name
            CurrentApplication = WindowsService?.GetActiveProcessName() ?? string.Empty;
        }

        private void StartAutoRefresh()
        {
            _refreshTimer = new Timer(async _ =>
            {
                var appName = WindowsService?.GetActiveProcessName() ?? string.Empty;
                // Auto-filter if enabled and changed
                if (AutoFilterByCurrentApp && appName != _lastAutoFilteredAppName)
                {
                    var mapped = MapProcessToApplication(appName);
                    if (!string.IsNullOrEmpty(mapped) && mapped != SelectedApplication)
                    {
                        SelectedApplication = mapped;
                        await InvokeAsync(() => OnSearch());
                    }
                    _lastAutoFilteredAppName = appName;
                }
                // only refresh when the active application has changed since last auto-refresh
                if (AutoFilterByCurrentApp && appName != _lastAutoRefreshedAppName)
                {
                    _lastAutoRefreshedAppName = appName;
                    await InvokeAsync(() => OnSearch());
                }

                // update current application display
                await InvokeAsync(() => { CurrentApplication = appName; StateHasChanged(); });
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }
        protected override async Task OnParametersSetAsync()
        {
            System.Diagnostics.Debug.WriteLine($"OnParametersSetAsync - InitialSearchTerm: '{InitialSearchTerm}'");
            
            // Set the initial search term if provided and not already set
            if (!string.IsNullOrWhiteSpace(InitialSearchTerm) && string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Clean up the search term - remove forward slashes and trim
                SearchTerm = InitialSearchTerm.Replace("/", "").Trim();
                System.Diagnostics.Debug.WriteLine($"SearchTerm set from parameter to: '{SearchTerm}'");
            }
            
            await base.OnParametersSetAsync();
        }

        private async Task LoadFilterOptions()
        {
            if (_isLoadingFilters || TalonService is null) return;
              lock (_filterLock)
            {                if (_staticFiltersLoaded)
                {
                    AvailableApplications = _staticAvailableApplications;
                    AvailableModes = _staticAvailableModes;
                    AvailableOperatingSystems = _staticAvailableOperatingSystems;
                    AvailableRepositories = _staticAvailableRepositories;
                    AvailableTags = _staticAvailableTags;
                    AvailableTitles = _staticAvailableTitles;
                    AvailableCodeLanguages = _staticAvailableCodeLanguages;
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
                    .ToList();                var titles = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Title))
                    .Select(c => c.Title!)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                var codeLanguages = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.CodeLanguage))
                    .SelectMany(c => c.CodeLanguage!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(cl => cl.Trim()))
                    .Distinct()
                    .OrderBy(cl => cl)
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
                    AvailableTitles = _staticAvailableTitles = titles;
                    AvailableCodeLanguages = _staticAvailableCodeLanguages = codeLanguages;
                    _staticFiltersLoaded = true;
                }
                
                StateHasChanged();
            }
            finally
            {
                _isLoadingFilters = false;
            }
        }        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            System.Diagnostics.Debug.WriteLine($"OnAfterRenderAsync - firstRender: {firstRender}, SearchTerm: '{SearchTerm}', HasSearched: {HasSearched}");
            
            if (firstRender)
            {
                await searchInput.FocusAsync();
                
                // If we have a search term from command line, perform the search after the first render
                if (!string.IsNullOrWhiteSpace(SearchTerm) && !HasSearched)
                {
                    System.Diagnostics.Debug.WriteLine($"OnAfterRenderAsync - Performing automatic search for: '{SearchTerm}'");
                    await OnSearch();
                    StateHasChanged(); // Force UI update after search
                }
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
        }        protected async Task OnSearchInputBlur()
        {
            // Trigger search when the search input loses focus
            await OnSearch();
        }        public async Task OnSearch()
        {
            System.Diagnostics.Debug.WriteLine($"OnSearch called - SearchTerm: '{SearchTerm}', Length: {SearchTerm?.Length}");
            
            // Cancel any existing search operation
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            // Clear the list contents cache when performing a new search
            _listContentsCache.Clear();
            _expandedLists.Clear();
            
            // Clear focus mode when performing a new search
            _focusedCommand = null;
            _isFocusMode = false;
              // Don't search if no criteria are specified - check for default filter states
            bool hasSearchTerm = !string.IsNullOrWhiteSpace(SearchTerm);
            bool hasApplicationFilter = !string.IsNullOrWhiteSpace(SelectedApplication);
            bool hasModeFilter = !string.IsNullOrWhiteSpace(SelectedMode);bool hasOSFilter = !string.IsNullOrWhiteSpace(SelectedOperatingSystem);
            bool hasRepositoryFilter = !string.IsNullOrWhiteSpace(SelectedRepository);
            bool hasTagsFilter = !string.IsNullOrWhiteSpace(SelectedTags);
            bool hasTitleFilter = !string.IsNullOrWhiteSpace(SelectedTitle);
            bool hasCodeLanguageFilter = !string.IsNullOrWhiteSpace(SelectedCodeLanguage);
              try
            {
                System.Diagnostics.Debug.WriteLine($"OnSearch - hasSearchTerm: {hasSearchTerm}, hasApplicationFilter: {hasApplicationFilter}, hasModeFilter: {hasModeFilter}");
                
                // Debug logging
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Search conditions - Term: '{SearchTerm}', Length: {SearchTerm?.Length}, UseSemanticMatching: {UseSemanticMatching}, hasSearchTerm: {hasSearchTerm}");
                }
                
                if (!hasSearchTerm && !hasApplicationFilter && !hasModeFilter && !hasOSFilter && !hasRepositoryFilter && !hasTagsFilter && !hasTitleFilter && !hasCodeLanguageFilter)
                {
                    System.Diagnostics.Debug.WriteLine("OnSearch - No search criteria, returning early");
                    Results = new List<TalonVoiceCommand>();
                    HasSearched = false;
                    await EnsureSearchFocus();
                    return;
                }

                System.Diagnostics.Debug.WriteLine("OnSearch - Proceeding with search");
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
                }                if (hasTitleFilter)
                {
                    filteredCommands = filteredCommands.Where(c => c.Title == SelectedTitle);
                }

                if (hasCodeLanguageFilter)
                {
                    filteredCommands = filteredCommands.Where(c => 
                        !string.IsNullOrWhiteSpace(c.CodeLanguage) && 
                        c.CodeLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Any(lang => lang.Trim().Equals(SelectedCodeLanguage, StringComparison.OrdinalIgnoreCase)));
                }
                
                if (UseSemanticMatching && hasSearchTerm && SearchTerm?.Length > 2)
                {
                    // Debug logging
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Using semantic search for term: '{SearchTerm}' with scope: '{SelectedSearchScope}'");                    }
                    
                    // Use appropriate search method based on scope
                    List<TalonVoiceCommand> semanticResults;
                    switch (SelectedSearchScope)
                    {
                        case SearchScope.CommandNamesOnly:
                            semanticResults = await TalonService.SearchCommandNamesOnlyAsync(SearchTerm!);
                            break;
                        case SearchScope.Script:
                            semanticResults = await TalonService.SearchScriptOnlyAsync(SearchTerm!);
                            break;
                        case SearchScope.All:
                        default:
                            semanticResults = await TalonService.SearchAllAsync(SearchTerm!);
                            break;
                    }
                    
                    // Debug logging
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Semantic search returned {semanticResults.Count} results");
                    }
                    
                    // Apply filters to semantic results
                    var finalResults = semanticResults.AsEnumerable();
                    
                    if (hasApplicationFilter)
                    {
                        finalResults = finalResults.Where(c => c.Application == SelectedApplication);
                    }
                    
                    if (hasModeFilter)
                    {
                        finalResults = finalResults.Where(c => c.Mode == SelectedMode);
                    }
                    
                    if (hasOSFilter)
                    {
                        finalResults = finalResults.Where(c => c.OperatingSystem == SelectedOperatingSystem);
                    }
                    
                    if (hasRepositoryFilter)
                    {
                        finalResults = finalResults.Where(c => c.Repository == SelectedRepository);
                    }
                      if (hasTagsFilter)
                    {
                        finalResults = finalResults.Where(c => 
                            !string.IsNullOrWhiteSpace(c.Tags) && 
                            c.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Any(tag => tag.Trim().Equals(SelectedTags, StringComparison.OrdinalIgnoreCase)));                    }
                    
                    if (hasTitleFilter)
                    {
                        finalResults = finalResults.Where(c => c.Title == SelectedTitle);
                    }
                    
                    Results = finalResults.Take(maxResults).ToList();
                    
                    // Debug logging
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] After filters applied: {Results.Count} final results");
                    }
                }
                else
                {
                    // Apply text search on filtered results based on scope
                    if (hasSearchTerm)
                    {
                        // Debug logging
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Using non-semantic search for term: '{SearchTerm}' with scope: '{SelectedSearchScope}'");                        }
                        
                        // Use appropriate search method based on scope
                        List<TalonVoiceCommand> searchResults;
                        switch (SelectedSearchScope)
                        {
                            case SearchScope.CommandNamesOnly:
                                searchResults = await TalonService.SearchCommandNamesOnlyAsync(SearchTerm!);
                                break;
                            case SearchScope.Script:
                                searchResults = await TalonService.SearchScriptOnlyAsync(SearchTerm!);
                                break;
                            case SearchScope.All:
                            default:
                                searchResults = await TalonService.SearchAllAsync(SearchTerm!);
                                break;
                        }
                        
                        // Debug logging
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Non-semantic search returned {searchResults.Count} results");
                        }
                        
                        // Apply filters to search results
                        var finalResults = searchResults.AsEnumerable();
                        
                        if (hasApplicationFilter)
                        {
                            finalResults = finalResults.Where(c => c.Application == SelectedApplication);
                        }
                        
                        if (hasModeFilter)
                        {
                            finalResults = finalResults.Where(c => c.Mode == SelectedMode);
                        }
                        
                        if (hasOSFilter)
                        {
                            finalResults = finalResults.Where(c => c.OperatingSystem == SelectedOperatingSystem);
                        }
                        
                        if (hasRepositoryFilter)
                        {
                            finalResults = finalResults.Where(c => c.Repository == SelectedRepository);
                        }
                          if (hasTagsFilter)
                        {
                            finalResults = finalResults.Where(c => 
                                !string.IsNullOrWhiteSpace(c.Tags) && 
                                c.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Any(tag => tag.Trim().Equals(SelectedTags, StringComparison.OrdinalIgnoreCase)));
                        }
                        
                        if (hasTitleFilter)
                        {
                            finalResults = finalResults.Where(c => c.Title == SelectedTitle);
                        }
                        
                        Results = finalResults.Take(maxResults).ToList();
                        
                        // Debug logging
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] After filters applied: {Results.Count} final results");                        }
                    }
                    else
                    {
                        Results = filteredCommands
                            .OrderBy(c => c.Mode ?? "")
                            .ThenBy(c => c.Application)
                            .ThenBy(c => c.Command)
                            .Take(maxResults)
                            .ToList();
                    }                }
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
        catch (Exception ex)
        {
            // Log the error and show user-friendly message
            Console.WriteLine($"Search error: {ex.Message}");
            Results = new List<TalonVoiceCommand>();
            HasSearched = true;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }        public async Task ClearFilters()
    {
            // Clear the list contents cache when clearing filters
            _listContentsCache.Clear();
            _expandedLists.Clear();
            
            // Clear focus mode when clearing filters
            _focusedCommand = null;
            _isFocusMode = false;            SelectedApplication = string.Empty;
            SelectedMode = string.Empty;
            SelectedOperatingSystem = string.Empty;
            SelectedRepository = string.Empty;
            SelectedTags = string.Empty;
            SelectedTitle = string.Empty;
            SelectedCodeLanguage = string.Empty;
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

        protected void OnTitleFilterChange(ChangeEventArgs e)
        {
            SelectedTitle = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnCodeLanguageFilterChange(ChangeEventArgs e)
        {
            SelectedCodeLanguage = e.Value?.ToString() ?? string.Empty;
            // Don't auto-search, let user control when to search
        }

        protected void OnSemanticToggleChange(ChangeEventArgs e)
        {
            UseSemanticMatching = e.Value != null && (bool)e.Value;
            // Don't auto-search, let user control when to search
        }

        protected void OnSearchScopeChange(ChangeEventArgs e)
        {
            if (Enum.TryParse<SearchScope>(e.Value?.ToString(), out var scope))
            {
                SelectedSearchScope = scope;
                // Update placeholder text based on scope
                StateHasChanged();
            }
        }

        private string GetSearchPlaceholder()
        {
            return SelectedSearchScope switch
            {
                SearchScope.CommandNamesOnly => "Search names only... (Alt+S)",
                SearchScope.Script => "Search script content... (Alt+S)",
                SearchScope.All => "Search all (commands, scripts, lists)... (Alt+S)",
                _ => "Search commands... (Alt+S)"
            };
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
        }        public List<string> GetListsUsedInScript(string script)
        {
            // This method name is misleading - we should actually parse the voice command, not the script
            // But keeping the same name to avoid breaking changes
            return new List<string>();
        }        public List<string> GetListsUsedInCommand(string command)
        {
            var lists = new List<string>();
            if (string.IsNullOrEmpty(command))
                return lists;

            Console.WriteLine($"[DEBUG] Analyzing voice command for lists: '{command}'");

            // NOTE: In Talon syntax:
            // <capture> = captures (functions that parse input) - NOT lists
            // {list} = actual list references - THESE are what we want
            // [optional] = optional elements

            // Pattern 1: {list_name} references in voice commands (actual lists)
            var curlyBracePattern = @"\{([a-zA-Z_][a-zA-Z0-9_.]*)\}";
            var curlyMatches = Regex.Matches(command, curlyBracePattern);
            Console.WriteLine($"[DEBUG] Found {curlyMatches.Count} {{list}} matches in command");
            foreach (Match match in curlyMatches)
            {
                var listName = match.Groups[1].Value;
                Console.WriteLine($"[DEBUG] Found list reference: '{{{listName}}}'");
                lists.Add(listName);
            }

            // Pattern 2: [optional] sections that may contain {list} references
            var squareBracePattern = @"\[([^\]]+)\]";
            var squareMatches = Regex.Matches(command, squareBracePattern);
            Console.WriteLine($"[DEBUG] Found {squareMatches.Count} [optional] sections in command");
            foreach (Match match in squareMatches)
            {
                var content = match.Groups[1].Value;
                Console.WriteLine($"[DEBUG] Checking optional section: '[{content}]'");
                
                // Only look for {list} references inside optional sections, ignore <captures>
                var innerCurlyMatches = Regex.Matches(content, @"\{([a-zA-Z_][a-zA-Z0-9_.]*)\}");
                
                foreach (Match innerMatch in innerCurlyMatches)
                {
                    var listName = innerMatch.Groups[1].Value;
                    Console.WriteLine($"[DEBUG] Found optional list reference: '[{{{listName}}}]'");
                    lists.Add(listName);                }
            }

            var finalLists = lists.Distinct().ToList();
            Console.WriteLine($"[DEBUG] Final lists found in command: {string.Join(", ", finalLists)}");
            Console.WriteLine($"[DEBUG] Note: Captures like <user.modelPrompt> are ignored as they are not lists");
            return finalLists;
        }

        public List<string> GetCapturesUsedInCommand(string command)
        {
            var captures = new List<string>();
            if (string.IsNullOrEmpty(command))
                return captures;

            Console.WriteLine($"[DEBUG] Analyzing voice command for captures: '{command}'");

            // Pattern 1: <capture_name> references in voice commands
            var angleBracePattern = @"<([a-zA-Z_][a-zA-Z0-9_.]+)>";
            var angleMatches = Regex.Matches(command, angleBracePattern);
            Console.WriteLine($"[DEBUG] Found {angleMatches.Count} <capture> matches in command");
            foreach (Match match in angleMatches)
            {
                var captureName = match.Groups[1].Value;
                Console.WriteLine($"[DEBUG] Found capture: '<{captureName}>'");
                captures.Add(captureName);
            }

            // Pattern 2: [optional] sections that may contain <capture> references
            var squareBracePattern = @"\[([^\]]+)\]";
            var squareMatches = Regex.Matches(command, squareBracePattern);
            Console.WriteLine($"[DEBUG] Found {squareMatches.Count} [optional] sections in command");
            foreach (Match match in squareMatches)
            {
                var content = match.Groups[1].Value;
                Console.WriteLine($"[DEBUG] Checking optional section for captures: '[{content}]'");
                
                // Look for <capture> references inside optional sections
                var innerAngleMatches = Regex.Matches(content, @"<([a-zA-Z_][a-zA-Z0-9_.]+)>");
                
                foreach (Match innerMatch in innerAngleMatches)
                {
                    var captureName = innerMatch.Groups[1].Value;
                    Console.WriteLine($"[DEBUG] Found optional capture: '[<{captureName}>]'");
                    captures.Add(captureName);
                }
            }

            var finalCaptures = captures.Distinct().ToList();
            Console.WriteLine($"[DEBUG] Final captures found in command: {string.Join(", ", finalCaptures)}");
            return finalCaptures;
        }

        private bool IsLikelyListName(string word)
        {
            // Common Talon list patterns
            var commonListNames = new[] { "model", "application", "browser", "editor", "terminal", "language", "framework" };
            return commonListNames.Contains(word.ToLower());
        }

        public async Task ToggleListDisplay(string listName)
        {
            if (_expandedLists.Contains(listName))
            {
                _expandedLists.Remove(listName);
            }
            else
            {
                _expandedLists.Add(listName);
                
                // Load list contents if not already cached
                if (!_listContentsCache.ContainsKey(listName) && TalonService != null)
                {
                    try
                    {
                        var listContents = await TalonService.GetListContentsAsync(listName);
                        _listContentsCache[listName] = listContents;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading list contents for {listName}: {ex.Message}");
                        _listContentsCache[listName] = new List<TalonList>();
                    }
                }
            }
            StateHasChanged();
        }

        public bool IsListExpanded(string listName)
        {
            return _expandedLists.Contains(listName);
        }

        public List<TalonList> GetListContents(string listName)
        {
            return _listContentsCache.TryGetValue(listName, out var contents) ? contents : new List<TalonList>();
        }        public bool CommandContainsLists(string command)
        {
            return GetListsUsedInCommand(command).Any();
        }

        public bool CommandContainsCaptures(string command)
        {
            return GetCapturesUsedInCommand(command).Any();
        }

        public bool CommandContainsListsOrCaptures(string command)
        {
            return GetListsUsedInCommand(command).Any() || GetCapturesUsedInCommand(command).Any();
        }

        // Keep this for backward compatibility but redirect to command-based check
        public bool ScriptContainsLists(string script)
        {
            return false; // Scripts don't contain list references, commands do
        }        /// <summary>
        /// Highlights captures in a command string by wrapping them with HTML span tags
        /// </summary>
        public string HighlightCapturesInCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return command;

            // Pattern to match captures like <user.text>, <user.model>, etc.
            var pattern = @"<([a-zA-Z_][a-zA-Z0-9_.]+)>";
            var highlightedCommand = Regex.Replace(command, pattern, 
                "<span class=\"capture-highlight\">&lt;$1&gt;</span>");

            return highlightedCommand;
        }

        /// <summary>
        /// Sets focus mode to show only the selected command card
        /// </summary>
        public async Task FocusOnCommand(TalonVoiceCommand command)
        {
            AutoFilterByCurrentApp= false; // Disable auto-filtering when focusing on a command
            _focusedCommand = command;
            _isFocusMode = true;
            StateHasChanged();
           // scroll focused card into view
           if (JSRuntime != null)
           {
               await JSRuntime.InvokeVoidAsync("scrollFocusedIntoView");
           }
        }

        /// <summary>
        /// Exits focus mode and shows all results again
        /// </summary>
        public void ExitFocusMode()
        {
            _focusedCommand = null;
            _isFocusMode = false;
            StateHasChanged();
        }

        /// <summary>
        /// Gets the commands to display based on focus mode
        /// </summary>
        public List<TalonVoiceCommand> GetDisplayedCommands()
        {
            if (_isFocusMode && _focusedCommand != null)
            {
                return new List<TalonVoiceCommand> { _focusedCommand };
            }
            return Results ?? new List<TalonVoiceCommand>();
        }

        /// <summary>
        /// Checks if the component is in focus mode
        /// </summary>
        public bool IsInFocusMode()
        {
            return _isFocusMode;
        }

        public void Dispose()
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _refreshTimer?.Dispose();
        }

        /// <summary>
        /// Manually clear the list contents cache - useful for debugging list display issues
        /// </summary>
        public void ClearListCache()
        {
            _listContentsCache.Clear();
            _expandedLists.Clear();
            StateHasChanged();
        }

        public async Task OnCaptureClick(string captureName)
        {
            var question = $"What values are available for the Talon capture <{captureName}>?";
            if (JSRuntime != null && _focusedCommand != null && !string.IsNullOrWhiteSpace(_focusedCommand.FilePath))
            {
                // Copy question to clipboard
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", question);

                // Open the script file in VS Code
                var filePath = _focusedCommand.FilePath.Replace("\\", "/");
                var uri = $"vscode://file/{filePath}";
                await JSRuntime.InvokeVoidAsync("window.open", uri, "_blank");
            }
        }
    }
}
