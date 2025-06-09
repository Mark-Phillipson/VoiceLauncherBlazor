using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading; // Added for Timer

namespace RazorClassLibrary.Pages
{
    public partial class AccessibleLauncherList : ComponentBase, IAsyncDisposable
    {
        [Inject] public required ILauncherDataService LauncherDataService { get; set; }
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] public required ILogger<AccessibleLauncherList> Logger { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; } // Added NavigationManager

        public List<LauncherDTO>? Launchers { get; set; }
        public LauncherDTO? SelectedLauncher { get; set; }
        public int SelectedIndex { get; set; } = -1;
        public string? PreviewUrl { get; set; } // This might be re-evaluated based on iframe limitations
        private bool _loadFailed = false;
        private string _errorMessage = string.Empty;

        private ElementReference listContainer;

        // Dwell to click state
        private Timer? _dwellTimer;
        private int _secondsDwelled = 0;
        private const int MaxDwellSeconds = 10;
        private string _dwellStatusMessage = string.Empty;
        private LauncherDTO? _itemBeingDwelledOn = null; // Tracks the item for which the timer is active

        protected override async Task OnInitializedAsync()
        {
            await LoadLaunchers();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (Launchers != null && Launchers.Any()) // Only focus if there's content
                    {
                        await listContainer.FocusAsync();
                    }
                }
                catch (JSException ex)
                {
                    Logger.LogWarning(ex, "Failed to focus list container on initial render.");
                }
            }
        }

        private async Task LoadLaunchers()
        {
            _loadFailed = false;
            _errorMessage = string.Empty;
            Launchers = null;
            // Stop any existing timer if data is reloaded
            _dwellTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _dwellTimer?.Dispose();
            _dwellTimer = null;
            _secondsDwelled = 0;
            _itemBeingDwelledOn = null;
            _dwellStatusMessage = string.Empty;
            StateHasChanged();
            try
            {
                var result = await LauncherDataService.GetFavoriteLaunchersAsync();
                if (result != null)
                {
                    Launchers = result
                        .Where(l => l.Favourite &&
                                    l.CommandLine != null &&
                                    l.CommandLine.Trim().ToLower().StartsWith("http"))
                        .OrderBy(l => l.SortOrder)
                        .ThenBy(l => l.Name)
                        .ToList();
                }
                else
                {
                    Launchers = new List<LauncherDTO>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load launchers.");
                _loadFailed = true;
                _errorMessage = ex.Message;
                Launchers = new List<LauncherDTO>();
            }
            StateHasChanged();
            if (!_loadFailed && (Launchers?.Any() ?? false)) {
                 try { 
                     await listContainer.FocusAsync(); 
                     // If you want to auto-select the first item:
                     // SelectItem(0, true); // Select and scroll to the first item
                 } catch (Exception ex) {
                     Logger.LogWarning(ex, "Failed to focus list container after loading.");
                 }
            }
        }

        protected void HandleKeyDown(KeyboardEventArgs e)
        {
            if (Launchers == null || !Launchers.Any()) return;

            bool selectionChanged = false;
            if (e.Key == "ArrowDown")
            {
                SelectedIndex = (SelectedIndex + 1) % Launchers.Count;
                selectionChanged = true;
            }
            else if (e.Key == "ArrowUp")
            {
                SelectedIndex = (SelectedIndex - 1 + Launchers.Count) % Launchers.Count;
                selectionChanged = true;
            }

            if (selectionChanged)
            {
                SelectItem(SelectedIndex, true);
            }
        }

        private void SelectItem(int index, bool scroll = true)
        {
            // Always stop and dispose any existing timer first
            _dwellTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _dwellTimer?.Dispose();
            _dwellTimer = null;
            _secondsDwelled = 0;
            _itemBeingDwelledOn = null;
            // PreviewUrl related logic might change if not using iframe
            // For now, we'll keep it simple for the dwell logic

            if (Launchers == null || index < 0 || index >= Launchers.Count)
            {
                SelectedIndex = -1;
                SelectedLauncher = null;
                PreviewUrl = null; 
                _dwellStatusMessage = "No item selected.";
            }
            else
            {
                SelectedIndex = index;
                SelectedLauncher = Launchers[index];
                PreviewUrl = SelectedLauncher.CommandLine; // Assuming it's a web link due to LoadLaunchers filter

                if (SelectedLauncher.CommandLine != null && SelectedLauncher.CommandLine.Trim().ToLower().StartsWith("http"))
                {
                    _itemBeingDwelledOn = SelectedLauncher;
                    _dwellStatusMessage = $"Selected: {SelectedLauncher.Name}. Hold selection for {MaxDwellSeconds}s to launch.";
                    // Start new timer
                    _dwellTimer = new Timer(DwellTimerCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                }
                else
                {
                    // This case should ideally not be hit if LoadLaunchers filters correctly
                    _dwellStatusMessage = $"Selected: {SelectedLauncher.Name}. (Not a web page)";
                }
            }
            InvokeAsync(StateHasChanged);

            if (scroll && SelectedIndex >= 0)
            {
                _ = ScrollToSelectedItemJs();
            }
        }
        
        private async void DwellTimerCallback(object? state)
        {
            await InvokeAsync(async () => // Ensure execution on the Blazor synchronization context and mark lambda as async
            {
                // Check if the timer should still be running for the current selection
                if (SelectedLauncher == null || _itemBeingDwelledOn == null || SelectedLauncher.Id != _itemBeingDwelledOn.Id)
                {
                    _dwellTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _dwellTimer?.Dispose();
                    _dwellTimer = null;
                    _secondsDwelled = 0;
                    // Status message will be updated by the next SelectItem call or cleared
                    return;
                }

                _secondsDwelled++;

                if (_secondsDwelled >= MaxDwellSeconds)
                {
                    _dwellTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _dwellTimer?.Dispose();
                    _dwellTimer = null;
                    
                    if (SelectedLauncher.CommandLine != null) // Redundant check given LoadLaunchers filter, but safe
                    {
                        string urlToLaunch = SelectedLauncher.CommandLine;
                        _dwellStatusMessage = $"Launching {SelectedLauncher.Name}...";
                        StateHasChanged(); // Update UI to show "Launching..."

                        // Use JSRuntime to open in a new tab
                        if (JSRuntime != null)
                        {
                            await JSRuntime.InvokeVoidAsync("open", urlToLaunch, "_blank");
                        }
                        else
                        {
                            // Fallback or log error if JSRuntime is unexpectedly null
                            Logger.LogError("JSRuntime is null, cannot open URL in new tab via JS.");
                            NavigationManager.NavigateTo(urlToLaunch, forceLoad: true); 
                        }
                    }
                    // Reset after launch attempt
                    _secondsDwelled = 0;
                    _itemBeingDwelledOn = null; 
                }
                else
                {
                    if (SelectedLauncher != null) // Check SelectedLauncher is not null before accessing Name
                    {
                        _dwellStatusMessage = $"Launching {SelectedLauncher.Name} in {MaxDwellSeconds - _secondsDwelled} seconds...";
                    }
                }
                StateHasChanged(); // Update UI with countdown or launching message
            });
        }

        private async Task ScrollToSelectedItemJs()
        {
            if (SelectedIndex >= 0 && JSRuntime != null)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('launcher-item-{SelectedIndex}')?.scrollIntoView({{ behavior: 'smooth', block: 'nearest' }});");
                }
                catch (JSException ex)
                {
                    Logger.LogWarning(ex, $"Failed to scroll to item launcher-item-{SelectedIndex}.");
                }
            }
        }
        
        public ValueTask DisposeAsync()
        {
            _dwellTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _dwellTimer?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
