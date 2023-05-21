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
    public partial class HtmlTagTable : ComponentBase
    {
        [Inject] public IHtmlTagDataService? HtmlTagDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<HtmlTagTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "HtmlTag Items (HtmlTags)";
        public List<HtmlTagDTO>? HtmlTagDTO { get; set; }
        public List<HtmlTagDTO>? FilteredHtmlTagDTO { get; set; }
        protected HtmlTagAddEdit? HtmlTagAddEdit { get; set; }
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
                if (HtmlTagDataService != null)
                {
                    var result = await HtmlTagDataService!.GetAllHtmlTagsAsync();
                    //var result = await HtmlTagDataService.SearchHtmlTagsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        HtmlTagDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredHtmlTagDTO = HtmlTagDTO;
            Title = $"Html Tag ({FilteredHtmlTagDTO?.Count})";

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
        protected async Task AddNewHtmlTagAsync()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<HtmlTagAddEdit>("Add Html Tag", parameters);
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
            if (FilteredHtmlTagDTO == null || HtmlTagDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredHtmlTagDTO = HtmlTagDTO.OrderBy(v => v.Tag).ToList();
                Title = $"All Html Tag ({FilteredHtmlTagDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredHtmlTagDTO = HtmlTagDTO
                    .Where(v =>
                    v.Tag != null && v.Tag.ToLower().Contains(temporary)
                     || v.Description != null && v.Description.ToLower().Contains(temporary)
                     || v.ListValue != null && v.ListValue.ToLower().Contains(temporary)
                     || v.SpokenForm != null && v.SpokenForm.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Html Tags ({FilteredHtmlTagDTO.Count})";
            }
        }
        protected void SortHtmlTag(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredHtmlTagDTO == null)
            {
                return;
            }
            if (sortColumn == "Tag")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderBy(v => v.Tag).ToList();
            }
            else if (sortColumn == "Tag Desc")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderByDescending(v => v.Tag).ToList();
            }
            if (sortColumn == "Description")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderBy(v => v.Description).ToList();
            }
            else if (sortColumn == "Description Desc")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderByDescending(v => v.Description).ToList();
            }
            if (sortColumn == "ListValue")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderBy(v => v.ListValue).ToList();
            }
            else if (sortColumn == "ListValue Desc")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderByDescending(v => v.ListValue).ToList();
            }
            if (sortColumn == "SpokenForm")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderBy(v => v.SpokenForm).ToList();
            }
            else if (sortColumn == "SpokenForm Desc")
            {
                FilteredHtmlTagDTO = FilteredHtmlTagDTO.OrderByDescending(v => v.SpokenForm).ToList();
            }
        }
        async Task DeleteHtmlTagAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllHtmlTag(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a htmlTag that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (HtmlTagDataService != null)
            {
                var htmlTag = await HtmlTagDataService.GetHtmlTagById(Id);
                parameters.Add("Title", "Please Confirm, Delete Html Tag");
                parameters.Add("Message", $"Tag: {htmlTag?.Tag}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Html Tag ({htmlTag?.Tag})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await HtmlTagDataService.DeleteHtmlTag(Id);
                        ToastService?.ShowSuccess(" Html Tag deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditHtmlTagAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<HtmlTagAddEdit>("Edit Html Tag", parameters);
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