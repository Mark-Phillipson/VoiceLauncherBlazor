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

        [Parameter] public string? Id { get; set; }
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
            CurrentValue = Value ?? string.Empty;
        }

        private async Task OnInput(ChangeEventArgs e)
        {
            CurrentValue = e?.Value?.ToString() ?? string.Empty;
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
                    {
                        await ValueChanged.InvokeAsync(CurrentValue);
                    }
                    if (OnDebounced.HasDelegate)
                    {
                        await OnDebounced.InvokeAsync(CurrentValue);
                    }
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
