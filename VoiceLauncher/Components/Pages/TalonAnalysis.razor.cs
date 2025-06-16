using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VoiceLauncher.Models;
using VoiceLauncher.Services;

namespace VoiceLauncher.Components.Pages
{
    public partial class TalonAnalysis
    {
        [Inject] private ITalonAnalysisService TalonAnalysisService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        private TalonAnalysisResult? analysisResult;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadAnalysisData();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && analysisResult != null)
            {
                await InitializeChart();
            }
        }

        private async Task LoadAnalysisData()
        {
            isLoading = true;
            StateHasChanged();

            try
            {
                analysisResult = await TalonAnalysisService.AnalyzeCommandsAsync();
            }
            catch (Exception ex)
            {
                // Log error - for now just set result to null
                analysisResult = null;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task InitializeChart()
        {
            if (analysisResult?.RepositoryStats.Any() == true)
            {
                var chartData = analysisResult.RepositoryStats.Take(8).Select(r => new
                {
                    repository = r.Repository,
                    commandCount = r.CommandCount
                }).ToArray();

                await JSRuntime.InvokeVoidAsync("initRepositoryChart", chartData);
            }
        }

        private async Task RefreshAnalysis()
        {
            await LoadAnalysisData();
            if (analysisResult != null)
            {
                await InitializeChart();
            }
        }
    }
}
