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
    public partial class GrammarNameLookUpTable : ComponentBase
    {
        [Parameter] public EventCallback<string> OnSelectCallback { get; set; }
        [Inject] public IGrammarNameDataService? GrammarNameDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<GrammarNameTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "GrammarName Items (GrammarNames)";
        public List<GrammarNameDTO>? GrammarNameDTO { get; set; }
        public List<GrammarNameDTO>? FilteredGrammarNameDTO { get; set; }
        protected GrammarNameAddEdit? GrammarNameAddEdit { get; set; }
        private bool _showItems = false;
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
                if (GrammarNameDataService != null)
                {
                    var result = await GrammarNameDataService!.GetAllGrammarNamesAsync();
                    //var result = await GrammarNameDataService.SearchGrammarNamesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        GrammarNameDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredGrammarNameDTO = GrammarNameDTO;
            Title = $"Grammar Names ({FilteredGrammarNameDTO?.Count})";

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
        protected async Task AddNewGrammarNameAsync()
        {
            if (GrammarNameDataService == null)
            {
                return;
            }
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<GrammarNameAddEdit>("Add Grammar Name", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    _showItems = false;
                    await LoadData();
                }
            }
        }

        private void ApplyFilter()
        {
            if (FilteredGrammarNameDTO == null || GrammarNameDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredGrammarNameDTO = GrammarNameDTO.OrderBy(v => v.NameOfGrammar).ToList();
                Title = $"All Grammar Names ({FilteredGrammarNameDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredGrammarNameDTO = GrammarNameDTO
                    .Where(v =>
                    v.NameOfGrammar != null && v.NameOfGrammar.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Grammar Names ({FilteredGrammarNameDTO.Count})";
            }
        }
        protected void SortGrammarName(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredGrammarNameDTO == null)
            {
                return;
            }
            if (sortColumn == "NameOfGrammar")
            {
                FilteredGrammarNameDTO = FilteredGrammarNameDTO.OrderBy(v => v.NameOfGrammar).ToList();
            }
            else if (sortColumn == "NameOfGrammar Desc")
            {
                FilteredGrammarNameDTO = FilteredGrammarNameDTO.OrderByDescending(v => v.NameOfGrammar).ToList();
            }
        }
        async Task DeleteGrammarNameAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllGrammarName(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a grammarName that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (GrammarNameDataService != null)
            {
                var grammarName = await GrammarNameDataService.GetGrammarNameById(Id);
                parameters.Add("Title", "Please Confirm, Delete Grammar Name");
                parameters.Add("Message", $"NameOfGrammar: {grammarName?.NameOfGrammar}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Grammar Name ({grammarName?.NameOfGrammar})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await GrammarNameDataService.DeleteGrammarName(Id);
                        ToastService?.ShowSuccess(" Grammar Name deleted successfully");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditGrammarNameAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<GrammarNameAddEdit>("Edit Grammar Name", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }
        private async Task SelectedItemAsync(int id)
        {
            if (GrammarNameDataService != null)
            {
                var grammar = await GrammarNameDataService.GetGrammarNameById(id);
                await OnSelectCallback.InvokeAsync(grammar.NameOfGrammar);
            }
        }
    }
}