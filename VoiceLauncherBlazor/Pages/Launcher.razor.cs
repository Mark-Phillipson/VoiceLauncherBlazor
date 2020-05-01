using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Launcher
    {
        [Parameter] public int launcherId { get; set; }
        private EditContext _editContext;
        public DataAccessLibrary.Models.Launcher launcher { get; set; }
        public List<DataAccessLibrary.Models.Category> categories { get; set; }
        public List<DataAccessLibrary.Models.Computer> computers { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414

        protected override async Task OnInitializedAsync()
        {
            if (launcherId > 0)
            {
                try
                {
                    launcher = await LauncherService.GetLauncherAsync(launcherId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            else
            {
                launcher = new DataAccessLibrary.Models.Launcher
                {
                    CommandLine = "http://"
                };
            }
            _editContext = new EditContext(launcher);
            try
            {
                categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
                computers = await ComputerService.GetComputersAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (launcherId > 0)
            {
                try
                {
                    launcher = await LauncherService.GetLauncherAsync(launcherId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }

        private async Task HandleValidSubmit()
        {
            try
            {
                var result = await LauncherService.SaveLauncher(launcher);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            NavigationManager.NavigateTo("launchers");
        }

    }
}
