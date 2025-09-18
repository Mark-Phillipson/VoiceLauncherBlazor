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
    [Inject]
    public FilterRefreshService FilterRefreshService { get; set; } = default!;
// Shared modal state (used to populate the reusable modal)
    public List<SelectionItem> SelectionModalItems { get; set; } = new();
    public string SelectionModalTitle { get; set; } = "Select";
    private string _openFilterTarget = string.Empty;

protected SelectionModal? _selectionModal;
private IJSObjectReference? _selectionModule;
private bool _selectionModuleLoaded = false;

    // Controls whether the search/filters region is collapsed to give more room to results
    public bool IsFiltersCollapsed { get; set; } = false;

    public void ToggleFiltersCollapse()
    {
        IsFiltersCollapsed = !IsFiltersCollapsed;
        // Ensure UI updates
        InvokeAsync(StateHasChanged);
    }

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
        Console.WriteLine($"ShowTitleModal: AvailableTitles count: {AvailableTitles?.Count ?? 0}");
        if (AvailableTitles?.Any() == true)
        {
            Console.WriteLine($"ShowTitleModal: Sample titles: {string.Join(", ", AvailableTitles.Take(5))}");
        }
        else
        {
            Console.WriteLine($"ShowTitleModal: _allCommandsCache count: {_allCommandsCache?.Count ?? 0}");
            if (_allCommandsCache?.Any() == true)
            {
                var titlesFromCache = _allCommandsCache
                    .Where(c => !string.IsNullOrWhiteSpace(c.Title))
                    .Select(c => c.Title!)
                    .Distinct()
                    .Take(5)
                    .ToList();
                Console.WriteLine($"ShowTitleModal: Sample titles from cache: {string.Join(", ", titlesFromCache)}");
            }
        }
        
        SelectionModalItems = ToSelectionItems(AvailableTitles, "bg-light");
        SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Titles" });
        
        // Add helpful message if no titles are available
        if (AvailableTitles?.Any() != true)
        {
            SelectionModalItems.Add(new SelectionItem 
            { 
                Id = "no-data", 
                Label = "No titles available - Import Talon scripts first", 
                ColorClass = "bg-warning text-dark"
            });
        }
        
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
    public bool IsUsingJavaScriptDisplay { get; set; } = false; // Track when JavaScript is displaying results
    public int JavaScriptResultCount { get; set; } = 0; // Track JavaScript result count
    public bool UseSemanticMatching { get; set; } = false;
    public SearchScope SelectedSearchScope { get; set; } = SearchScope.CommandNamesOnly; // Default to command names only
    public string InfoMessage { get; set; } = string.Empty;
    
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
    
    // Helper method to clear JavaScript display state
    private void ClearJavaScriptDisplay()
    {
        IsUsingJavaScriptDisplay = false;
        JavaScriptResultCount = 0;
    }
    
    // Helper method to get the effective result count
    public int GetEffectiveResultCount()
    {
        return IsUsingJavaScriptDisplay ? JavaScriptResultCount : (Results?.Count ?? 0);
    }
    
    // Helper method to check if there are any results to display
    public bool HasAnyResults()
    {
        return IsUsingJavaScriptDisplay ? JavaScriptResultCount > 0 : (Results?.Any() == true);
    }
    
    // Debug method to force reload filter options
    public async Task ForceReloadFilterOptions()
    {
        Console.WriteLine("ForceReloadFilterOptions: Starting manual reload...");
        lock (_filterLock)
        {
            _staticFiltersLoaded = false;
        }
        _allCommandsCache = null;
        await LoadFilterOptions();
        StateHasChanged();
        Console.WriteLine("ForceReloadFilterOptions: Completed");
    }

