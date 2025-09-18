
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalonVoiceCommandsServer.Models;

namespace TalonVoiceCommandsServer.Components.Shared
{
    public partial class ListSidePanel : ComponentBase, IDisposable
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public string? SelectedListName { get; set; }
        [Parameter] public List<TalonList>? Values { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnCopyRequested { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private ElementReference filterInputRef;
    private bool _wasOpen = false;

        // Live filter term: log changes and re-render immediately so we can diagnose behavior.
        private string _filterTerm = string.Empty;
        public string FilterTerm
        {
            get => _filterTerm;
            set
            {
                if (_filterTerm != value)
                {
                    _filterTerm = value ?? string.Empty;
                    try { System.Console.WriteLine($"ListSidePanel: FilterTerm set to '{_filterTerm}'"); } catch { }
                    // Ensure UI updates when the term changes (Blazor Server roundtrip)
                    _ = InvokeAsync(StateHasChanged);
                }
            }
        }

        // FilteredValues uses FilterTerm so typing filters live.
        protected List<TalonList> FilteredValues =>
            string.IsNullOrWhiteSpace(FilterTerm) || Values == null
                ? Values ?? new List<TalonList>()
                : Values.Where(v =>
                    (!string.IsNullOrEmpty(v.SpokenForm) && v.SpokenForm.Contains(FilterTerm, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(v.ListValue) && v.ListValue.Contains(FilterTerm, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();

        protected async Task Close()
        {
            if (OnClose.HasDelegate) await OnClose.InvokeAsync();
            // Detach client-side filter when closing
            try
            {
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("listSideFilter.detach", "#list-side-panel", ".list-filter-input");
                    }
            }
            catch { }
        }

        protected async Task OnCopy()
        {
            if (OnCopyRequested.HasDelegate) await OnCopyRequested.InvokeAsync();
            else if (Values != null)
            {
                var csv = string.Join("\n", Values.Select(v => $"\"{v.SpokenForm}\",\"{v.ListValue}\""));
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", csv);
            }
        }

        protected async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Escape")
            {
                await Close();
            }
        }

        protected async Task OnFilterKeyDown(KeyboardEventArgs e)
        {
            // When Enter is pressed in the filter box, force an immediate UI update so
            // the filtered list is recalculated and displayed. We handle key suppression
            // in the Razor markup to prevent parent form submission.
            if (e.Key == "Enter")
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task OnFilterKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                // Apply current FilterTerm immediately and update UI
                await InvokeAsync(StateHasChanged);
            }
        }

        // OnFilterInput removed; debounce now handled in FilterTerm setter and _appliedFilterTerm

        public void Dispose()
        {
            // nothing to dispose currently
        }

        protected override async Task OnParametersSetAsync()
        {
            // If the panel just opened, focus the filter input so keyboard users can type immediately
            if (IsOpen && !_wasOpen)
            {
                try
                {
                    // small delay to allow DOM to render
                    await Task.Delay(30);
                    await filterInputRef.FocusAsync();
                    // Attach client-side filter for instant response
                    try
                    {
                        if (JSRuntime != null)
                        {
                            // Try to import the module (if served as module), otherwise ensure script tag exists
                            try
                            {
                                // If the file exports nothing, import returns a module object; still fine
                                await JSRuntime.InvokeAsync<object>("import", "/js/list-side-filter.js");
                            }
                            catch
                            {
                                // Fallback: inject the script tag if not available as module
                                try
                                {
                                    await JSRuntime.InvokeVoidAsync("eval", "(function(){ if(!window.listSideFilter){ var s=document.createElement('script'); s.src='/js/list-side-filter.js'; document.head.appendChild(s); } })()");
                                }
                                catch { }
                            }

                            // Finally call attach (no-op if function not yet loaded)
                            await JSRuntime.InvokeVoidAsync("listSideFilter.attach", "#list-side-panel", ".list-filter-input", ".table-scroll-table tbody tr");
                        }
                    }
                    catch (Exception ex)
                    {
                        try { await JSRuntime.InvokeVoidAsync("console.error", "list-side-filter attach error:", ex.Message); } catch { }
                    }
                }
                catch { }
            }
            _wasOpen = IsOpen;
            await base.OnParametersSetAsync();
        }
    }
}
