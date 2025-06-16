using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Models;
using RazorClassLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public partial class TalonAnalysis : ComponentBase
    {
        [Inject] private ITalonAnalysisService TalonAnalysisService { get; set; } = default!;
        [Inject] private ILogger<TalonAnalysis> Logger { get; set; } = default!;

        private TalonAnalysisResult? analysisResult;
        private bool isLoading = false;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadAnalysisAsync();
        }

        private async Task LoadAnalysisAsync()
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
        }

        private async Task RefreshAnalysisAsync()
        {
            await LoadAnalysisAsync();
        }
    }
}
