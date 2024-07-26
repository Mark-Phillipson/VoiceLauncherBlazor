
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
using RazorClassLibrary.Shared;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.DTOs;



namespace RazorClassLibrary.Pages
{
    public partial class CursorlessCheatsheetItemTable : ComponentBase
    {
        [Inject] public required ICursorlessCheatsheetItemDataService CursorlessCheatsheetItemDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<CursorlessCheatsheetItemTable>? Logger { get; set; }

        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "CursorlessCheatsheetItem Items (CursorlessCheatsheetItems)";
        public string EditTitle { get; set; } = "Edit CursorlessCheatsheetItem Item (CursorlessCheatsheetItems)";
        [Parameter] public int ParentId { get; set; }
        public List<CursorlessCheatsheetItemDTO>? CursorlessCheatsheetItemDTO { get; set; }
        public List<CursorlessCheatsheetItemDTO>? FilteredCursorlessCheatsheetItemDTO { get; set; }
        protected CursorlessCheatsheetItemAddEdit? CursorlessCheatsheetItemAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = String.Empty;
        public List<string>? PropertyInfo { get; set; }
        [CascadingParameter] public ClaimsPrincipal? User { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int CursorlessCheatsheetItemId { get; set; }
        private string cursorlessTypeFilter = "Action";
        private string? text; // Optionally, set a default value here
        private List<string> cursorlessTypeFilterList = new List<string> { "Action", "Compound Targets", "Destination", "Modifier", "Paired Delimiters", "Scope", "Scope visualizer", "Special Mark" };
        private int counter = 0;
        private bool showCards = true;
        private bool getFromJson = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (CursorlessCheatsheetItemDataService != null)
                {
                    var result = await CursorlessCheatsheetItemDataService!.GetAllCursorlessCheatsheetItemsAsync(getFromJson);
                    //var result = await CursorlessCheatsheetItemDataService.SearchCursorlessCheatsheetItemsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        CursorlessCheatsheetItemDTO = result.ToList();
                        FilteredCursorlessCheatsheetItemDTO = result.ToList();
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError("e, Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredCursorlessCheatsheetItemDTO = CursorlessCheatsheetItemDTO!.Where(x => x.CursorlessType == cursorlessTypeFilter).ToList();
            Title = $"Cursorless Cheatsheet Item ({FilteredCursorlessCheatsheetItemDTO?.Count})";

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
        private async Task AddNewCursorlessCheatsheetItem()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<CursorlessCheatsheetItemAddEdit>("Add Cursorless Cheatsheet Item", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            CursorlessCheatsheetItemId = 0;
        }


        private void ApplyFilter()
        {
            if (FilteredCursorlessCheatsheetItemDTO == null || CursorlessCheatsheetItemDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredCursorlessCheatsheetItemDTO = CursorlessCheatsheetItemDTO.OrderBy(v => v.SpokenForm).ToList();
                Title = $"Cheatsheet ({FilteredCursorlessCheatsheetItemDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredCursorlessCheatsheetItemDTO = CursorlessCheatsheetItemDTO
                    .Where(v =>
                    (v.SpokenForm != null && v.SpokenForm.ToLower().Contains(temporary))
                     || (v.Meaning != null && v.Meaning.ToLower().Contains(temporary))
                     || (v.CursorlessType != null && v.CursorlessType.ToLower().Contains(temporary))
                    )
                    .ToList();
                Title = $"Cheatsheet ({FilteredCursorlessCheatsheetItemDTO.Count})";
            }
        }
        protected void SortCursorlessCheatsheetItem(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredCursorlessCheatsheetItemDTO == null)
            {
                return;
            }
            if (sortColumn == "SpokenForm")
            {
                FilteredCursorlessCheatsheetItemDTO = FilteredCursorlessCheatsheetItemDTO.OrderBy(v => v.SpokenForm).ToList();
            }
            else if (sortColumn == "SpokenForm Desc")
            {
                FilteredCursorlessCheatsheetItemDTO = FilteredCursorlessCheatsheetItemDTO.OrderByDescending(v => v.SpokenForm).ToList();
            }
            if (sortColumn == "CursorlessType")
            {
                FilteredCursorlessCheatsheetItemDTO = FilteredCursorlessCheatsheetItemDTO.OrderBy(v => v.CursorlessType).ToList();
            }
            else if (sortColumn == "CursorlessType Desc")
            {
                FilteredCursorlessCheatsheetItemDTO = FilteredCursorlessCheatsheetItemDTO.OrderByDescending(v => v.CursorlessType).ToList();
            }
        }
        private async Task DeleteCursorlessCheatsheetItem(int id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (CursorlessCheatsheetItemDataService != null)
            {
                var cursorlessCheatsheetItem = await CursorlessCheatsheetItemDataService.GetCursorlessCheatsheetItemById(id);
                parameters.Add("Title", "Please Confirm, Delete Cursorless Cheatsheet Item");
                parameters.Add("Message", $"SpokenForm: {cursorlessCheatsheetItem?.SpokenForm}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Cursorless Cheatsheet Item ({cursorlessCheatsheetItem?.SpokenForm})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await CursorlessCheatsheetItemDataService.DeleteCursorlessCheatsheetItem(id);
                        ToastService?.ShowSuccess("Cursorless Cheatsheet Item deleted successfully");
                        await LoadData();
                    }
                }
            }
            CursorlessCheatsheetItemId = id;
        }

        private async void EditCursorlessCheatsheetItem(int id)
        {
            var parameters = new ModalParameters();
            parameters.Add("id", id);
            var formModal = Modal?.Show<CursorlessCheatsheetItemAddEdit>("Edit Cursorless Cheatsheet Item", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            CursorlessCheatsheetItemId = id;
        }
        private void FilterByCursorlessType(string cursorlessType)
        {
            if (FilteredCursorlessCheatsheetItemDTO == null || CursorlessCheatsheetItemDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(cursorlessType))
            {
                FilteredCursorlessCheatsheetItemDTO = CursorlessCheatsheetItemDTO.OrderBy(v => v.SpokenForm).ToList();
                Title = $"All Cheatsheet ({FilteredCursorlessCheatsheetItemDTO.Count})";
            }
            else
            {
                var temporary = cursorlessType.ToLower().Trim();
                FilteredCursorlessCheatsheetItemDTO = CursorlessCheatsheetItemDTO
                    .Where(v =>
                    (v.CursorlessType != null && v.CursorlessType.ToLower() == temporary.ToLower())
                    )
                    .ToList();
                Title = $"Filtered Cheatsheet ({FilteredCursorlessCheatsheetItemDTO.Count})";
            }
        }
        private async Task ShowCards()
        {
            if (!showCards)
            {
                getFromJson = false;
                await LoadData();
                StateHasChanged();
            }
        }
        private async Task ExportAsJson()
        {
            var result = await CursorlessCheatsheetItemDataService.ExportToJsonAsync();
        }
    }
}