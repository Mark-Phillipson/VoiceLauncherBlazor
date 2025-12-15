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
using RazorClassLibrary.Models;
using RazorClassLibrary.Components;

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
    // Shared modal state (used to populate the reusable modal)
        public List<SelectionItem> SelectionModalItems { get; set; } = new();
        public string SelectionModalTitle { get; set; } = "Select";
        private string _openFilterTarget = string.Empty;

        // Collapsible panel state
        public bool RepositoryPanelExpanded { get; set; } = false;
        public bool FilterPanelExpanded { get; set; } = false;
        public void ToggleRepositoryPanel() => RepositoryPanelExpanded = !RepositoryPanelExpanded;
        public void ToggleFilterPanel() => FilterPanelExpanded = !FilterPanelExpanded;

    protected SelectionModal? _selectionModal;
    private IJSObjectReference? _selectionModule;
    private bool _selectionModuleLoaded = false;

    // Helper to convert string lists to SelectionItem
        private List<SelectionItem> ToSelectionItems(List<string>? items, string? defaultColor = "")
        {
            if (items == null) return new List<SelectionItem>();
            // If this is for applications, show only the filename in the label
            if (object.ReferenceEquals(items, AvailableApplications))
            {
                return items.Select(i => new SelectionItem { Id = i, Label = GetFileName(i), ColorClass = defaultColor }).ToList();
            }
            return items.Select(i => new SelectionItem { Id = i, Label = i, ColorClass = defaultColor }).ToList();
        }

        // Show modal handlers for each filter
        public async Task ShowApplicationModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableApplications, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Applications" });
            SelectionModalTitle = "Select Application";
            _openFilterTarget = "application";
            // Ensure the component receives new parameters
            await InvokeAsync(StateHasChanged);
            if (_selectionModal != null)
            {
                await _selectionModal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowModeModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableModes, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Modes" });
            SelectionModalTitle = "Select Mode";
            _openFilterTarget = "mode";
            await InvokeAsync(StateHasChanged);
            if (_selectionModal != null)
            {
                    await _selectionModal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowTagsModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableTags, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Tags" });
            SelectionModalTitle = "Select Tag";
            _openFilterTarget = "tags";
            await InvokeAsync(StateHasChanged);
            var modal = _selectionModal;
            if (modal != null)
            {
                await modal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowOSModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableOperatingSystems, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Operating Systems" });
            SelectionModalTitle = "Select Operating System";
            _openFilterTarget = "os";
            await InvokeAsync(StateHasChanged);
            var modal = _selectionModal;
            if (modal != null)
            {
                await modal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowRepositoryModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableRepositories, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Repositories" });
            SelectionModalTitle = "Select Repository";
            _openFilterTarget = "repository";
            await InvokeAsync(StateHasChanged);
            var modal = _selectionModal;
            if (modal != null)
            {
                await modal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowTitleModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableTitles, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Titles" });
            SelectionModalTitle = "Select Title";
            _openFilterTarget = "title";
            await InvokeAsync(StateHasChanged);
            var modal = _selectionModal;
            if (modal != null)
            {
                await modal.ShowAsync();
                return;
            }
            if (_selectionModule != null)
            {
                await _selectionModule.InvokeVoidAsync("showModal", "#selectionModal");
            }
        }

        public async Task ShowCodeLanguageModal()
        {
            SelectionModalItems = ToSelectionItems(AvailableCodeLanguages, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Code Languages" });
            SelectionModalTitle = "Select Code Language";
            _openFilterTarget = "codelanguage";
            await InvokeAsync(StateHasChanged);
            var modal = _selectionModal;
            if (modal != null)
            {
                await modal.ShowAsync();
                return;
            }
            await ShowSelectionModalAsync("#selectionModal");
        }

        // Helper that invokes the JS module if loaded, or falls back to the global bootstrapInterop
        private async Task ShowSelectionModalAsync(string selector)
        {
            try
            {
                // Prefer the SelectionModal component reference so Blazor applies
                // the latest parameter values before any JS interaction.
                var modal = _selectionModal;
                if (modal != null)
                {
                    await modal.ShowAsync();
                    return;
                }
                if (_selectionModule != null && _selectionModuleLoaded)
                {
                    await _selectionModule.InvokeVoidAsync("showModal", selector);
                }
                else if (JSRuntime != null)
                {
                    // Fallback to global
                    await JSRuntime.InvokeVoidAsync("bootstrapInterop.showModal", selector);
                }
                else
                {
                    Console.WriteLine("No JS runtime available to show modal");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShowSelectionModalAsync error: " + ex.Message);
                // Try fallback
                try
                {
                    if (JSRuntime != null)
                        await JSRuntime.InvokeVoidAsync("bootstrapInterop.showModal", selector);
                }
                catch { }
            }
        }

        // Called by the modal when a selection is made
        public async Task OnModalSelected(SelectionItem sel)
        {
            var value = sel?.Id ?? string.Empty;
            switch (_openFilterTarget)
            {
                case "application":
                    SelectedApplication = value;
                    break;
                case "mode":
                    SelectedMode = value;
                    break;
                case "tags":
                    SelectedTags = value;
                    break;
                case "os":
                    SelectedOperatingSystem = value;
                    break;
                case "repository":
                    SelectedRepository = value;
                    break;
                case "title":
                    SelectedTitle = value;
                    break;
                case "codelanguage":
                    SelectedCodeLanguage = value;
                    break;
            }
            _openFilterTarget = string.Empty;
            await InvokeAsync(StateHasChanged);
        }

        // Called when a repository breakdown badge is clicked
        public async Task OnRepositoryBreakdownClick(string repository)
        {
            SelectedRepository = repository;
            await InvokeAsync(StateHasChanged);
        }
        private ElementReference searchInput;
        
        [Parameter] public string InitialSearchTerm { get; set; } = string.Empty;
        [Parameter] public bool IsBlazorHybrid { get; set; } = false; // Used to detect if running in Blazor Hybrid mode
        
        public string SearchTerm { get; set; } = string.Empty;
        public List<TalonVoiceCommand> Results { get; set; } = new();
        public bool IsLoading { get; set; }
        public bool HasSearched { get; set; }
        public bool IsRandomResult { get; set; } = false;
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
    public DataAccessLibrary.Services.ITalonVoiceCommandDataService? TalonService { get; set; }
    [Inject]
    public ApplicationMappingService? ApplicationMappingService { get; set; }
    [Inject]
        public IJSRuntime? JSRuntime { get; set; }
        [Inject]
        public IWindowsService? WindowsService { get; set; }

        public string CurrentApplication { get; set; } = string.Empty;
        
        private List<TalonVoiceCommand>? _allCommandsCache;
    // System statistics
    public int TotalCommands { get; set; } = 0;
    public int TotalLists { get; set; } = 0;
    public Dictionary<string, int> RepositoryCounts { get; set; } = new();
        private bool _isLoadingFilters = false;        private CancellationTokenSource? _searchCancellationTokenSource;
        private Timer? _refreshTimer;

    public bool AutoFilterByCurrentApp { get; set; } = false;
        /// <summary>
        /// Toggles displaying full card body or just header
        /// </summary>
        public bool ShowFullCards { get; set; } = false;

        private string _lastAutoFilteredAppName = string.Empty;
        private string _lastAutoRefreshedAppName = string.Empty;

        private string? MapProcessToApplication(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName) || AvailableApplications == null) return null;
            
            // Try to use the mapping service first
            if (ApplicationMappingService != null)
            {
                var displayName = ApplicationMappingService.GetDisplayName(processName);
                // Check if the mapped display name is actually in our available list
                var mapped = AvailableApplications.FirstOrDefault(a => a.Equals(displayName, StringComparison.OrdinalIgnoreCase));
                if (mapped != null) return mapped;
            }

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
            // System.Diagnostics.Debug.WriteLine($"TalonVoiceCommandSearch - InitialSearchTerm: '{InitialSearchTerm}'");
            
            // Always load filter options to ensure fresh data
            await LoadFilterOptions();

            // start auto-refresh every 30 seconds
            StartAutoRefresh();
            // System.Diagnostics.Debug.WriteLine("Auto-refresh timer started (30s interval)");

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
            // System.Diagnostics.Debug.WriteLine($"OnParametersSetAsync - InitialSearchTerm: '{InitialSearchTerm}'");
            
            // Set the initial search term if provided and not already set
            if (!string.IsNullOrWhiteSpace(InitialSearchTerm) && string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Clean up the search term - remove forward slashes and trim
                SearchTerm = InitialSearchTerm.Replace("/", "").Trim();
                // System.Diagnostics.Debug.WriteLine($"SearchTerm set from parameter to: '{SearchTerm}'");
            }
            
            await base.OnParametersSetAsync();
        }

        private async Task LoadFilterOptions()
        {
            if (_isLoadingFilters || TalonService is null) return;
            lock (_filterLock)
            {
                if (_staticFiltersLoaded)
                {
                    // Reuse the static filter lists for UI population but DO NOT
                    // return early — we still need to ensure the command cache
                    // and statistics are available for this instance (e.g. after
                    // a page refresh). Previously an early return left
                    // _allCommandsCache null and UI stats empty.
                    AvailableApplications = _staticAvailableApplications;
                    AvailableModes = _staticAvailableModes;
                    AvailableOperatingSystems = _staticAvailableOperatingSystems;
                    AvailableRepositories = _staticAvailableRepositories;
                    AvailableTags = _staticAvailableTags;
                    AvailableTitles = _staticAvailableTitles;
                    AvailableCodeLanguages = _staticAvailableCodeLanguages;
                    // continue rather than return
                }

                if (_isLoadingFilters) return;
                _isLoadingFilters = true;
            }
            
            try
            {
                // Ensure we have the command cache available even if static filter
                // lists are already loaded. Previously we returned early when
                // _staticFiltersLoaded was true which left _allCommandsCache null
                // for the new component instance and prevented stats/UI from
                // showing.

                // Cache all commands to avoid multiple database calls
                if (_allCommandsCache == null)
                {
                    _allCommandsCache = await TalonService.GetAllCommandsForFiltersAsync();
                }

                if (ApplicationMappingService == null)
                {
                    Console.WriteLine("LoadFilterOptions: ApplicationMappingService is null - mappings will not be applied");
                }

                var all = _allCommandsCache ?? new List<TalonVoiceCommand>();

                var rawApplications = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.Application))
                    .SelectMany(c => c.Application!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()))
                    .Distinct();

                var applications = ApplicationMappingService != null 
                    ? ApplicationMappingService.GetNormalizedList(rawApplications)
                    : rawApplications.OrderBy(a => a).ToList();

                var modes = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.Mode))
                    .Select(c => c.Mode!)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                var operatingSystems = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.OperatingSystem))
                    .Select(c => c.OperatingSystem!)
                    .Distinct()
                    .OrderBy(os => os)
                    .ToList();

                var repositories = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.Repository))
                    .Select(c => c.Repository!)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                var tags = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.Tags))
                    .SelectMany(c => c.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()))
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                var titles = all
                    .Where(c => !string.IsNullOrWhiteSpace(c.Title))
                    .Select(c => c.Title!)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                var codeLanguages = all
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

                // Compute and expose simple system statistics based on cached commands
                ComputeSystemStatsFromCache();

                StateHasChanged();

                // If the page loaded without a prior search or search term,
                // perform an initial search so users see results after a refresh.
                // This ensures the UI isn't empty on first load.
                if (!HasSearched && string.IsNullOrWhiteSpace(SearchTerm))
                {
                    try
                    {
                        await OnSearch();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Initial OnSearch failed: " + ex.Message);
                    }
                }
            }
            finally
            {
                _isLoadingFilters = false;
            }
        }        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // System.Diagnostics.Debug.WriteLine($"OnAfterRenderAsync - firstRender: {firstRender}, SearchTerm: '{SearchTerm}', HasSearched: {HasSearched}");
            
            if (firstRender)
            {
                // Load the selection modal JS module from static web assets first so focus helper is available
                try
                {
                    if (JSRuntime != null && !_selectionModuleLoaded)
                    {
                        _selectionModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/RazorClassLibrary/selectionModalInterop.js");
                        _selectionModuleLoaded = true;
                        Console.WriteLine("Selection modal JS module loaded.");
                        try
                        {
                            await JSRuntime.InvokeVoidAsync("console.debug", "Selection modal JS module loaded (client)");
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    // best effort, fallback to window.bootstrapInterop if available
                    Console.WriteLine("Selection modal JS module failed to load: " + ex.Message);
                    try
                    {
                        if (JSRuntime != null)
                            await JSRuntime.InvokeVoidAsync("console.error", "Selection modal JS module failed to load (client):", ex?.Message);
                    }
                    catch { }
                }

                // Ensure the search input receives focus (module-aware)
                await EnsureSearchFocus();

                // If we have a search term from command line, perform the search after the first render
                if (!string.IsNullOrWhiteSpace(SearchTerm) && !HasSearched)
                {
                    // System.Diagnostics.Debug.WriteLine($"OnAfterRenderAsync - Performing automatic search for: '{SearchTerm}'");
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
        }
        protected async Task OnSearchInputBlur()
        {
            // Avoid triggering search on blur - this can cause focus to be re-applied
            // which traps keyboard navigation (tab). Searches are triggered by
            // Enter or explicit submit instead.
            await Task.CompletedTask;
        }
        public async Task OnSearch()
        {
            // System.Diagnostics.Debug.WriteLine($"OnSearch called - SearchTerm: '{SearchTerm}', Length: {SearchTerm?.Length}");

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
            IsRandomResult = false;
            // Don't search if no criteria are specified - check for default filter states
            bool hasSearchTerm = !string.IsNullOrWhiteSpace(SearchTerm);
            bool hasApplicationFilter = !string.IsNullOrWhiteSpace(SelectedApplication);
            bool hasModeFilter = !string.IsNullOrWhiteSpace(SelectedMode); bool hasOSFilter = !string.IsNullOrWhiteSpace(SelectedOperatingSystem);
            bool hasRepositoryFilter = !string.IsNullOrWhiteSpace(SelectedRepository);
            bool hasTagsFilter = !string.IsNullOrWhiteSpace(SelectedTags);
            bool hasTitleFilter = !string.IsNullOrWhiteSpace(SelectedTitle);
            bool hasCodeLanguageFilter = !string.IsNullOrWhiteSpace(SelectedCodeLanguage);
            try
            {
                // System.Diagnostics.Debug.WriteLine($"OnSearch - hasSearchTerm: {hasSearchTerm}, hasApplicationFilter: {hasApplicationFilter}, hasModeFilter: {hasModeFilter}");

                // Debug logging
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Search conditions - Term: '{SearchTerm}', Length: {SearchTerm?.Length}, UseSemanticMatching: {UseSemanticMatching}, hasSearchTerm: {hasSearchTerm}");
                }

                if (!hasSearchTerm && !hasApplicationFilter && !hasModeFilter && !hasOSFilter && !hasRepositoryFilter && !hasTagsFilter && !hasTitleFilter && !hasCodeLanguageFilter)
                {
                    // System.Diagnostics.Debug.WriteLine("OnSearch - No search criteria, returning early");
                    IsLoading = true;
                    StateHasChanged();

                    string os = "";
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        os = "windows";
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    {
                        os = "linux";
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    {
                        os = "mac";
                    }

                    if (TalonService != null)
                    {
                        Results = await TalonService.GetRandomCommandsAsync(20, os);
                        IsRandomResult = true;
                    }
                    else
                    {
                        Results = new List<TalonVoiceCommand>();
                    }

                    HasSearched = false;
                    IsLoading = false;
                    StateHasChanged();
                    await EnsureSearchFocus();
                    return;
                }

                // System.Diagnostics.Debug.WriteLine("OnSearch - Proceeding with search");
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
                        filteredCommands = filteredCommands.Where(c => 
                            c.Application != null && 
                            c.Application.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(a => a.Trim())
                            .Any(app => 
                                string.Equals(app, SelectedApplication, StringComparison.OrdinalIgnoreCase) ||
                                (ApplicationMappingService != null && string.Equals(ApplicationMappingService.GetDisplayName(app), SelectedApplication, StringComparison.OrdinalIgnoreCase))
                            )
                        );
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
                    if (hasTitleFilter)
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
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Using semantic search for term: '{SearchTerm}' with scope: '{SelectedSearchScope}'");
                        }

                        // Use semantic search methods which search across all fields
                        List<TalonVoiceCommand> semanticResults;
                        switch (SelectedSearchScope)
                        {
                            case SearchScope.CommandNamesOnly:
                                // Use semantic search but filter results to only show command name matches
                                semanticResults = await TalonService.SemanticSearchAsync(SearchTerm!);
                                break;
                            case SearchScope.Script:
                                // Use semantic search but filter results to show script matches
                                semanticResults = await TalonService.SemanticSearchAsync(SearchTerm!);
                                break;
                            case SearchScope.All:
                            default:
                                // Use the proper semantic search method that includes list expansions
                                semanticResults = await TalonService.SemanticSearchWithListsAsync(SearchTerm!);
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
                            if (ApplicationMappingService != null)
                            {
                                var aliases = ApplicationMappingService.GetAliases(SelectedApplication);
                                finalResults = finalResults.Where(c => aliases.Contains(c.Application, StringComparer.OrdinalIgnoreCase));
                            }
                            else
                            {
                                finalResults = finalResults.Where(c => c.Application == SelectedApplication);
                            }
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
                                await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Using non-semantic search for term: '{SearchTerm}' with scope: '{SelectedSearchScope}'");
                            }

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
                                if (ApplicationMappingService != null)
                                {
                                    var aliases = ApplicationMappingService.GetAliases(SelectedApplication);
                                    finalResults = finalResults.Where(c => aliases.Contains(c.Application, StringComparer.OrdinalIgnoreCase));
                                }
                                else
                                {
                                    finalResults = finalResults.Where(c => c.Application == SelectedApplication);
                                }
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
                                await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] After filters applied: {Results.Count} final results");
                            }
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
            // Ensure StateHasChanged runs on the renderer/Dispatcher thread
            await InvokeAsync(() => StateHasChanged());

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
            // Try JS module helper first (robust retries and selection), then fallback
            try
            {
                if (_selectionModuleLoaded && _selectionModule != null)
                {
                    await _selectionModule.InvokeVoidAsync("focusElement", "#tvcs-search-input", 10, true);
                    return;
                }

                // Fallback to ElementReference focus
                await searchInput.FocusAsync();
            }
            catch
            {
                // Final best-effort fallback using global setTimeout
                if (JSRuntime != null)
                {
                    try
                    {
                        await JSRuntime.InvokeVoidAsync("setTimeout", "function() { var el = document.querySelector('#tvcs-search-input'); if (el) el.focus(); }", 10);
                    }
                    catch { }
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
            var fileName = System.IO.Path.GetFileName(filePath);
            // Remove trailing double quote if present
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith("\""))
            {
                fileName = fileName.TrimEnd('"');
            }
            return fileName;
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
        }

        /// <summary>
        /// Compute simple system statistics from the cached commands.
        /// Populates TotalCommands, TotalLists, and RepositoryCounts.
        /// </summary>
        private void ComputeSystemStatsFromCache()
        {
            try
            {
                var all = _allCommandsCache ?? new List<TalonVoiceCommand>();
                TotalCommands = all.Count;

                // Find all list names referenced in commands
                var lists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cmd in all)
                {
                    var used = GetListsUsedInCommand(cmd.Command);
                    foreach (var l in used)
                        lists.Add(l);
                }
                TotalLists = lists.Count;

                // Repository counts
                var repoCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cmd in all)
                {
                    var repo = string.IsNullOrWhiteSpace(cmd.Repository) ? "(none)" : cmd.Repository!;
                    if (!repoCounts.ContainsKey(repo)) repoCounts[repo] = 0;
                    repoCounts[repo]++;
                }
                RepositoryCounts = repoCounts.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ComputeSystemStatsFromCache error: " + ex.Message);
                TotalCommands = 0;
                TotalLists = 0;
                RepositoryCounts = new Dictionary<string, int>();
            }
        }
        public List<string> GetListsUsedInCommand(string command)
        {
            var lists = new List<string>();
            if (string.IsNullOrEmpty(command))
                return lists;

            // Console.WriteLine($"[DEBUG] Analyzing voice command for lists: '{command}'");

            // NOTE: In Talon syntax:
            // {capture} = captures (functions that parse input)
            // <list> = actual list references - THESE are what we want
            // [optional] = optional elements

            // Pattern 1: <list_name> references in voice commands (actual lists)
            var angleBracePattern = @"<([a-zA-Z_][a-zA-Z0-9_.]*)>";
            var angleMatches = Regex.Matches(command, angleBracePattern);
            // Console.WriteLine($"[DEBUG] Found {angleMatches.Count} <list> matches in command");
            foreach (Match match in angleMatches)
            {
                var listName = match.Groups[1].Value;
                // Console.WriteLine($"[DEBUG] Found list reference: '<{listName}>'");
                lists.Add(listName);
            }

            // Pattern 2: [optional] sections that may contain <list> references
            var squareBracePattern = @"\[([^\]]+)\]";
            var squareMatches = Regex.Matches(command, squareBracePattern);
            // Console.WriteLine($"[DEBUG] Found {squareMatches.Count} [optional] sections in command");
            foreach (Match match in squareMatches)
            {
                var content = match.Groups[1].Value;
                // Console.WriteLine($"[DEBUG] Checking optional section: '[{content}]'");

                // Only look for <list> references inside optional sections, ignore {captures}
                var innerAngleMatches = Regex.Matches(content, @"<([a-zA-Z_][a-zA-Z0-9_.]*)>");

                foreach (Match innerMatch in innerAngleMatches)
                {
                    var listName = innerMatch.Groups[1].Value;
                    // Console.WriteLine($"[DEBUG] Found optional list reference: '[<{listName}>]'");
                    lists.Add(listName);
                }
            }

            var finalLists = lists.Distinct().ToList();
            // Console.WriteLine($"[DEBUG] Final lists found in command: {string.Join(", ", finalLists)}");
            // Console.WriteLine($"[DEBUG] Note: Captures like {"{user.text}"} are ignored as they are not lists");
            return finalLists;
        }

        public List<string> GetCapturesUsedInCommand(string command)
        {
            var captures = new List<string>();
            if (string.IsNullOrEmpty(command))
                return captures;

            // Console.WriteLine($"[DEBUG] Analyzing voice command for captures: '{command}'");

            // Pattern 1: {capture_name} references in voice commands
            var curlyCapturePattern = @"\{([a-zA-Z_][a-zA-Z0-9_.]+)\}";
            var curlyMatches = Regex.Matches(command, curlyCapturePattern);
            // Console.WriteLine($"[DEBUG] Found {curlyMatches.Count} {{capture}} matches in command");
            foreach (Match match in curlyMatches)
            {
                var captureName = match.Groups[1].Value;
                // Console.WriteLine($"[DEBUG] Found capture: '{{{captureName}}}'");
                captures.Add(captureName);
            }

            // Pattern 2: [optional] sections that may contain {capture} references
            var squareBracePattern = @"\[([^\]]+)\]";
            var squareMatches = Regex.Matches(command, squareBracePattern);
            // Console.WriteLine($"[DEBUG] Found {squareMatches.Count} [optional] sections in command");
            foreach (Match match in squareMatches)
            {
                var content = match.Groups[1].Value;
                // Console.WriteLine($"[DEBUG] Checking optional section for captures: '[{content}]'");

                // Look for {capture} references inside optional sections
                var innerCurlyMatches = Regex.Matches(content, @"\{([a-zA-Z_][a-zA-Z0-9_.]+)\}");

                foreach (Match innerMatch in innerCurlyMatches)
                {
                    var captureName = innerMatch.Groups[1].Value;
                    // Console.WriteLine($"[DEBUG] Found optional capture: '[{{{captureName}}}]'");
                    captures.Add(captureName);
                }
            }

            var finalCaptures = captures.Distinct().ToList();
            // Console.WriteLine($"[DEBUG] Final captures found in command: {string.Join(", ", finalCaptures)}");
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
            // HTML-encode entire command so raw '<' '>' are visible as text
            var encoded = System.Net.WebUtility.HtmlEncode(command);

            try
            {
                // Highlight lists which are encoded as &lt;name&gt;
                encoded = Regex.Replace(encoded, @"&lt;([a-zA-Z_][a-zA-Z0-9_.]*)&gt;", m =>
                    $"<span class=\"list-highlight\">{m.Value}</span>");

                // Highlight captures which remain as {name} after encoding
                encoded = Regex.Replace(encoded, @"\{([a-zA-Z_][a-zA-Z0-9_.]+)\}", m =>
                    $"<span class=\"capture-highlight\">{System.Net.WebUtility.HtmlEncode(m.Value)}</span>");

                return encoded;
            }
            catch
            {
                // Fallback to safe encoded string
                return System.Net.WebUtility.HtmlEncode(command);
            }
        }

        /// <summary>
        /// Sets focus mode to show only the selected command card
        /// </summary>
        public async Task FocusOnCommand(TalonVoiceCommand command)
        {
            AutoFilterByCurrentApp= false; // Disable auto-filtering when focusing on a command
            _focusedCommand = command;
            _isFocusMode = true;
            ShowFullCards = true; // Show details
            FilterPanelExpanded = true; // Ensure filters are not collapsed
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
