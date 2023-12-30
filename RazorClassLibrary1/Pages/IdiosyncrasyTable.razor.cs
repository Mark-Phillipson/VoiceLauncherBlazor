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
    public partial class IdiosyncrasyTable : ComponentBase
    {
        [Inject] public IIdiosyncrasyDataService? IdiosyncrasyDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<IdiosyncrasyTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Idiosyncrasy Items (Idiosyncrasies)";
        public List<IdiosyncrasyDTO>? IdiosyncrasyDTO { get; set; }
        public List<IdiosyncrasyDTO>? FilteredIdiosyncrasyDTO { get; set; }
        protected IdiosyncrasyAddEdit? IdiosyncrasyAddEdit { get; set; }
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
                if (IdiosyncrasyDataService != null)
                {
                    var result = await IdiosyncrasyDataService!.GetAllIdiosyncrasiesAsync();
                    //var result = await IdiosyncrasyDataService.SearchIdiosyncrasiesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        IdiosyncrasyDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredIdiosyncrasyDTO = IdiosyncrasyDTO;
            Title = $"Idiosyncrasy ({FilteredIdiosyncrasyDTO?.Count})";

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
        protected async Task AddNewIdiosyncrasyAsync()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<IdiosyncrasyAddEdit>("Add Idiosyncrasy", parameters);
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
            if (FilteredIdiosyncrasyDTO == null || IdiosyncrasyDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredIdiosyncrasyDTO = IdiosyncrasyDTO.OrderBy(v => v.FindString).ToList();
                Title = $"All Idiosyncrasy ({FilteredIdiosyncrasyDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredIdiosyncrasyDTO = IdiosyncrasyDTO
                    .Where(v =>
                    v.FindString != null && v.FindString.ToLower().Contains(temporary)
                     || v.ReplaceWith != null && v.ReplaceWith.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Idiosyncrasys ({FilteredIdiosyncrasyDTO.Count})";
            }
        }
        protected void SortIdiosyncrasy(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredIdiosyncrasyDTO == null)
            {
                return;
            }
            if (sortColumn == "FindString")
            {
                FilteredIdiosyncrasyDTO = FilteredIdiosyncrasyDTO.OrderBy(v => v.FindString).ToList();
            }
            else if (sortColumn == "FindString Desc")
            {
                FilteredIdiosyncrasyDTO = FilteredIdiosyncrasyDTO.OrderByDescending(v => v.FindString).ToList();
            }
            if (sortColumn == "ReplaceWith")
            {
                FilteredIdiosyncrasyDTO = FilteredIdiosyncrasyDTO.OrderBy(v => v.ReplaceWith).ToList();
            }
            else if (sortColumn == "ReplaceWith Desc")
            {
                FilteredIdiosyncrasyDTO = FilteredIdiosyncrasyDTO.OrderByDescending(v => v.ReplaceWith).ToList();
            }
        }
        async Task DeleteIdiosyncrasyAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllIdiosyncrasy(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a idiosyncrasy that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (IdiosyncrasyDataService != null)
            {
                var idiosyncrasy = await IdiosyncrasyDataService.GetIdiosyncrasyById(Id);
                parameters.Add("Title", "Please Confirm, Delete Idiosyncrasy");
                parameters.Add("Message", $"FindString: {idiosyncrasy?.FindString}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Idiosyncrasy ({idiosyncrasy?.FindString})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await IdiosyncrasyDataService.DeleteIdiosyncrasy(Id);
                        ToastService?.ShowSuccess(" Idiosyncrasy deleted successfully");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditIdiosyncrasyAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<IdiosyncrasyAddEdit>("Edit Idiosyncrasy", parameters);
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