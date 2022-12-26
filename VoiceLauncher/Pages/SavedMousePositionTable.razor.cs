
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
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Pages
{
    public partial class SavedMousePositionTable : ComponentBase
    {
        [Inject] public ISavedMousePositionDataService? SavedMousePositionDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<SavedMousePositionTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "SavedMousePosition Items (SavedMousePositions)";
        public List<SavedMousePositionDTO>? SavedMousePositionDTO { get; set; }
        public List<SavedMousePositionDTO>? FilteredSavedMousePositionDTO { get; set; }
        protected SavedMousePositionAddEdit? SavedMousePositionAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
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
            try
            {
                if (SavedMousePositionDataService != null)
                {
                    var result = (await SavedMousePositionDataService.GetAllSavedMousePositionsAsync());

                    if (result != null)
                    {
                        SavedMousePositionDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredSavedMousePositionDTO = SavedMousePositionDTO;
            Title = $"Saved Mouse Position ({FilteredSavedMousePositionDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime!= null )
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
        protected async Task AddNewSavedMousePositionAsync()
        {
            var parameters = new ModalParameters();
            if (User != null )
            {
                parameters.Add(nameof(User), User); 
            }
            var formModal = Modal?.Show<SavedMousePositionAddEdit>("Add Saved Mouse Position", parameters);
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
            if (FilteredSavedMousePositionDTO == null || SavedMousePositionDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredSavedMousePositionDTO = SavedMousePositionDTO.OrderBy(v => v.NamedLocation).ToList();
                Title = $"All Saved Mouse Position ({FilteredSavedMousePositionDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredSavedMousePositionDTO = SavedMousePositionDTO
                    .Where(v => 
                    (v.NamedLocation!= null  && v.NamedLocation.ToLower().Contains(temporary))
                    )
                    .ToList();
                Title = $"Filtered Saved Mouse Positions ({FilteredSavedMousePositionDTO.Count})";
            }
        }
        protected void SortSavedMousePosition(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
                        if (FilteredSavedMousePositionDTO == null)
            {
                return;
            }
            if (sortColumn == "NamedLocation")
            {
                FilteredSavedMousePositionDTO = FilteredSavedMousePositionDTO.OrderBy(v => v.NamedLocation).ToList();
            }
            else if (sortColumn == "NamedLocation Desc")
            {
                FilteredSavedMousePositionDTO = FilteredSavedMousePositionDTO.OrderByDescending(v => v.NamedLocation).ToList();
            }
        }
        async Task DeleteSavedMousePositionAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllSavedMousePosition(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a savedMousePosition that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (SavedMousePositionDataService != null)
            {
                var savedMousePosition = await SavedMousePositionDataService.GetSavedMousePositionById(Id);
                parameters.Add("Title", "Please Confirm, Delete Saved Mouse Position");
                parameters.Add("Message", $"NamedLocation: {savedMousePosition?.NamedLocation}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Saved Mouse Position ({savedMousePosition?.NamedLocation})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await SavedMousePositionDataService.DeleteSavedMousePosition(Id);
                        ToastService?.ShowSuccess(" Saved Mouse Position deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditSavedMousePositionAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            if (User != null)
            {
                parameters.Add(nameof(User), User); 
            }
            var formModal = Modal?.Show<SavedMousePositionAddEdit>("Edit Saved Mouse Position", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }
    }
}