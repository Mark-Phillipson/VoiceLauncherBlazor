using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class GrammarItemTable : ComponentBase
    {
        [Inject] public IGrammarItemDataService? GrammarItemDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<GrammarItemTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "GrammarItem Items (GrammarItems)";

        [Parameter] public int GrammarNameId { get; set; }
        public List<GrammarItemDTO>? GrammarItemDTO { get; set; }
        public List<GrammarItemDTO>? FilteredGrammarItemDTO { get; set; }
        protected GrammarItemAddEdit? GrammarItemAddEdit { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
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
                if (GrammarItemDataService != null)
                {
                    var result = await GrammarItemDataService!.GetAllGrammarItemsAsync(GrammarNameId);
                    //var result = await GrammarItemDataService.SearchGrammarItemsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        GrammarItemDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredGrammarItemDTO = GrammarItemDTO;
            Title = $"Items ({FilteredGrammarItemDTO?.Count})";

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
        protected async Task AddNewGrammarItemAsync()
        {
            if (FilteredGrammarItemDTO == null)
            {
                return;
            }
            GrammarItemDTO grammarItem = new GrammarItemDTO();
            grammarItem.GrammarNameId = GrammarNameId;
            FilteredGrammarItemDTO.Add(grammarItem);
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("window.setFocus", FilteredGrammarItemDTO.Count.ToString());
            }

            return;
        }

        private void ApplyFilter()
        {
            if (FilteredGrammarItemDTO == null || GrammarItemDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredGrammarItemDTO = GrammarItemDTO.OrderBy(v => v.Value).ToList();
                Title = $"All Items ({FilteredGrammarItemDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredGrammarItemDTO = GrammarItemDTO
                    .Where(v =>
                    v.Value != null && v.Value.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Items ({FilteredGrammarItemDTO.Count})";
            }
        }
        protected void SortGrammarItem(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredGrammarItemDTO == null)
            {
                return;
            }
            if (sortColumn == "Value")
            {
                FilteredGrammarItemDTO = FilteredGrammarItemDTO.OrderBy(v => v.Value).ToList();
            }
            else if (sortColumn == "Value Desc")
            {
                FilteredGrammarItemDTO = FilteredGrammarItemDTO.OrderByDescending(v => v.Value).ToList();
            }
        }
        async Task DeleteGrammarItemAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllGrammarItem(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a grammarItem that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (GrammarItemDataService != null)
            {
                var grammarItem = await GrammarItemDataService.GetGrammarItemById(Id);
                parameters.Add("Title", "Please Confirm, Delete Grammar Item");
                parameters.Add("Message", $"Value: {grammarItem?.Value}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Grammar Item ({grammarItem?.Value})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await GrammarItemDataService.DeleteGrammarItem(Id);
                        ToastService?.ShowSuccess(" Grammar Item deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditGrammarItemAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<GrammarItemAddEdit>("Edit Grammar Item", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }
        async Task SaveAllAsync()
        {
            if (GrammarItemDataService == null)
            {
                return;
            }
            var result = await GrammarItemDataService.SaveAllAsync(FilteredGrammarItemDTO);
            if (result == false)
            {
                ToastService?.ShowError(" Error saving! ", "Error");
            }
            else
            {
                ToastService?.ShowSuccess(" Grammar Items Updated! ", "Success");
            }
        }
        private async Task CallChangeAsync(string elementId)
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("CallChange", elementId);
            }
        }

    }
}