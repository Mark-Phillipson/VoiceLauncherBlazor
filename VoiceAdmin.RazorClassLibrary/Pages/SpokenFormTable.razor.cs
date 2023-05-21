using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;

namespace RazorClassLibrary.Pages
{
    public partial class SpokenFormTable : ComponentBase
    {
        [Inject] public ISpokenFormDataService? SpokenFormDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<SpokenFormTable>? Logger { get; set; }

        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "SpokenForm Items (SpokenForms)";
        public string EditTitle { get; set; } = "Edit SpokenForm Item (SpokenForms)";

        [Parameter] public int WindowsSpeechVoiceCommandId { get; set; }
        public List<SpokenFormDTO>? SpokenFormDTO { get; set; }
        public List<SpokenFormDTO>? FilteredSpokenFormDTO { get; set; }
        protected SpokenFormAddEdit? SpokenFormAddEdit { get; set; }
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
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int SpokenFormId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (SpokenFormDataService != null)
                {
                    var result = await SpokenFormDataService!.GetAllSpokenFormsAsync(WindowsSpeechVoiceCommandId);
                    //var result = await SpokenFormDataService.SearchSpokenFormsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        SpokenFormDTO = result.ToList();
                        FilteredSpokenFormDTO = result.ToList();
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredSpokenFormDTO = SpokenFormDTO;
            Title = $"Spoken Form ({FilteredSpokenFormDTO?.Count})";

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
        private async Task AddNewSpokenForm()
        {
            var parameters = new ModalParameters();

            parameters.Add(nameof(WindowsSpeechVoiceCommandId), WindowsSpeechVoiceCommandId);
            var formModal = Modal?.Show<SpokenFormAddEdit>("Add Spoken Form", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            SpokenFormId = 0;
        }


        private void ApplyFilter()
        {
            if (FilteredSpokenFormDTO == null || SpokenFormDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredSpokenFormDTO = SpokenFormDTO.OrderBy(v => v.SpokenFormText).ToList();
                Title = $"All Spoken Form ({FilteredSpokenFormDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredSpokenFormDTO = SpokenFormDTO
                    .Where(v =>
                    v.SpokenFormText != null && v.SpokenFormText.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Spoken Forms ({FilteredSpokenFormDTO.Count})";
            }
        }
        protected void SortSpokenForm(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredSpokenFormDTO == null)
            {
                return;
            }
            if (sortColumn == "SpokenFormText")
            {
                FilteredSpokenFormDTO = FilteredSpokenFormDTO.OrderBy(v => v.SpokenFormText).ToList();
            }
            else if (sortColumn == "SpokenFormText Desc")
            {
                FilteredSpokenFormDTO = FilteredSpokenFormDTO.OrderByDescending(v => v.SpokenFormText).ToList();
            }
        }
        private async Task DeleteSpokenForm(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (SpokenFormDataService != null)
            {
                var spokenForm = await SpokenFormDataService.GetSpokenFormById(Id);
                parameters.Add("Title", "Please Confirm, Delete Spoken Form");
                parameters.Add("Message", $"SpokenFormText: {spokenForm?.SpokenFormText}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Spoken Form ({spokenForm?.SpokenFormText})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await SpokenFormDataService.DeleteSpokenForm(Id);
                        ToastService?.ShowSuccess("Spoken Form deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
            SpokenFormId = Id;
        }

        private async void EditSpokenForm(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<SpokenFormAddEdit>("Edit Spoken Form", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            SpokenFormId = Id;
        }

    }
}