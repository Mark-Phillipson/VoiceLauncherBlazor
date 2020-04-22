using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Launchers
    {
        [Parameter] public int? categoryIdFilter { get; set; }
        public bool ShowDialog { get; set; }
        private int launcherIdDelete { get; set; }
        private List<VoiceLauncherBlazor.Models.Launcher> launchers;
        public string StatusMessage { get; set; }
        public List<VoiceLauncherBlazor.Models.Category> categories { get; set; }
        public List<VoiceLauncherBlazor.Models.Computer> computers { get; set; }
        public int MaximumRows { get; set; } = 15;
        public bool ShowCreateNewOrEdit { get; set; }
        private int _launcherId;
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414
        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm != value)
                {
                    searchTerm = value;
                }
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (categoryIdFilter != null)
            {
                await FilterByCategory();
                try
                {
                    categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            else
            {
                try
                {
                    _loadFailed = false;
                    launchers = await LauncherService.GetLaunchersAsync(maximumRows: MaximumRows);
                    categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }

        async Task ApplyFilter()
        {
            if (SearchTerm != null)
            {
                try
                {
                    launchers = await LauncherService.GetLaunchersAsync(SearchTerm.Trim(), maximumRows: MaximumRows);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
                StateHasChanged();
            }
        }

        void ConfirmDelete(int launcherId)
        {
            ShowDialog = true;
            launcherIdDelete = launcherId;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task SortLaunchers(string column, string sortType)
        {
            try
            {
                launchers = await LauncherService.GetLaunchersAsync(searchTerm, column, sortType, maximumRows: MaximumRows);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteLauncher(int launcherId)
        {
            try
            {
                var result = await LauncherService.DeleteLauncher(launcherId);
                StatusMessage = result;
                ShowDialog = false;
                launchers = await LauncherService.GetLaunchersAsync(searchTerm, maximumRows: MaximumRows);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        async Task SaveAllLaunchers()
        {
            try
            {
                var temporary = await LauncherService.SaveAllLaunchers(launchers);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            StatusMessage = $"Launchers Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        async Task FilterByCategory()
        {
            try
            {
                launchers = await LauncherService.GetLaunchersAsync(null, null, null, categoryIdFilter, maximumRows: MaximumRows);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        private void EditRecord(int launcherId)
        {
            _launcherId = launcherId;
            ShowCreateNewOrEdit = true;
        }
        private void CloseDialog()
        {
            ShowCreateNewOrEdit = false;
        }
        private void CreateRecord()
        {
            ShowCreateNewOrEdit = true;
        }
    }
}
