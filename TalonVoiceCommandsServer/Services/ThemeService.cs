using Microsoft.JSInterop;

namespace TalonVoiceCommandsServer.Services
{
    /// <summary>
    /// Lightweight theme service to store/read theme and toggle via JS interop.
    /// Uses localStorage key "tvc-theme" and sets document element attribute data-bs-theme.
    /// </summary>
    public class ThemeService
    {
        private const string ThemeKey = "tvc-theme";
        private readonly IJSRuntime _js;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetThemeAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("tvcTheme.getTheme");
            }
            catch
            {
                return null;
            }
        }

        public async Task SetThemeAsync(string theme)
        {
            await _js.InvokeVoidAsync("tvcTheme.setTheme", theme);
        }

        public async Task ToggleThemeAsync()
        {
            await _js.InvokeVoidAsync("tvcTheme.toggleTheme");
        }

        /// <summary>
        /// Toggle theme and return the new theme value from JS interop ("dark" or "light").
        /// </summary>
        public async Task<string?> ToggleThemeAndGetAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("tvcTheme.toggleTheme");
            }
            catch
            {
                return null;
            }
        }
    }
}
