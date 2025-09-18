
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalonVoiceCommandsServer.Models;

namespace TalonVoiceCommandsServer.Components.Shared
{
    public partial class ListSidePanel : ComponentBase
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

        private string _filterTerm = string.Empty;
        public string FilterTerm
        {
            get => _filterTerm;
            set
            {
                if (_filterTerm != value)
                {
                    _filterTerm = value;
                    InvokeAsync(StateHasChanged);
                }
            }
        }

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
                }
                catch { }
            }
            _wasOpen = IsOpen;
            await base.OnParametersSetAsync();
        }
    }
}
