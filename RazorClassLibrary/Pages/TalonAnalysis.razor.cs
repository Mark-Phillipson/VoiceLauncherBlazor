using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Models;
using RazorClassLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public partial class TalonAnalysis : ComponentBase    {
        [Inject] private ITalonAnalysisService TalonAnalysisService { get; set; } = default!;
        [Inject] private ILogger<TalonAnalysis> Logger { get; set; } = default!;

        private TalonAnalysisResult? analysisResult;
        private bool isLoading = false;
        private string? errorMessage;
        private bool isHybridMode = false;        protected override async Task OnInitializedAsync()
        {
            await DetectHybridModeAsync();
            await LoadAnalysisAsync();
        }

        private async Task DetectHybridModeAsync()
        {
            try
            {
                // Check if we're running in a WebView (Blazor Hybrid)
                isHybridMode = await JSRuntime.InvokeAsync<bool>("eval", 
                    "window.chrome && window.chrome.webview != null");
            }
            catch
            {
                // If detection fails, default to false (server mode)
                isHybridMode = false;
            }
        }protected override Task OnAfterRenderAsync(bool firstRender)
        {
            // Chart initialization is now handled in LoadAnalysisAsync after data is loaded
            return Task.CompletedTask;
        }private async Task LoadAnalysisAsync()
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                StateHasChanged();

                Logger.LogInformation("Starting Talon command analysis");
                
                // Add a small delay to show the loading spinner
                await Task.Delay(100);
                analysisResult = await TalonAnalysisService.AnalyzeCommandsAsync();
                
                Logger.LogInformation("Talon command analysis completed successfully");
                  // Initialize chart after data is loaded and UI is updated
                StateHasChanged();
                await Task.Delay(1500); // Give more time for UI to render
                
                if (analysisResult?.ApplicationStats != null && analysisResult.ApplicationStats.Any())
                {
                    await InitializeApplicationChartAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during Talon command analysis");
                errorMessage = $"Error analyzing commands: {ex.Message}";
                analysisResult = null;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }        private async Task RefreshAnalysisAsync()
        {
            await LoadAnalysisAsync();
        }private async Task InitializeApplicationChartAsync()
        {
            try
            {
                Logger.LogInformation("Attempting to initialize application chart");
                
                if (analysisResult?.ApplicationStats == null)
                {
                    Logger.LogWarning("No analysis result or application stats available");
                    return;
                }

                if (!analysisResult.ApplicationStats.Any())
                {
                    Logger.LogWarning("Application stats collection is empty");
                    return;
                }                Logger.LogInformation("Found {Count} applications for chart", analysisResult.ApplicationStats.Count);
                Logger.LogInformation("Application stats list: {Stats}", 
                    string.Join(", ", analysisResult.ApplicationStats.Select(a => $"{a.Application}:{a.CommandCount}")));
                
                var chartData = analysisResult.ApplicationStats.Select(a => new 
                { 
                    application = a.Application, 
                    commandCount = a.CommandCount, 
                    percentage = a.Percentage 
                }).ToArray();
                
                Logger.LogInformation("Calling JavaScript initApplicationChart with {Count} data points", chartData.Length);
                Logger.LogInformation("Chart data: {Data}", System.Text.Json.JsonSerializer.Serialize(chartData.Take(5)));
                Logger.LogInformation("Full chart data being sent: {FullData}", System.Text.Json.JsonSerializer.Serialize(chartData));
                
                // Pass as explicit array to ensure proper serialization
                await JSRuntime.InvokeVoidAsync("initApplicationChart", (object)chartData);
                Logger.LogInformation("Successfully called initApplicationChart");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize application chart");
            }
        }
    }
}
