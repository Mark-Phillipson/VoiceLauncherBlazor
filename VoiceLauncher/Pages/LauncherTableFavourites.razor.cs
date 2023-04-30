using System.Collections.Generic;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using System.Security.Claims;
using Ardalis.GuardClauses;
using VoiceLauncher.Shared;
using VoiceLauncher.Services;
using VoiceLauncher.DTOs;

namespace VoiceLauncher.Pages
{
    public partial class LauncherTableFavourites : ComponentBase
    {
        [Inject] public ILauncherDataService? LauncherDataService { get; set; }
        [Inject] public ICategoryDataService? CategoryDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<LauncherTableFavourites>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Favourite Launcher Items";

        [Parameter] public int CategoryId { get; set; }
        public List<LauncherDTO>? LauncherDTO { get; set; }
        public List<LauncherDTO>? FilteredLauncherDTO { get; set; }
        protected LauncherAddEdit? LauncherAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
        private string? _randomColor1;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = String.Empty;
        public List<string>? PropertyInfo { get; set; }
        [CascadingParameter] public ClaimsPrincipal? User { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            CategoryDTO? category = null;
            try
            {
                if (LauncherDataService != null)
                {
                    var result = await LauncherDataService!.GetFavoriteLaunchersAsync();
                    //var result = await LauncherDataService.SearchLaunchersAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        LauncherDTO = result.ToList();
                    }
                }

                if (CategoryDataService != null)
                {
                    category = await CategoryDataService.GetCategoryById(CategoryId);

                }
            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredLauncherDTO = LauncherDTO;
            Title = $"Favourites: {category?.CategoryName} ({FilteredLauncherDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        protected async Task AddNewLauncherAsync()
        {
            var parameters = new ModalParameters();

            parameters.Add(nameof(CategoryId), CategoryId);
            var formModal = Modal?.Show<LauncherAddEdit>("Add Launcher", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }

        private void ApplyFilter()
        {
            if (FilteredLauncherDTO == null || LauncherDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredLauncherDTO = LauncherDTO.OrderBy(v => v.Name).ToList();
                Title = $"All Favourites ({FilteredLauncherDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredLauncherDTO = LauncherDTO
                    .Where(v =>
                    (v.Name != null && v.Name.ToLower().Contains(temporary))
                     || (v.CommandLine != null && v.CommandLine.ToLower().Contains(temporary))
                    )
                    .ToList();
                Title = $"Filtered Favourites ({FilteredLauncherDTO.Count})";
            }
        }
        protected void SortLauncher(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredLauncherDTO == null)
            {
                return;
            }
            if (sortColumn == "Name")
            {
                FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(v => v.Name).ToList();
            }
            else if (sortColumn == "Name Desc")
            {
                FilteredLauncherDTO = FilteredLauncherDTO.OrderByDescending(v => v.Name).ToList();
            }
            if (sortColumn == "CommandLine")
            {
                FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(v => v.CommandLine).ToList();
            }
            else if (sortColumn == "CommandLine Desc")
            {
                FilteredLauncherDTO = FilteredLauncherDTO.OrderByDescending(v => v.CommandLine).ToList();
            }
        }
        async Task DeleteLauncherAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllLauncher(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a launcher that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (LauncherDataService != null)
            {
                var launcher = await LauncherDataService.GetLauncherById(Id);
                parameters.Add("Title", "Please Confirm, Delete Launcher");
                parameters.Add("Message", $"Name: {launcher?.Name}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Launcher ({launcher?.Name})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await LauncherDataService.DeleteLauncher(Id);
                        ToastService?.ShowSuccess(" Launcher deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditLauncherAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<LauncherAddEdit>("Edit Launcher", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }
        private async Task LaunchItemAsync(string commandLine)
        {
            if (JSRuntime == null)
            {
                return;
            }
            if (commandLine.Trim().ToLower().StartsWith("http") && NavigationManager != null)
            {
                NavigationManager.NavigateTo(commandLine,true,false);
            }
            else
            {
                await JSRuntime.InvokeVoidAsync(
                    "clipboardCopy.copyText", commandLine);
                var message = $"Copied Successfully: '{commandLine}'";
                ToastService!.ShowSuccess(message, "Copy Commandline");
            }
        }
         public  string RandomColour { get { _randomColor1 = GetColour(); return  _randomColor1; } set => _randomColor1 = value; }
        public  string  GetColour()
        {
            var random = new Random();
             return  String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

        }
    }
}