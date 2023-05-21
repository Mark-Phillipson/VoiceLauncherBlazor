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
    public partial class PhraseListGrammarTable : ComponentBase
    {
        [Inject] public IPhraseListGrammarDataService? PhraseListGrammarDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<PhraseListGrammarTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "PhraseListGrammar Items (PhraseListGrammars)";
        public List<PhraseListGrammarDTO>? PhraseListGrammarDTO { get; set; }
        public List<PhraseListGrammarDTO>? FilteredPhraseListGrammarDTO { get; set; }
        protected PhraseListGrammarAddEdit? PhraseListGrammarAddEdit { get; set; }
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
                if (PhraseListGrammarDataService != null)
                {
                    var result = await PhraseListGrammarDataService!.GetAllPhraseListGrammarsAsync();
                    //var result = await PhraseListGrammarDataService.SearchPhraseListGrammarsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        PhraseListGrammarDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredPhraseListGrammarDTO = PhraseListGrammarDTO;
            Title = $"Phrase List Grammar ({FilteredPhraseListGrammarDTO?.Count})";

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
        protected async Task AddNewPhraseListGrammarAsync()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<PhraseListGrammarAddEdit>("Add Phrase List Grammar", parameters);
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
            if (FilteredPhraseListGrammarDTO == null || PhraseListGrammarDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredPhraseListGrammarDTO = PhraseListGrammarDTO.OrderBy(v => v.PhraseListGrammarValue).ToList();
                Title = $"All Phrase List Grammar ({FilteredPhraseListGrammarDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredPhraseListGrammarDTO = PhraseListGrammarDTO
                    .Where(v =>
                    v.PhraseListGrammarValue != null && v.PhraseListGrammarValue.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Phrase List Grammars ({FilteredPhraseListGrammarDTO.Count})";
            }
        }
        protected void SortPhraseListGrammar(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredPhraseListGrammarDTO == null)
            {
                return;
            }
            if (sortColumn == "PhraseListGrammarValue")
            {
                FilteredPhraseListGrammarDTO = FilteredPhraseListGrammarDTO.OrderBy(v => v.PhraseListGrammarValue).ToList();
            }
            else if (sortColumn == "PhraseListGrammarValue Desc")
            {
                FilteredPhraseListGrammarDTO = FilteredPhraseListGrammarDTO.OrderByDescending(v => v.PhraseListGrammarValue).ToList();
            }
        }
        async Task DeletePhraseListGrammarAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllPhraseListGrammar(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a phraseListGrammar that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (PhraseListGrammarDataService != null)
            {
                var phraseListGrammar = await PhraseListGrammarDataService.GetPhraseListGrammarById(Id);
                parameters.Add("Title", "Please Confirm, Delete Phrase List Grammar");
                parameters.Add("Message", $"PhraseListGrammarValue: {phraseListGrammar?.PhraseListGrammarValue}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Phrase List Grammar ({phraseListGrammar?.PhraseListGrammarValue})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await PhraseListGrammarDataService.DeletePhraseListGrammar(Id);
                        ToastService?.ShowSuccess(" Phrase List Grammar deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditPhraseListGrammarAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<PhraseListGrammarAddEdit>("Edit Phrase List Grammar", parameters);
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