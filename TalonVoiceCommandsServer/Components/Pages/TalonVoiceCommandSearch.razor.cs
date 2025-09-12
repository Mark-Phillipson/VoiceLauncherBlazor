using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TalonVoiceCommandsServer.Models;
using System.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Threading;
using System;
using System.Text.RegularExpressions;
using TalonVoiceCommandsServer.Services;
using TalonVoiceCommandsServer.Components;

namespace TalonVoiceCommandsServer.Components.Pages;

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

protected SelectionModal? _selectionModal;
private IJSObjectReference? _selectionModule;
private bool _selectionModuleLoaded = false;

// Helper to convert string lists to SelectionItem
    private List<SelectionItem> ToSelectionItems(List<string>? items, string? defaultColor = "")
    {
        if (items == null) return new List<SelectionItem>();
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
public ITalonVoiceCommandDataService? TalonService { get; set; }
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
    public List<TalonCommandBreakdown> CommandsBreakdown { get; set; } = new();
    
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
    
    // Tab management for UI improvements
    public enum TabType
    {
        SearchCommands,
        ImportScripts,
        AnalysisReport
    }
    
    public TabType ActiveTab { get; set; } = TabType.SearchCommands;
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
        Console.WriteLine($"TalonVoiceCommandSearch.OnInitializedAsync - InitialSearchTerm: '{InitialSearchTerm}'");
        
        // Try to load basic filter options (won't work until localStorage is loaded via button)
        await LoadFilterOptions();
        
        // DON'T load commands automatically - wait for user to click "Load Data"
        // This prevents blocking the Blazor connection on tab switch
        Console.WriteLine("OnInitializedAsync: Skipping automatic data load to prevent connection timeout");

        // start auto-refresh every 30 seconds
        StartAutoRefresh();
        Console.WriteLine("Auto-refresh timer started (30s interval)");

        // initial application name - handle cross-platform gracefully
        try
        {
            CurrentApplication = WindowsService?.GetActiveProcessName() ?? string.Empty;
        }
        catch (System.DllNotFoundException)
        {
            // Windows APIs not available on non-Windows systems - set default
            CurrentApplication = string.Empty;
            Console.WriteLine("Windows API not available - running in cross-platform mode");
        }
        catch (Exception ex)
        {
            CurrentApplication = string.Empty;
            Console.WriteLine($"Error getting active process name: {ex.Message}");
        }
        
        Console.WriteLine($"OnInitializedAsync completed - Commands in cache: {_allCommandsCache?.Count ?? 0}");
    }

    private void StartAutoRefresh()
    {
        _refreshTimer = new Timer(async _ =>
        {
            var appName = string.Empty;
            try
            {
                appName = WindowsService?.GetActiveProcessName() ?? string.Empty;
            }
            catch (System.DllNotFoundException)
            {
                // Windows APIs not available on non-Windows systems - use empty string
                appName = string.Empty;
            }
            catch (Exception)
            {
                // Handle any other exceptions gracefully
                appName = string.Empty;
            }
            
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
                AvailableApplications = _staticAvailableApplications;
                AvailableModes = _staticAvailableModes;
                AvailableOperatingSystems = _staticAvailableOperatingSystems;
                AvailableRepositories = _staticAvailableRepositories;
                AvailableTags = _staticAvailableTags;
                AvailableTitles = _staticAvailableTitles;
                AvailableCodeLanguages = _staticAvailableCodeLanguages;
                Console.WriteLine($"LoadFilterOptions: Using cached filters - {AvailableApplications.Count} applications");
                return;
            }
            
            if (_isLoadingFilters) return;
            _isLoadingFilters = true;
        }
        
        try
        {
            Console.WriteLine("LoadFilterOptions: Loading fresh data from service");
            
            // Don't automatically load from storage during initialization
            // Only use data if it's already available in the service
            
            // Cache all commands to avoid multiple database calls (only if data already exists)
            _allCommandsCache = await TalonService.GetAllCommandsForFiltersAsync();
            Console.WriteLine($"LoadFilterOptions: Retrieved {_allCommandsCache.Count} commands from service");
            
            // If no commands are available, don't try to load from storage automatically
            if (_allCommandsCache.Count == 0)
            {
                Console.WriteLine("LoadFilterOptions: No commands available - skipping filter population to prevent auto-loading");
                return;
            }
            
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
                
            var titles = _allCommandsCache
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
                
                // Compute statistics after loading commands
                ComputeSystemStatsFromCache();
            }
            
            Console.WriteLine($"LoadFilterOptions: Completed successfully - {applications.Count} applications, {modes.Count} modes, {repositories.Count} repositories");
            StateHasChanged();
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
            await searchInput.FocusAsync();

            // Load the selection modal JS module from static web assets
            try
            {
                if (JSRuntime != null && !_selectionModuleLoaded)
                {
                    // Use the local interop file so this project doesn't rely on the RazorClassLibrary static web assets
                    _selectionModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/selectionModalInterop.js");
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
            
            // Note: localStorage loading is now handled by the manual "Load Data" button
            // to avoid connection timeouts during page initialization

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
    
    /// <summary>
    /// Ensures that command and list data is loaded from localStorage before allowing searches.
    /// This is now a simple single-attempt method called only by the Load Data button.
    /// </summary>
    private async Task EnsureDataIsLoadedForSearch()
    {
        if (TalonService == null) return;
        
        try
        {
            Console.WriteLine("EnsureDataIsLoadedForSearch: Loading data from localStorage");
            
            // Single attempt to load from localStorage
            if (TalonService is TalonVoiceCommandsServer.Services.TalonVoiceCommandDataService concrete)
            {
                await concrete.EnsureLoadedFromLocalStorageAsync(JSRuntime);
                Console.WriteLine("EnsureDataIsLoadedForSearch: localStorage load completed");
            }

            // Load filter options which will read the service cache and populate _allCommandsCache
            if ((_allCommandsCache == null || !_staticFiltersLoaded))
            {
                Console.WriteLine("EnsureDataIsLoadedForSearch: Loading filter options");
                await LoadFilterOptions();
            }

            Console.WriteLine($"EnsureDataIsLoadedForSearch completed: _allCommandsCache count: {(_allCommandsCache?.Count ?? 0)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EnsureDataIsLoadedForSearch error: {ex.Message}");
        }
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
        // Only trigger search when input loses focus if we have cached data available
        if (_allCommandsCache != null && _allCommandsCache.Count > 0)
        {
            await OnSearch();
        }
    }        
    
    protected async Task OnSearch()
    {
        Console.WriteLine("=== OnSearch method ENTRY POINT - this should always appear ===");
        System.Diagnostics.Debug.WriteLine("=== OnSearch method ENTRY POINT - DEBUG ===");
        
        var searchStartTime = DateTime.UtcNow;
        Console.WriteLine($"OnSearch: Starting search at {searchStartTime:HH:mm:ss.fff}");
        Console.WriteLine("**** ONSEARCH METHOD CALLED - THIS SHOULD APPEAR IN LOGS ****");
        
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
            // System.Diagnostics.Debug.WriteLine($"OnSearch - hasSearchTerm: {hasSearchTerm}, hasApplicationFilter: {hasApplicationFilter}, hasModeFilter: {hasModeFilter}");
            
            // Debug logging
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Search conditions - Term: '{SearchTerm}', Length: {SearchTerm?.Length}, UseSemanticMatching: {UseSemanticMatching}, hasSearchTerm: {hasSearchTerm}");
            }
            
            if (!hasSearchTerm && !hasApplicationFilter && !hasModeFilter && !hasOSFilter && !hasRepositoryFilter && !hasTagsFilter && !hasTitleFilter && !hasCodeLanguageFilter)
            {
                // System.Diagnostics.Debug.WriteLine("OnSearch - No search criteria, returning early");
                Results = new List<TalonVoiceCommand>();
                HasSearched = false;
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
            // Use direct JavaScript display to avoid SignalR data transfer issues
            try
            {
                Console.WriteLine("OnSearch: Starting direct JavaScript search and display...");
                
                // Show the JavaScript results container and hide C# results
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("eval", "document.querySelector('.search-results-container').style.display = 'block'");
                }
                
                var resultCount = await TalonService.SearchAndDisplayDirectlyAsync(
                    searchTerm: hasSearchTerm ? SearchTerm : null,
                    application: hasApplicationFilter ? SelectedApplication : null,
                    mode: hasModeFilter ? SelectedMode : null,
                    operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                    repository: hasRepositoryFilter ? SelectedRepository : null,
                    tags: hasTagsFilter ? SelectedTags : null,
                    title: hasTitleFilter ? SelectedTitle : null,
                    codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                    useSemanticMatching: UseSemanticMatching,
                    searchScope: (int)SelectedSearchScope,
                    maxResults: 500
                );

                Console.WriteLine($"OnSearch: Direct display showed {resultCount} results");
                
                // Clear C# results since we're using JavaScript display
                Results = new List<TalonVoiceCommand>();
                
                IsLoading = false;
                HasSearched = true;
                StateHasChanged();
                var searchEndTime = DateTime.UtcNow;
                var duration = searchEndTime - searchStartTime;
                Console.WriteLine($"OnSearch: Direct search completed successfully in {duration.TotalMilliseconds}ms at {searchEndTime:HH:mm:ss.fff}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnSearch: Direct display failed: {ex.Message}");
                Console.WriteLine("OnSearch: Falling back to simple search...");
                
                // Hide JavaScript results container on error
                if (JSRuntime != null)
                {
                    try
                    {
                        await JSRuntime.InvokeVoidAsync("eval", "document.querySelector('.search-results-container').style.display = 'none'");
                    }
                    catch { }
                }
            }

            // Fallback: Use simple limited search to prevent SignalR timeouts
            try
            {
                Console.WriteLine("OnSearch: Starting simple limited IndexedDB search to prevent timeouts...");
                var searchResults = await TalonService.SearchFilteredCommandsSimpleAsync(
                    searchTerm: hasSearchTerm ? SearchTerm : null,
                    application: hasApplicationFilter ? SelectedApplication : null,
                    mode: hasModeFilter ? SelectedMode : null,
                    operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                    repository: hasRepositoryFilter ? SelectedRepository : null,
                    tags: hasTagsFilter ? SelectedTags : null,
                    title: hasTitleFilter ? SelectedTitle : null,
                    codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                    useSemanticMatching: UseSemanticMatching,
                    searchScope: (int)SelectedSearchScope,
                    maxResults: 500
                );

                Console.WriteLine($"OnSearch: Simple search returned {searchResults.Count} results");
                Results = searchResults;
                
                IsLoading = false;
                HasSearched = true;
                StateHasChanged();
                var searchEndTime = DateTime.UtcNow;
                var duration = searchEndTime - searchStartTime;
                Console.WriteLine($"OnSearch: Simple search completed successfully in {duration.TotalMilliseconds}ms at {searchEndTime:HH:mm:ss.fff}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnSearch: Simple search failed: {ex.Message}");
                Console.WriteLine("OnSearch: Falling back to C#-only search...");
            }

            // Fallback: Try C#-only search if simple search fails
            try
            {
                Console.WriteLine("OnSearch: Starting C#-only in-memory search...");
                var searchResults = await TalonService.GetFilteredCommandsInMemory(
                    searchTerm: hasSearchTerm ? SearchTerm : null,
                    application: hasApplicationFilter ? SelectedApplication : null,
                    mode: hasModeFilter ? SelectedMode : null,
                    operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                    repository: hasRepositoryFilter ? SelectedRepository : null,
                    tags: hasTagsFilter ? SelectedTags : null,
                    title: hasTitleFilter ? SelectedTitle : null,
                    codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                    useSemanticMatching: UseSemanticMatching,
                    searchScope: (int)SelectedSearchScope
                );

                // If we got results OR if there are no search criteria (meaning we should show all results)
                // then use the C# results. If empty but we have search criteria, fall back to JS approach.
                if (searchResults.Count > 0 || (!hasSearchTerm && !hasApplicationFilter && !hasModeFilter && !hasOSFilter && !hasRepositoryFilter && !hasTagsFilter && !hasTitleFilter && !hasCodeLanguageFilter))
                {
                    Console.WriteLine($"OnSearch: C#-only search returned {searchResults.Count} results, setting Results...");
                    Results = searchResults;
                    Console.WriteLine("OnSearch: Results set, updating UI state...");
                    IsLoading = false;
                    HasSearched = true;
                    StateHasChanged();
                    var searchEndTime = DateTime.UtcNow;
                    var duration = searchEndTime - searchStartTime;
                    Console.WriteLine($"OnSearch: C#-only search completed successfully in {duration.TotalMilliseconds}ms at {searchEndTime:HH:mm:ss.fff}");
                    return;
                }
                else
                {
                    Console.WriteLine("OnSearch: C#-only search returned empty results (likely no data in memory)");
                    Console.WriteLine("OnSearch: Attempting to load data from IndexedDB first...");
                    
                    // Try to load data from IndexedDB with a timeout
                    try
                    {
                        using var loadCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                        await TalonService.EnsureLoadedFromIndexedDBAsync(JSRuntime);
                        Console.WriteLine("OnSearch: Successfully loaded data from IndexedDB, retrying C# search...");
                        
                        // Retry the C# search now that data should be loaded
                        var retryResults = await TalonService.GetFilteredCommandsInMemory(
                            searchTerm: hasSearchTerm ? SearchTerm : null,
                            application: hasApplicationFilter ? SelectedApplication : null,
                            mode: hasModeFilter ? SelectedMode : null,
                            operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                            repository: hasRepositoryFilter ? SelectedRepository : null,
                            tags: hasTagsFilter ? SelectedTags : null,
                            title: hasTitleFilter ? SelectedTitle : null,
                            codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                            useSemanticMatching: UseSemanticMatching,
                            searchScope: (int)SelectedSearchScope
                        );
                        
                        Console.WriteLine($"OnSearch: Retry search returned {retryResults.Count} results");
                        Results = retryResults;
                        IsLoading = false;
                        HasSearched = true;
                        StateHasChanged();
                        var searchEndTime = DateTime.UtcNow;
                        var duration = searchEndTime - searchStartTime;
                        Console.WriteLine($"OnSearch: Data load + search completed in {duration.TotalMilliseconds}ms");
                        return;
                    }
                    catch (Exception loadEx)
                    {
                        Console.WriteLine($"OnSearch: Failed to load data from IndexedDB: {loadEx.Message}");
                        Console.WriteLine("OnSearch: Falling back to ID-based search...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnSearch: C#-only search failed with exception: {ex.Message}");
                Console.WriteLine($"OnSearch: Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"OnSearch: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("OnSearch: Falling back to ID-based search...");
            }

            // Try the ID-based filtered search as first fallback
            try
            {
                Console.WriteLine("OnSearch: Starting ID-based filtered search...");
                var searchResults = await TalonService.SearchFilteredCommandsByIdsAsync(
                    searchTerm: hasSearchTerm ? SearchTerm : null,
                    application: hasApplicationFilter ? SelectedApplication : null,
                    mode: hasModeFilter ? SelectedMode : null,
                    operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                    repository: hasRepositoryFilter ? SelectedRepository : null,
                    tags: hasTagsFilter ? SelectedTags : null,
                    title: hasTitleFilter ? SelectedTitle : null,
                    codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                    useSemanticMatching: UseSemanticMatching,
                    searchScope: (int)SelectedSearchScope
                );

                Console.WriteLine($"OnSearch: ID-based search returned {searchResults.Count} results, setting Results...");
                Results = searchResults;
                Console.WriteLine("OnSearch: Results set, updating UI state...");
                IsLoading = false;
                HasSearched = true;
                StateHasChanged();
                var searchEndTime = DateTime.UtcNow;
                var duration = searchEndTime - searchStartTime;
                Console.WriteLine($"OnSearch: ID-based search completed successfully in {duration.TotalMilliseconds}ms at {searchEndTime:HH:mm:ss.fff}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnSearch: ID-based search failed with exception: {ex.Message}");
                Console.WriteLine($"OnSearch: Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"OnSearch: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("OnSearch: Falling back to full object search...");
            }

            // Fallback to original filtered search
            try
            {
                Console.WriteLine("OnSearch: Starting full object filtered search...");
                var searchResults = await TalonService.SearchFilteredCommandsAsync(
                    searchTerm: hasSearchTerm ? SearchTerm : null,
                    application: hasApplicationFilter ? SelectedApplication : null,
                    mode: hasModeFilter ? SelectedMode : null,
                    operatingSystem: hasOSFilter ? SelectedOperatingSystem : null,
                    repository: hasRepositoryFilter ? SelectedRepository : null,
                    tags: hasTagsFilter ? SelectedTags : null,
                    title: hasTitleFilter ? SelectedTitle : null,
                    codeLanguage: hasCodeLanguageFilter ? SelectedCodeLanguage : null,
                    useSemanticMatching: UseSemanticMatching,
                    searchScope: (int)SelectedSearchScope
                );

                Console.WriteLine($"OnSearch: Filtered search returned {searchResults.Count} results, setting Results...");
                Results = searchResults;
                Console.WriteLine("OnSearch: Results set, updating UI state...");
                IsLoading = false;
                HasSearched = true;
                StateHasChanged();
                var searchEndTime = DateTime.UtcNow;
                var duration = searchEndTime - searchStartTime;
                Console.WriteLine($"OnSearch: Filtered search completed successfully in {duration.TotalMilliseconds}ms at {searchEndTime:HH:mm:ss.fff}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnSearch: Filtered search failed with exception: {ex.Message}");
                Console.WriteLine($"OnSearch: Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"OnSearch: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine("OnSearch: Falling back to cache approach...");
            }
            
            // Fallback: Check if we have cached data, if not show a helpful message
            if (_allCommandsCache == null || _allCommandsCache.Count == 0)
            {
                Console.WriteLine("OnSearch: No cached commands available and filtered search failed");
                Results = new List<TalonVoiceCommand>();
                IsLoading = false;
                HasSearched = true;
                StateHasChanged();
                return;
            }
            
            // Use cached data (which we know is loaded if we get to this point)
            var allCommands = _allCommandsCache;
            Console.WriteLine($"OnSearch: Using fallback cache approach with {allCommands.Count} commands");
            
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
                    // Debug logging with try-catch to prevent JS connection issues
                    try
                    {
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Using non-semantic search for term: '{SearchTerm}' with scope: '{SelectedSearchScope}'");
                        }
                    }
                    catch (Exception jsEx)
                    {
                        Console.WriteLine($"[DEBUG] JS logging failed: {jsEx.Message}");
                    }
                    
                    // Search within the already filtered cache data instead of calling service methods
                    Console.WriteLine($"[DEBUG] Starting in-memory search for '{SearchTerm}' on {filteredCommands.Count()} filtered commands");
                    var lowerTerm = SearchTerm!.ToLower();
                    List<TalonVoiceCommand> searchResults;
                    
                    var startTime = DateTime.Now;
                    switch (SelectedSearchScope)
                    {
                        case SearchScope.CommandNamesOnly:
                            Console.WriteLine("[DEBUG] Searching command names only...");
                            searchResults = filteredCommands
                                .Where(c => c.Command.ToLower().Contains(lowerTerm))
                                .Take(100)
                                .ToList();
                            break;
                        case SearchScope.Script:
                            Console.WriteLine("[DEBUG] Searching scripts only...");
                            searchResults = filteredCommands
                                .Where(c => c.Script.ToLower().Contains(lowerTerm))
                                .Take(100)
                                .ToList();
                            break;
                        case SearchScope.All:
                        default:
                            Console.WriteLine("[DEBUG] Searching all fields...");
                            searchResults = filteredCommands
                                .Where(c => c.Command.ToLower().Contains(lowerTerm) ||
                                           c.Script.ToLower().Contains(lowerTerm) ||
                                           c.Application.ToLower().Contains(lowerTerm) ||
                                           (c.Mode != null && c.Mode.ToLower().Contains(lowerTerm)) ||
                                           (c.Title != null && c.Title.ToLower().Contains(lowerTerm)))
                                .Take(100)
                                .ToList();
                            break;
                    }
                    var elapsed = DateTime.Now - startTime;
                    Console.WriteLine($"[DEBUG] In-memory search completed in {elapsed.TotalMilliseconds}ms, found {searchResults.Count} results");
                    
                    // Debug logging with try-catch to prevent JS connection issues
                    try
                    {
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("console.log", $"[DEBUG] Non-semantic search returned {searchResults.Count} results");
                        }
                    }
                    catch (Exception jsEx)
                    {
                        Console.WriteLine($"[DEBUG] JS logging failed: {jsEx.Message}");
                    }
                    
                    Results = searchResults.OrderByDescending(c => c.CreatedAt).ToList();
                    
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
        try
        {
            StateHasChanged();
        }
        catch (Exception stateEx)
        {
            Console.WriteLine($"StateHasChanged error: {stateEx.Message}");
        }
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
    
    /// <summary>
    /// Switches to the specified tab
    /// </summary>
    public void SwitchTab(TabType tab)
    {
        ActiveTab = tab;
        StateHasChanged();
    }
    
    /// <summary>
    /// Handles keyboard shortcuts for tab navigation
    /// </summary>
    public async Task OnKeyDown(KeyboardEventArgs e)
    {
        // Handle Alt+number shortcuts for tab switching
        if (e.AltKey)
        {
            switch (e.Key)
            {
                case "1":
                    SwitchTab(TabType.SearchCommands);
                    break;
                case "2":
                    SwitchTab(TabType.ImportScripts);
                    break;
                case "3":
                    SwitchTab(TabType.AnalysisReport);
                    break;
            }
        }
        
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource?.Dispose();
        _refreshTimer?.Dispose();
    }

    /// <summary>
    /// Load commands into cache and compute system statistics
    /// </summary>
    private async Task LoadCommandsAndComputeStatistics()
    {
        if (TalonService != null)
        {
            // Ensure commands are cached
            _allCommandsCache = await TalonService.GetAllCommandsForFiltersAsync();
            
            // Load breakdown
            CommandsBreakdown = await TalonService.GetTalonCommandsBreakdownAsync();
            
            // Compute statistics
            ComputeSystemStatsFromCache();
        }
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
                var repo = cmd.Repository ?? "Unknown";
                if (repoCounts.ContainsKey(repo))
                    repoCounts[repo]++;
                else
                    repoCounts[repo] = 1;
            }
            RepositoryCounts = repoCounts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error computing system stats: {ex.Message}");
            TotalCommands = 0;
            TotalLists = 0;
            RepositoryCounts = new Dictionary<string, int>();
        }
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
