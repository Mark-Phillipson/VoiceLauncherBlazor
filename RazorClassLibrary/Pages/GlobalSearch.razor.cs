using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Timers;

namespace RazorClassLibrary.Pages
{
    public partial class GlobalSearch : ComponentBase, IDisposable
    {
        [Inject] public required IJSRuntime JSRuntime { get; set; }
          private string _inputValue = "";
        private string searchTerm = "";
        private System.Timers.Timer? _debounceTimer;
        private System.Timers.Timer? _countdownTimer;        private const int DebounceDelay = 2000; // 1 second delay
        private int _remainingMs = 0;
        private bool _isCountingDown = false;
        
        // Property to expose DebounceDelay to Razor markup
        public int DebounceDelayMs => DebounceDelay;
        
        private string InputValue
        {
            get => _inputValue;
            set
            {
                _inputValue = value;
                OnInputChanged();
            }
        }
          private void OnInputChanged()
        {
            // Stop existing timers
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();
            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();
            
            // Reset countdown state
            _remainingMs = DebounceDelay;
            _isCountingDown = !string.IsNullOrWhiteSpace(_inputValue);
            
            if (_isCountingDown)
            {
                // Start countdown timer (updates every 100ms for smooth visual feedback)
                _countdownTimer = new System.Timers.Timer(100);
                _countdownTimer.Elapsed += OnCountdownTick;
                _countdownTimer.AutoReset = true;
                _countdownTimer.Start();
            }
            
            // Start debounce timer
            _debounceTimer = new System.Timers.Timer(DebounceDelay);
            _debounceTimer.Elapsed += OnTimerElapsed;
            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();
            
            StateHasChanged();
        }
          private void OnCountdownTick(object? sender, ElapsedEventArgs e)
        {
            _remainingMs -= 100;
            if (_remainingMs <= 0)
            {
                _isCountingDown = false;
                _countdownTimer?.Stop();
                _countdownTimer?.Dispose();
            }
            
            InvokeAsync(StateHasChanged);
        }
        
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            InvokeAsync(() =>
            {
                _isCountingDown = false;
                searchTerm = _inputValue;
                StateHasChanged();
            });
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
                 {
                     if (firstRender)
                     {
                         try
                         {
                             if (JSRuntime != null)
                             {
                                 await JSRuntime.InvokeVoidAsync("window.setFocus", "searchTerm");
                             }
                         }
                         catch (Exception exception)
                         {
                             Console.WriteLine(exception.Message);
                         }                     }
                 }
          public void Dispose()
        {
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();
            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();
        }
    }
}