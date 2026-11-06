using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace RazorClassLibrary.Components
{
    public partial class DebounceInput : ComponentBase, IDisposable
    {
        private CancellationTokenSource? _cts;
        private string CurrentValue { get; set; } = string.Empty;
        private string? _lastParentValue;
        private int _lastResetVersion;
        [Parameter] public int ResetVersion { get; set; }
        [Parameter] public string? Id { get; set; }
        [Parameter] public string? Accesskey { get; set; }
        [Parameter] public string Autocomplete { get; set; } = "off";
        [Parameter] public string CssClass { get; set; } = "form-control selection-filter";
        [Parameter] public string Placeholder { get; set; } = string.Empty;
        [Parameter] public string AriaLabel { get; set; } = "Filter items";
        [Parameter] public string Value { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> ValueChanged { get; set; }
        [Parameter] public EventCallback<string> OnDebounced { get; set; }
        [Parameter] public int DebounceMilliseconds { get; set; } = 300;
        [Parameter] public bool ImmediateOnEnter { get; set; } = true;

        protected override void OnParametersSet()
        {
            var incomingValue = Value ?? string.Empty;

            var valueChanged = !string.Equals(
                incomingValue,
                _lastParentValue,
                StringComparison.Ordinal);

            var resetRequested = ResetVersion != _lastResetVersion;

            if (valueChanged || resetRequested)
            {
                CurrentValue = incomingValue;
            }

            _lastParentValue = incomingValue;
            _lastResetVersion = ResetVersion;
        }

        private async Task OnInput(ChangeEventArgs e)
        {
            CurrentValue = e?.Value?.ToString() ?? string.Empty;
            var inputValue = CurrentValue;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var token = _cts.Token;

            try
            {
                await Task.Delay(DebounceMilliseconds, token);

                if (!token.IsCancellationRequested)
                {
                    if (ValueChanged.HasDelegate)
                        await ValueChanged.InvokeAsync(inputValue);

                    if (OnDebounced.HasDelegate)
                        await OnDebounced.InvokeAsync(inputValue);
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (ImmediateOnEnter && (e.Key == "Enter"))
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                if (ValueChanged.HasDelegate)
                {
                    await ValueChanged.InvokeAsync(CurrentValue);
                }
                if (OnDebounced.HasDelegate)
                {
                    await OnDebounced.InvokeAsync(CurrentValue);
                }
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
