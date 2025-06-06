﻿using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Pages
{
    public partial class MultipleLaunchers : ComponentBase
    {
        [Inject] public required LauncherService LauncherService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public required IJSRuntime JSRuntime { get; set; }

        public bool ShowDialog { get; set; }
        public int MultipleLauncherIdDelete { get; set; }
        public string? SearchTerm { get; set; }
        private List<DataAccessLibrary.Models.MultipleLauncher>? multipleLaunchers;
        private List<DataAccessLibrary.Models.Launcher>? launchers;
        private DataAccessLibrary.Models.MultipleLauncher multipleLauncher = new();
        private DataAccessLibrary.Models.LauncherMultipleLauncherBridge bridge = new();
        public string? StatusMessage { get; set; }
        public string? StatusClassName { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414
        private string? LauncherFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
                launchers = await LauncherService.GetLaunchersAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }

        }
        private async Task DeleteBridge(DataAccessLibrary.Models.LauncherMultipleLauncherBridge bridge)
        {
            StatusMessage = await LauncherService.DeleteBridge(bridge);
            StatusClassName = "text-danger";
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
        }
        private void CreateBridge()
        {
            NavigationManager.NavigateTo($"/multiplelauncherbridge");
        }
        private async Task ReloadLaunchers()
        {
            launchers = await LauncherService.GetLaunchersAsync(LauncherFilter ?? "");
            await CallChangeAsync("FilterLauncher");
            await JSRuntime.InvokeVoidAsync("setFocus", "NewbridgeSelect ");
        }
        private async Task SaveBridge()
        {
            StatusMessage = await LauncherService.SaveBridge(bridge);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
            LauncherFilter = null;
            bridge = new DataAccessLibrary.Models.LauncherMultipleLauncherBridge();
        }
        private async Task SaveMultipleLauncher()
        {
            StatusMessage = await LauncherService.SaveMultipleLauncher(multipleLauncher);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
            StatusClassName = "text-success";
            multipleLauncher = new DataAccessLibrary.Models.MultipleLauncher();
        }
        private async Task SaveMultipleLaunchers()
        {
            if (multipleLaunchers != null)
            {
                multipleLaunchers = await LauncherService.SaveAllMultipleLauncher(multipleLaunchers);
            }
        }
        private async Task DeleteMultipleLauncher(int multipleLauncherId)
        {
            StatusMessage = await LauncherService.DeleteMultipleLauncher(multipleLauncherId);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
        }
        private async Task ApplyFilter()
        {
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm ?? "");
        }
        private async Task CallChangeAsync(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("CallChange", elementId);
        }

    }
}
