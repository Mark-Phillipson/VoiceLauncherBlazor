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
    public partial class MicrophoneTable : ComponentBase
    {
        [Inject] public IMicrophoneDataService? MicrophoneDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<MicrophoneTable>? Logger { get; set; }

        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Microphone Items (Microphones)";
        public string EditTitle { get; set; } = "Edit Microphone Item (Microphones)";
        [Parameter] public int ParentId { get; set; }
        public List<MicrophoneDTO>? MicrophoneDTO { get; set; }
        public List<MicrophoneDTO>? FilteredMicrophoneDTO { get; set; }
        protected MicrophoneAddEdit? MicrophoneAddEdit { get; set; }
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
        private int MicrophoneId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (MicrophoneDataService != null)
                {
                    var result = await MicrophoneDataService!.GetAllMicrophonesAsync();
                    //var result = await MicrophoneDataService.SearchMicrophonesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        MicrophoneDTO = result.ToList();
                        FilteredMicrophoneDTO = result.ToList();
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
            FilteredMicrophoneDTO = MicrophoneDTO;
            Title = $"Microphone ({FilteredMicrophoneDTO?.Count})";

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
        private async Task AddNewMicrophone()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<MicrophoneAddEdit>("Add Microphone", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            MicrophoneId = 0;
        }


        private void ApplyFilter()
        {
            if (FilteredMicrophoneDTO == null || MicrophoneDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredMicrophoneDTO = MicrophoneDTO.OrderBy(v => v.MicrophoneName).ToList();
                Title = $"All Microphone ({FilteredMicrophoneDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredMicrophoneDTO = MicrophoneDTO
                    .Where(v =>
                    v.MicrophoneName != null && v.MicrophoneName.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Microphones ({FilteredMicrophoneDTO.Count})";
            }
        }
        protected void SortMicrophone(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredMicrophoneDTO == null)
            {
                return;
            }
            if (sortColumn == "MicrophoneName")
            {
                FilteredMicrophoneDTO = FilteredMicrophoneDTO.OrderBy(v => v.MicrophoneName).ToList();
            }
            else if (sortColumn == "MicrophoneName Desc")
            {
                FilteredMicrophoneDTO = FilteredMicrophoneDTO.OrderByDescending(v => v.MicrophoneName).ToList();
            }
        }
        private async Task DeleteMicrophone(int Id)
        {
            var parameters = new ModalParameters();
            if (MicrophoneDataService != null)
            {
                var microphone = await MicrophoneDataService.GetMicrophoneById(Id);
                parameters.Add("Title", "Please Confirm, Delete Microphone");
                parameters.Add("Message", $"MicrophoneName: {microphone?.MicrophoneName}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Microphone ({microphone?.MicrophoneName})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await MicrophoneDataService.DeleteMicrophone(Id);
                        ToastService?.ShowSuccess("Microphone deleted successfully");
                        await LoadData();
                    }
                }
            }
            MicrophoneId = Id;
        }

        private async void EditMicrophone(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<MicrophoneAddEdit>("Edit Microphone", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            MicrophoneId = Id;
        }

    }
}