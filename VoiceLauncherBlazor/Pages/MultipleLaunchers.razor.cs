using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
    public partial class MultipleLaunchers
    {
        public bool ShowDialog { get; set; }
        public int MultipleLauncherIdDelete { get; set; }
        public string SearchTerm { get; set; }
        private List<VoiceLauncherBlazor.Models.MultipleLauncher> multipleLaunchers;
        private List<VoiceLauncherBlazor.Models.Launcher> launchers;
        private VoiceLauncherBlazor.Models.MultipleLauncher multipleLauncher = new Models.MultipleLauncher();
        private VoiceLauncherBlazor.Models.LauncherMultipleLauncherBridge bridge = new Models.LauncherMultipleLauncherBridge();
        public string StatusMessage { get; set; }
        public string StatusClassName { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414
        private string LauncherFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
                launchers = await LauncherService.GetLaunchersAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }

        }
        private async Task DeleteBridge(VoiceLauncherBlazor.Models.LauncherMultipleLauncherBridge bridge)
        {
            StatusMessage = await LauncherService.DeleteBridge(bridge);
            StatusClassName = "text-danger";
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
        }
        private async Task CreateBridge(VoiceLauncherBlazor.Models.MultipleLauncher multipleLauncher)
        {
            bridge = new Models.LauncherMultipleLauncherBridge { MultipleLauncherId = multipleLauncher.Id };
            await JSRuntime.InvokeVoidAsync("setFocus", "FilterLauncher");
        }
        private async Task ReloadLaunchers()
        {
            launchers = await LauncherService.GetLaunchersAsync(LauncherFilter);
            await CallChangeAsync("FilterLauncher");
            await JSRuntime.InvokeVoidAsync("setFocus", "NewbridgeSelect ");
        }
        private async Task SaveBridge()
        {
            StatusMessage = await LauncherService.SaveBridge(bridge);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
            LauncherFilter = null;
            bridge = new Models.LauncherMultipleLauncherBridge();
        }
        private async Task SaveMultipleLauncher()
        {
            StatusMessage = await LauncherService.SaveMultipleLauncher(multipleLauncher);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
            StatusClassName = "text-success";
            multipleLauncher = new Models.MultipleLauncher();
        }
        private async Task SaveMultipleLaunchers()
        {
            multipleLaunchers = await LauncherService.SaveAllMultipleLauncher(multipleLaunchers);
        }
        private async Task DeleteMultipleLauncher(int multipleLauncherId)
        {
            StatusMessage = await LauncherService.DeleteMultipleLauncher(multipleLauncherId);
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
        }
        private async Task ApplyFilter()
        {
            multipleLaunchers = await LauncherService.GetMultipleLaunchersAsync(SearchTerm);
        }
        private async Task CallChangeAsync(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("CallChange", elementId);
        }

    }
}