[Inject]
public ITalonVoiceCommandDataService? TalonService { get; set; }
[Inject]
    public IJSRuntime? JSRuntime { get; set; }
    [Inject]
    public IWindowsService? WindowsService { get; set; }

    public string CurrentApplication { get; set; } = string.Empty;
    
    private List<TalonVoiceCommand>? _allCommandsCache;
    // Tracks whether any data exists either in-memory or persisted in IndexedDB
    public bool HasAnyData { get; set; } = false;
    
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

    private string NormalizeAppName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var n = name.Trim().ToLowerInvariant();
        if (n == "code" || n == "vscode" || n == "visual studio code" || n == "vs code") return "visual studio code";
        if (n == "devenv" || n == "visual studio" || n == "vs" || n == "msvs") return "visual studio";
        if (n == "chrome" || n == "google chrome") return "chrome";
        if (n == "msedge" || n == "edge" || n == "microsoft edge") return "edge";
        return name;
    }

    private string? MapProcessToApplication(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName) || AvailableApplications == null) return null;
        var normalized = NormalizeAppName(processName);
        var exact = AvailableApplications.FirstOrDefault(a => a.Equals(normalized, StringComparison.OrdinalIgnoreCase) || a.Equals(processName, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;
        var partial = AvailableApplications.FirstOrDefault(a =>
            a.IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0 ||
            normalized.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0 ||
            a.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0 ||
            processName.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
        return partial;
    }

    // For list display functionality
    private Dictionary<string, List<TalonList>> _listContentsCache = new();
    private HashSet<string> _expandedLists = new();
    // Side-panel state for showing full list contents on the left
    private bool _isListPanelOpen = false;
    private string? _selectedPanelListName = null;
    private List<TalonList> _selectedPanelValues = new();
    private bool _isListPanelLoading = false;
    // Tracks the most recently requested list for the side-panel so older/background
    // loads don't overwrite a newer user's click (prevents stale content showing).
    private string? _lastRequestedPanelListName = null;

    // Lists tab state
    private List<string> _allListNames = new();
    private string _listFilterTerm = string.Empty;
    public string ListFilterTerm
    {
        get => _listFilterTerm;
        set
        {
            if (_listFilterTerm != value)
            {
                _listFilterTerm = value;
                // ensure UI updates immediately while typing
                InvokeAsync(StateHasChanged);
            }
        }
    }
    private bool _isListsLoading = false;
    private string? _highlightedListName = null;

    // For focused card functionality
    private TalonVoiceCommand? _focusedCommand = null;
    private bool _isFocusMode = false;
    
    // Tab management for UI improvements
    public enum TabType
    {
        SearchCommands,
        ImportScripts,
        AnalysisReport,
        Lists
    }
    
    public TabType ActiveTab { get; set; } = TabType.SearchCommands;
    private static bool _staticFiltersLoaded = false;        private static List<string> _staticAvailableApplications = new();
    private static List<string> _staticAvailableModes = new();
    private static List<string> _staticAvailableOperatingSystems = new();
    private static List<string> _staticAvailableRepositories = new();
    private static List<string> _staticAvailableTags = new();
    private static List<string> _staticAvailableTitles = new();
    private static List<string> _staticAvailableCodeLanguages = new();private static readonly object _filterLock = new object();

    // Predefined common filter values that users can use immediately without imports
    private static readonly List<string> _predefinedApplications = new()
    {
        "vscode", "code", "visual studio code", "vs code",
        "chrome", "google chrome", "firefox", "edge", "safari",
        "terminal", "cmd", "powershell", "bash", "zsh",
        "word", "excel", "powerpoint", "outlook", "teams",
        "slack", "discord", "zoom", "skype",
        "notepad", "sublime", "atom", "vim", "emacs",
        "windows", "explorer", "finder",
        "global", "default"
    };
    
    private static readonly List<string> _predefinedModes = new()
    {
        "command", "insert", "dictation", "sleep"
    };
    
    private static readonly List<string> _predefinedOperatingSystems = new()
    {
        "windows", "linux", "mac", "macos", "ubuntu", "debian"
    };
    
    private static readonly List<string> _predefinedRepositories = new()
    {
        "community", "knausj_talon", "talon_community", "user_settings", "personal"
    };
    
    private static readonly List<string> _predefinedTags = new()
    {
        "navigation", "editing", "browser", "terminal", "git", "debugging",
        "file_management", "window_management", "text_manipulation",
        "code_completion", "refactoring", "search", "replace",
        "user.terminal", "user.bash", "user.powershell",
        "user.vim", "user.emacs", "user.vscode",
        "user.chrome", "user.firefox",
        "user.coding", "user.debugging", "user.git"
    };
    
    private static readonly List<string> _predefinedCodeLanguages = new()
    {
        "python", "javascript", "typescript", "c#", "csharp", "java", "cpp", "c++",
        "go", "rust", "html", "css", "sql", "json", "xml", "yaml", "markdown",
        "bash", "powershell", "dockerfile", "terraform"
    };

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
        await InvokeAsync(StateHasChanged);
        // If the title modal is open, refresh its contents
        if (_openFilterTarget == "title" && _selectionModal != null)
        {
            SelectionModalItems = ToSelectionItems(AvailableTitles, "bg-light");
            SelectionModalItems.Insert(0, new SelectionItem { Id = string.Empty, Label = "All Titles" });
            await _selectionModal.ShowAsync();
        }
    }        protected override async Task OnInitializedAsync()
    {
        Results = new List<TalonVoiceCommand>();
        ClearJavaScriptDisplay(); // Initialize display state
        
        // Debug: Log the initial search term
        Console.WriteLine($"TalonVoiceCommandSearch.OnInitializedAsync - InitialSearchTerm: '{InitialSearchTerm}'");
        
        // Try to load basic filter options (won't work until localStorage is loaded via button)
        await LoadFilterOptions();

        // Subscribe to filter refresh event
        if (FilterRefreshService != null)
        {
            FilterRefreshService.OnRefreshRequested += RefreshFiltersAsync;
        }
        
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
        await UpdateHasAnyDataAsync();
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
            await UpdateHasAnyDataAsync();
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

    /// <summary>
    /// Loads predefined filter values that users can use immediately without importing data
    /// </summary>
    private void LoadPredefinedFilterValues()
    {
        Console.WriteLine("LoadPredefinedFilterValues: Loading predefined filter values for immediate use");
        
        lock (_filterLock)
        {
            // Set predefined values directly to instance properties
            AvailableApplications = _predefinedApplications.ToList();
            AvailableModes = _predefinedModes.ToList();
            AvailableOperatingSystems = _predefinedOperatingSystems.ToList();
            AvailableRepositories = _predefinedRepositories.ToList();
            AvailableTags = _predefinedTags.ToList();
            AvailableTitles = new List<string>(); // Will be populated from imported data
            AvailableCodeLanguages = _predefinedCodeLanguages.ToList();
            
            Console.WriteLine($"LoadPredefinedFilterValues: Loaded {AvailableApplications.Count} applications, {AvailableModes.Count} modes, {AvailableOperatingSystems.Count} operating systems");
        }
        
        StateHasChanged();
    }

    /// <summary>
    /// Merges predefined filter values with data from imported commands
    /// </summary>
    private List<string> MergeWithPredefined(List<string> importedValues, List<string> predefinedValues)
    {
        var merged = new HashSet<string>(predefinedValues, StringComparer.OrdinalIgnoreCase);
        foreach (var value in importedValues)
        {
            merged.Add(value);
        }
        return merged.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
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
            Console.WriteLine("LoadFilterOptions: Starting JavaScript-based filter loading process");
            
            // STEP 1: Always load predefined values first so users have immediate filter options
            LoadPredefinedFilterValues();
            
            // STEP 2: Try to get imported data using JavaScript to prevent connection timeouts
            var filterValues = await TalonService.GetFilterValuesFromJavaScriptAsync();
            Console.WriteLine($"LoadFilterOptions: Retrieved filter values from JavaScript");
            
            // STEP 3: If we have imported data, merge it with predefined values for enhanced filtering
            if (filterValues.Applications.Count > 0 || filterValues.Modes.Count > 0 || 
                filterValues.Repositories.Count > 0 || filterValues.Tags.Count > 0 ||
                filterValues.Titles.Count > 0)
            {
                Console.WriteLine("LoadFilterOptions: Enhancing predefined filters with imported data");
                
                // Get basic statistics without loading all commands
                var stats = await TalonService.GetDataStatisticsFromJavaScriptAsync();
                
                // Merge imported data with predefined values
                lock (_filterLock)
                {
                    AvailableApplications = _staticAvailableApplications = MergeWithPredefined(filterValues.Applications, _predefinedApplications);
                    AvailableModes = _staticAvailableModes = MergeWithPredefined(filterValues.Modes, _predefinedModes);
                    AvailableOperatingSystems = _staticAvailableOperatingSystems = MergeWithPredefined(filterValues.OperatingSystems, _predefinedOperatingSystems);
                    AvailableRepositories = _staticAvailableRepositories = MergeWithPredefined(filterValues.Repositories, _predefinedRepositories);
                    AvailableTags = _staticAvailableTags = MergeWithPredefined(filterValues.Tags, _predefinedTags);
                    AvailableTitles = _staticAvailableTitles = filterValues.Titles; // Titles come only from imported data
                    Console.WriteLine($"LoadFilterOptions: Set AvailableTitles to {AvailableTitles.Count} items");
                    AvailableCodeLanguages = _staticAvailableCodeLanguages = MergeWithPredefined(filterValues.CodeLanguages, _predefinedCodeLanguages);
                    _staticFiltersLoaded = true;
                    
                    TotalCommands = stats.TotalCommands;
                    HasAnyData = stats.HasData;
                }
                
                Console.WriteLine($"LoadFilterOptions: Enhanced filters - {AvailableApplications.Count} applications, {AvailableModes.Count} modes, {AvailableRepositories.Count} repositories");
                Console.WriteLine($"LoadFilterOptions: Found {filterValues.Repositories.Count} imported repositories: {string.Join(", ", filterValues.Repositories)}");
                Console.WriteLine($"LoadFilterOptions: Found {filterValues.Tags.Count} imported tags: {string.Join(", ", filterValues.Tags.Take(10))}");
            }
            else
            {
                Console.WriteLine("LoadFilterOptions: No imported data available, using predefined values only");
                
                // Still cache the predefined values as static for consistency
                lock (_filterLock)
                {
                    _staticAvailableApplications = AvailableApplications.ToList();
                    _staticAvailableModes = AvailableModes.ToList();
                    _staticAvailableOperatingSystems = AvailableOperatingSystems.ToList();
                    _staticAvailableRepositories = AvailableRepositories.ToList();
                    _staticAvailableTags = AvailableTags.ToList();
                    _staticAvailableTitles = AvailableTitles.ToList();
                    _staticAvailableCodeLanguages = AvailableCodeLanguages.ToList();
                    _staticFiltersLoaded = true;
                }
            }
            
            Console.WriteLine($"LoadFilterOptions: Completed successfully - {AvailableApplications.Count} applications, {AvailableModes.Count} modes");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadFilterOptions: Error occurred - {ex.Message}");
            // Even if there's an error, ensure users have predefined values to work with
            LoadPredefinedFilterValues();
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
            try
            {
                if (searchInput.Context != null)
                {
                    await searchInput.FocusAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FocusAsync error: {ex.Message}");
            }

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
            
            // CRITICAL FIX: Now that JavaScript interop is available, attempt to refresh filters
            // if they couldn't be loaded during OnInitializedAsync due to static rendering
            try
            {
                // Check if we have minimal filter data (indicating OnInitializedAsync filter load failed)
                if (AvailableTitles.Count == 0 && TalonService != null)
                {
                    Console.WriteLine("OnAfterRenderAsync: Attempting to refresh filters now that JS interop is available");
                    await LoadFilterOptions();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnAfterRenderAsync: Filter refresh attempt failed: {ex.Message}");
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

            // If the Lists tab is active on first render, focus its search box for keyboard users
            try
            {
                if (ActiveTab == TabType.Lists && JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("eval", "(function(){const el=document.querySelector('input[placeholder=\\\"Search lists...\\\"]'); if(el){el.focus(); el.select();}})()");
                }
            }
            catch { }
            // Register DotNet reference so client JS can invoke instance methods
            try
            {
                if (JSRuntime != null)
                {
                    var dotNetRef = DotNetObjectReference.Create(this);
                    await JSRuntime.InvokeVoidAsync("TalonStorageDB.registerDotNetRef", dotNetRef);
                    Console.WriteLine("OnAfterRenderAsync: Registered DotNetObjectReference with TalonStorageDB JS");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnAfterRenderAsync: Failed to register DotNet reference: " + ex.Message);
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
            await UpdateHasAnyDataAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EnsureDataIsLoadedForSearch error: {ex.Message}");
        }
    }

    private async Task UpdateHasAnyDataAsync()
    {
        try
        {
            if ((_allCommandsCache?.Count ?? 0) > 0)
            {
                HasAnyData = true;
                return;
            }
            if (JSRuntime != null)
            {
                var info = await JSRuntime.InvokeAsync<object>("TalonStorageDB.getStorageInfo");
                var json = System.Text.Json.JsonSerializer.Serialize(info);
                // If metadata says we have commands, trust it
                if (json.Contains("\"commands\":") && !json.Contains("\"commands\":{\"count\":0"))
                {
                    HasAnyData = true;
                }
                else
                {
                    // Fallback: directly count records in the commands store
                    try
                    {
                        var count = await JSRuntime.InvokeAsync<int>("TalonStorageDB.getCommandsCount");
                        HasAnyData = count > 0;
                    }
                    catch
                    {
                        HasAnyData = false;
                    }
                }
            }
        }
        catch
        {
            // ignore, keep previous value
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
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
        // Always clear InfoMessage at the start of a search
        InfoMessage = string.Empty;
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
                JavaScriptResultCount = resultCount; // Store the result count for display
                // If no results and an application filter is set, just show 'No results found.'
                if (resultCount == 0 && hasApplicationFilter)
                {
                    InfoMessage = $"No results found for application '{SelectedApplication}'.";
                }
                
                // Clear C# results since we're using JavaScript display
                Results = new List<TalonVoiceCommand>();
                IsUsingJavaScriptDisplay = true; // Set flag to indicate JavaScript display is active
                
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
                if (searchResults.Count == 0 && hasApplicationFilter)
                {
                    Console.WriteLine("OnSearch: Zero results with app filter (simple); retry without application filter...");
                    var fallback = await TalonService.SearchFilteredCommandsSimpleAsync(
                        searchTerm: hasSearchTerm ? SearchTerm : null,
                        application: null,
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
                    if (fallback.Count > 0)
                    {
                        searchResults = fallback;
                        InfoMessage = $"Showing {fallback.Count} results across all applications (no matches found for '{SelectedApplication}').";
                    }
                }
                Results = searchResults;
                ClearJavaScriptDisplay(); // Clear JavaScript display state
                
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
                    ClearJavaScriptDisplay(); // Clear JavaScript display state
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
                        ClearJavaScriptDisplay(); // Clear JavaScript display state
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
                ClearJavaScriptDisplay(); // Clear JavaScript display state
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
                ClearJavaScriptDisplay(); // Clear JavaScript display state
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
                ClearJavaScriptDisplay(); // Clear display state when clearing results
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
                            var normalizedTerm = SearchTerm.Trim().Trim('"', '\'', '“', '”');
                            searchResults = filteredCommands
                                .Where(c => !string.IsNullOrWhiteSpace(c.Command) && c.Command.Trim().Trim('"', '\'', '“', '”').Equals(normalizedTerm, StringComparison.OrdinalIgnoreCase))
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
            IsUsingJavaScriptDisplay = false; // Clear flag when clearing results
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
        ClearJavaScriptDisplay(); // Clear display state when clearing results
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
        ClearJavaScriptDisplay(); // Clear display state when clearing results
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

    // Opens the left-hand side panel showing the full list contents (loads from TalonService if needed)
    [Microsoft.JSInterop.JSInvokable("OpenListInSidePanel")]
    public async Task OpenListInSidePanel(string listName)
    {
        if (string.IsNullOrWhiteSpace(listName)) return;
        // If another request was made after this one, bail out early to avoid
        // overwriting the newer request. We still set the initial state below
        // so callers (including JS) can open the panel immediately.
        _selectedPanelListName = listName;
        _isListPanelOpen = true;
        _isListPanelLoading = true;
        StateHasChanged();

        try
        {
            // Try cache first
            if (_listContentsCache.TryGetValue(listName, out var cached))
            {
                _selectedPanelValues = cached;
            }
            else if (TalonService != null)
            {
                var contents = await TalonService.GetListContentsAsync(listName);
                _listContentsCache[listName] = contents;
                _selectedPanelValues = contents;
            }
            else
            {
                _selectedPanelValues = new List<TalonList>();
            }

            // If server-side lookup returned no items, attempt to fetch from client IndexedDB via JSRuntime as a fallback
            if ((_selectedPanelValues == null || _selectedPanelValues.Count == 0) && JSRuntime != null)
            {
                try
                {
                    var cts2 = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15));
                    var itemsElement = await JSRuntime.InvokeAsync<System.Text.Json.JsonElement>("TalonStorageDB.loadLists", cts2.Token);
                    if (itemsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var arr = itemsElement;

                        // Helper to normalize names like the client
                        string Normalize(string s) => (s ?? string.Empty).ToString().Trim().TrimStart('<', '{', '[').TrimEnd('>', '}', ']');
                        var baseName = Normalize(listName);
                        var alt = baseName.StartsWith("user.") ? baseName.Substring(5) : $"user.{baseName}";
                        var forms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { baseName, alt };

                        var matches = new List<TalonList>();
                        foreach (var el in arr.EnumerateArray())
                        {
                            if (el.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                            string ln = string.Empty;
                            if (el.TryGetProperty("ListName", out var p1) && p1.ValueKind == System.Text.Json.JsonValueKind.String)
                                ln = p1.GetString() ?? string.Empty;
                            else if (el.TryGetProperty("listName", out var p2) && p2.ValueKind == System.Text.Json.JsonValueKind.String)
                                ln = p2.GetString() ?? string.Empty;
                            else if (el.TryGetProperty("List", out var p3) && p3.ValueKind == System.Text.Json.JsonValueKind.String)
                                ln = p3.GetString() ?? string.Empty;

                            ln = Normalize(ln);
                            if (forms.Contains(ln))
                            {
                                // Map available properties into TalonList; be defensive about missing properties
                                string spoken = string.Empty;
                                if (el.TryGetProperty("SpokenForm", out var sp) && sp.ValueKind == System.Text.Json.JsonValueKind.String)
                                    spoken = sp.GetString() ?? string.Empty;
                                else if (el.TryGetProperty("spokenForm", out var sp2) && sp2.ValueKind == System.Text.Json.JsonValueKind.String)
                                    spoken = sp2.GetString() ?? string.Empty;

                                string listValue = string.Empty;
                                if (el.TryGetProperty("ListValue", out var lv) && lv.ValueKind == System.Text.Json.JsonValueKind.String)
                                    listValue = lv.GetString() ?? string.Empty;
                                else if (el.TryGetProperty("listValue", out var lv2) && lv2.ValueKind == System.Text.Json.JsonValueKind.String)
                                    listValue = lv2.GetString() ?? string.Empty;

                                var tal = new TalonList
                                {
                                    ListName = ln ?? string.Empty,
                                    SpokenForm = spoken,
                                    ListValue = listValue
                                };
                                matches.Add(tal);
                            }
                        }

                        if (matches.Any())
                        {
                            // If another request was made after this one, don't apply these
                            // results since they would be stale.
                            if (_lastRequestedPanelListName == listName)
                            {
                                _listContentsCache[listName] = matches;
                                _selectedPanelValues = matches;
                                Console.WriteLine($"[DEBUG] Populated panel from client IndexedDB fallback with {matches.Count} items for '{listName}'");
                            }
                            else
                            {
                                Console.WriteLine($"[DEBUG] Ignoring client IndexedDB fallback for '{listName}' because a newer request exists ({_lastRequestedPanelListName}).");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OpenListInSidePanel: client-side fallback failed: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenListInSidePanel error loading {listName}: {ex.Message}");
            _selectedPanelValues = new List<TalonList>();
        }
        finally
        {
            // Only clear the loading state if this is still the most recent request.
            if (_lastRequestedPanelListName == listName)
            {
                _isListPanelLoading = false;
            }
            else
            {
                // There's a newer request in progress; leave loading true for that request.
                Console.WriteLine($"OpenListInSidePanel: Not clearing loading state for '{listName}' because newer request '{_lastRequestedPanelListName}' exists.");
            }
            // Notify client JS for diagnostics (if available)
            try
            {
                var jsInfo = new { ListName = listName, Count = _selectedPanelValues?.Count ?? 0 };
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("TalonStorageDB._onPanelDataLoaded", jsInfo);
                }
            }
            catch { }

            await InvokeAsync(StateHasChanged);
        }
    }

    // Accept list contents from the client (IndexedDB) to populate the side panel when
    // the server-side cache hasn't been loaded from localStorage yet.
    [Microsoft.JSInterop.JSInvokable("OpenListInSidePanelWithClientData")]
    public async Task OpenListInSidePanelWithClientData(string listName, string itemsJson)
    {
        if (string.IsNullOrWhiteSpace(listName)) return;
        try
        {
            Console.WriteLine($"OpenListInSidePanelWithClientData invoked for '{listName}' (itemsJson length: {itemsJson?.Length ?? 0})");
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = System.Text.Json.JsonSerializer.Deserialize<List<TalonList>>(itemsJson ?? "[]", options) ?? new List<TalonList>();
            Console.WriteLine($"OpenListInSidePanelWithClientData: Deserialized {items.Count} items for '{listName}'");
            // Only apply these client-provided values if this request is still the most
            // recent one. This avoids race conditions where a user clicked another
            // list while the client was sending data for a prior click.
            if (_lastRequestedPanelListName == listName)
            {
                _listContentsCache[listName] = items;
                _selectedPanelListName = listName;
                _selectedPanelValues = items;
                _isListPanelOpen = true;
                _isListPanelLoading = false;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                Console.WriteLine($"OpenListInSidePanelWithClientData: Ignoring client data for '{listName}' because newer request '{_lastRequestedPanelListName}' exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenListInSidePanelWithClientData error: {ex.Message}");
        }
    }

    public async Task CloseListSidePanel()
    {
        _isListPanelOpen = false;
        _selectedPanelListName = null;
        _selectedPanelValues = new List<TalonList>();
        await InvokeAsync(StateHasChanged);
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

    /// <summary>
    /// Loads all distinct list names available in IndexedDB/local cache.
    /// Uses JSRuntime to call TalonStorageDB.loadLists when available for performance.
    /// Falls back to deriving list names from cached commands or list cache.
    /// </summary>
    public async Task LoadAllListNamesAsync()
    {
        if (_allListNames != null && _allListNames.Count > 0) return;
        _isListsLoading = true;
        _allListNames = new List<string>();
        try
        {
            // Prefer JS IndexedDB loader for large datasets
            if (JSRuntime != null)
            {
                try
                {
                    // Try the aggregated breakdown first (fast)
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    var breakdown = await JSRuntime.InvokeAsync<System.Text.Json.JsonElement>("TalonStorageDB.getListsBreakdown", cts.Token);
                    if (breakdown.ValueKind == System.Text.Json.JsonValueKind.Object && breakdown.TryGetProperty("perList", out var perList) && perList.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var el in perList.EnumerateArray())
                        {
                            if (el.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                            if (el.TryGetProperty("listName", out var ln) && ln.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var name = ln.GetString() ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                            }
                            else if (el.TryGetProperty("listName", out var ln2) && ln2.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var name = ln2.GetString() ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                            }
                        }
                        _allListNames = names.OrderBy(n => n).ToList();
                    }
                    else
                    {
                        // Fallback: load all records if breakdown isn't available
                        var items = await JSRuntime.InvokeAsync<System.Text.Json.JsonElement>("TalonStorageDB.loadLists", cts.Token);
                        if (items.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var el in items.EnumerateArray())
                            {
                                string name = string.Empty;
                                if (el.TryGetProperty("ListName", out var p1) && p1.ValueKind == System.Text.Json.JsonValueKind.String)
                                    name = p1.GetString() ?? string.Empty;
                                else if (el.TryGetProperty("listName", out var p2) && p2.ValueKind == System.Text.Json.JsonValueKind.String)
                                    name = p2.GetString() ?? string.Empty;
                                else if (el.TryGetProperty("List", out var p3) && p3.ValueKind == System.Text.Json.JsonValueKind.String)
                                    name = p3.GetString() ?? string.Empty;

                                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                            }
                            _allListNames = names.OrderBy(n => n).ToList();
                        }
                    }
                }
                catch (Exception jsEx)
                {
                    Console.WriteLine($"LoadAllListNamesAsync: JS load failed: {jsEx.Message}");
                }
            }

            // Fallback: derive from cached list contents
            if ((_allListNames == null || _allListNames.Count == 0) && _listContentsCache != null && _listContentsCache.Any())
            {
                _allListNames = _listContentsCache.Keys.OrderBy(k => k).ToList();
            }

            // Final fallback: derive from cached commands by scanning for list usages
            if ((_allListNames == null || _allListNames.Count == 0) && _allCommandsCache != null)
            {
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var cmd in _allCommandsCache)
                {
                    var used = GetListsUsedInCommand(cmd.Command);
                    foreach (var n in used) names.Add(n);
                }
                _allListNames = names.OrderBy(n => n).ToList();
            }

            // Ensure non-null
            if (_allListNames == null) _allListNames = new List<string>();
            Console.WriteLine($"LoadAllListNamesAsync: Loaded {_allListNames?.Count ?? 0} list names (sample: {string.Join(", ", (_allListNames ?? new List<string>()).Take(10))})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoadAllListNamesAsync error: {ex.Message}");
            _allListNames = new List<string>();
        }
        finally
        {
            _isListsLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected IEnumerable<string> FilteredListNames =>
        string.IsNullOrWhiteSpace(_listFilterTerm)
            ? (_allListNames ?? new List<string>())
            : (_allListNames ?? new List<string>()).Where(n => n != null && n.IndexOf(_listFilterTerm, StringComparison.OrdinalIgnoreCase) >= 0);

    public async Task OnListClickedAsync(string listName)
    {
        if (string.IsNullOrWhiteSpace(listName)) return;
        _highlightedListName = listName;

        // Mark this as the most recent request so any prior background loads will
        // be ignored if they complete later.
        _lastRequestedPanelListName = listName;

        // Immediately clear the previous panel contents and show a loading state
        // so users don't see the old list while the new one is being fetched.
        _isListPanelOpen = true;
        _isListPanelLoading = true;
        _selectedPanelListName = listName;
        _selectedPanelValues = new List<TalonList>();
        await InvokeAsync(StateHasChanged);

        // Fast-path: ask client for list items directly and render immediately
        if (JSRuntime != null)
        {
            try
            {
                // Try to get items for this list quickly from IndexedDB
                // Prefer the fixes-enabled helper so client-side normalization is applied
                string? itemsJson = null;
                try
                {
                    itemsJson = await JSRuntime.InvokeAsync<string>("TalonStorageDB.getListItemsJsonWithFixes", listName);
                }
                catch
                {
                    // Older clients may not have the new helper; fall back to the original
                    itemsJson = await JSRuntime.InvokeAsync<string>("TalonStorageDB.getListItemsJson", listName);
                }
                if (!string.IsNullOrWhiteSpace(itemsJson))
                {
                    try
                    {
                        // Call the server-invokable method to populate UI/server cache
                        await OpenListInSidePanelWithClientData(listName, itemsJson);

                        // Background-refresh the server's authoritative cache without
                        // blocking the UI. This ensures future operations have server cache.
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await OpenListInSidePanel(listName);
                            }
                            catch (Exception bgEx)
                            {
                                Console.WriteLine($"Background OpenListInSidePanel failed for '{listName}': {bgEx.Message}");
                            }
                        });

                        return; // fast path complete
                    }
                    catch (Exception inner)
                    {
                        Console.WriteLine($"OnListClickedAsync: fast-path JSON handling failed: {inner.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // If getListItems isn't available or fails, fall back to the older route
                Console.WriteLine($"OnListClickedAsync: fast-path JS call failed: {ex.Message}");
            }
        }

        // Slow path: client didn't populate panel immediately; load synchronously so
        // the user sees results as soon as available.
        await OpenListInSidePanel(listName);
    }
    public string GetListItemCount(string listName)
    {
        if (string.IsNullOrWhiteSpace(listName)) return "-";
        if (_listContentsCache.TryGetValue(listName, out var items)) return items?.Count.ToString() ?? "0";
        return "-";
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
    /// Highlights content in text based on active filters
    /// </summary>
    public string HighlightFilteredContent(string text, string filterType)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var encoded = System.Net.WebUtility.HtmlEncode(text);
        
        try
        {
            switch (filterType.ToLower())
            {
                case "application":
                    if (!string.IsNullOrWhiteSpace(SelectedApplication))
                    {
                        encoded = HighlightText(encoded, SelectedApplication, "highlight-application");
                    }
                    break;
                case "mode":
                    if (!string.IsNullOrWhiteSpace(SelectedMode))
                    {
                        encoded = HighlightText(encoded, SelectedMode, "highlight-mode");
                    }
                    break;
                case "tags":
                    if (!string.IsNullOrWhiteSpace(SelectedTags))
                    {
                        encoded = HighlightText(encoded, SelectedTags, "highlight-tags");
                    }
                    break;
                case "os":
                    if (!string.IsNullOrWhiteSpace(SelectedOperatingSystem))
                    {
                        encoded = HighlightText(encoded, SelectedOperatingSystem, "highlight-os");
                    }
                    break;
                case "repository":
                    if (!string.IsNullOrWhiteSpace(SelectedRepository))
                    {
                        encoded = HighlightText(encoded, SelectedRepository, "highlight-repository");
                    }
                    break;
                case "title":
                    if (!string.IsNullOrWhiteSpace(SelectedTitle))
                    {
                        encoded = HighlightText(encoded, SelectedTitle, "highlight-title");
                    }
                    break;
                case "codelanguage":
                    if (!string.IsNullOrWhiteSpace(SelectedCodeLanguage))
                    {
                        encoded = HighlightText(encoded, SelectedCodeLanguage, "highlight-code-language");
                    }
                    break;
            }
            return encoded;
        }
        catch
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }

    /// <summary>
    /// Helper method to highlight specific text with a CSS class
    /// </summary>
    private string HighlightText(string text, string searchText, string cssClass)
    {
        if (string.IsNullOrEmpty(searchText))
            return text;

        try
        {
            // Case-insensitive replacement
            var regex = new Regex(Regex.Escape(searchText), RegexOptions.IgnoreCase);
            return regex.Replace(text, match => 
                $"<span class=\"{cssClass}\">{match.Value}</span>");
        }
        catch
        {
            return text;
        }
    }

    /// <summary>
    /// Highlights multiple filter matches in a single text
    /// </summary>
    public string HighlightAllFilterMatches(string text, TalonVoiceCommand command)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = System.Net.WebUtility.HtmlEncode(text);

        try
        {
            // Highlight active filter matches
            if (!string.IsNullOrWhiteSpace(SelectedApplication) && 
                !string.IsNullOrWhiteSpace(command.Application) &&
                command.Application.Contains(SelectedApplication, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedApplication, "highlight-application");
            }

            if (!string.IsNullOrWhiteSpace(SelectedMode) && 
                !string.IsNullOrWhiteSpace(command.Mode) &&
                command.Mode.Contains(SelectedMode, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedMode, "highlight-mode");
            }

            if (!string.IsNullOrWhiteSpace(SelectedTags) && 
                !string.IsNullOrWhiteSpace(command.Tags) &&
                command.Tags.Contains(SelectedTags, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedTags, "highlight-tags");
            }

            if (!string.IsNullOrWhiteSpace(SelectedOperatingSystem) && 
                !string.IsNullOrWhiteSpace(command.OperatingSystem) &&
                command.OperatingSystem.Contains(SelectedOperatingSystem, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedOperatingSystem, "highlight-os");
            }

            if (!string.IsNullOrWhiteSpace(SelectedRepository) && 
                !string.IsNullOrWhiteSpace(command.Repository) &&
                command.Repository.Contains(SelectedRepository, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedRepository, "highlight-repository");
            }

            if (!string.IsNullOrWhiteSpace(SelectedTitle) && 
                !string.IsNullOrWhiteSpace(command.Title) &&
                command.Title.Contains(SelectedTitle, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedTitle, "highlight-title");
            }

            if (!string.IsNullOrWhiteSpace(SelectedCodeLanguage) && 
                !string.IsNullOrWhiteSpace(command.CodeLanguage) &&
                command.CodeLanguage.Contains(SelectedCodeLanguage, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightText(result, SelectedCodeLanguage, "highlight-code-language");
            }

            return result;
        }
        catch
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }

    /// <summary>
    /// Combines capture highlighting with filter highlighting for display
    /// </summary>
    public string HighlightCapturesAndFilters(string command, TalonVoiceCommand cmd)
    {
        if (string.IsNullOrEmpty(command))
            return command;

        // First apply capture highlighting (this already HTML encodes)
        var result = HighlightCapturesInCommand(command);

        try
        {
            // Then apply filter highlighting to the already-encoded result
            // Note: Since result is already HTML, we need to search for the original text patterns
            if (!string.IsNullOrWhiteSpace(SelectedApplication) && 
                !string.IsNullOrWhiteSpace(cmd.Application) &&
                command.Contains(SelectedApplication, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightTextInHtml(result, SelectedApplication, "highlight-application");
            }

            if (!string.IsNullOrWhiteSpace(SelectedMode) && 
                !string.IsNullOrWhiteSpace(cmd.Mode) &&
                command.Contains(SelectedMode, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightTextInHtml(result, SelectedMode, "highlight-mode");
            }

            if (!string.IsNullOrWhiteSpace(SelectedTags) && 
                !string.IsNullOrWhiteSpace(cmd.Tags) &&
                command.Contains(SelectedTags, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightTextInHtml(result, SelectedTags, "highlight-tags");
            }

            if (!string.IsNullOrWhiteSpace(SelectedRepository) && 
                !string.IsNullOrWhiteSpace(cmd.Repository) &&
                command.Contains(SelectedRepository, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightTextInHtml(result, SelectedRepository, "highlight-repository");
            }

            if (!string.IsNullOrWhiteSpace(SelectedTitle) && 
                !string.IsNullOrWhiteSpace(cmd.Title) &&
                command.Contains(SelectedTitle, StringComparison.OrdinalIgnoreCase))
            {
                result = HighlightTextInHtml(result, SelectedTitle, "highlight-title");
            }

            return result;
        }
        catch
        {
            return HighlightCapturesInCommand(command);
        }
    }

    /// <summary>
    /// Helper method to highlight text within already-HTML content
    /// </summary>
    private string HighlightTextInHtml(string html, string searchText, string cssClass)
    {
        if (string.IsNullOrEmpty(searchText))
            return html;

        try
        {
            // HTML encode the search text to match what's in the HTML
            var encodedSearchText = System.Net.WebUtility.HtmlEncode(searchText);
            
            // Case-insensitive replacement in HTML content
            var regex = new Regex(Regex.Escape(encodedSearchText), RegexOptions.IgnoreCase);
            return regex.Replace(html, match => 
                $"<span class=\"{cssClass}\">{match.Value}</span>");
        }
        catch
        {
            return html;
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
        // If switching to Lists tab, load list names asynchronously
        if (tab == TabType.Lists)
        {
            _ = LoadAllListNamesAsync();
            // Schedule focus into the lists search box so keyboard is ready
            try
            {
                if (JSRuntime != null)
                {
                    _ = InvokeAsync(async () =>
                    {
                        try
                        {
                            // small delay to allow DOM update
                            await Task.Delay(50);
                            await JSRuntime.InvokeVoidAsync("eval", "(function(){const el=document.querySelector('input[placeholder=\"Search lists...\"]'); if(el){el.focus(); el.select();}})()");
                        }
                        catch { }
                    });
                }
            }
            catch { }
        }
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
                    case "4":
                        SwitchTab(TabType.Lists);
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
        // Unsubscribe from filter refresh event
        if (FilterRefreshService != null)
        {
            FilterRefreshService.OnRefreshRequested -= RefreshFiltersAsync;
        }
        // Attempt to unregister DotNetRef from JS
        try
        {
            if (JSRuntime != null)
            {
                _ = JSRuntime.InvokeVoidAsync("(function(){ if(window.TalonStorageDB && window.TalonStorageDB._dotNetRef){ window.TalonStorageDB._dotNetRef = null; console.log('TalonStorageDB: DotNet reference cleared'); } })");
            }
        }
        catch { }
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
