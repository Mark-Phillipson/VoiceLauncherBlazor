
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
using DataAccessLibrary.DTO;
using System.Security.Claims;
using Ardalis.GuardClauses;

using SampleApplication.Services;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Shared;

namespace RazorClassLibrary.Pages
{
    public partial class PromptTable : ComponentBase
    {
        [Inject] public IPromptDataService? PromptDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<PromptTable>? Logger { get; set; }

        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Prompt Items (Prompts)";
        public string EditTitle { get; set; } = "Edit Prompt Item (Prompts)";
        [Parameter] public int ParentId { get; set; }
        public List<PromptDTO>? PromptDTO { get; set; }
        public List<PromptDTO>? FilteredPromptDTO { get; set; }
        protected PromptAddEdit? PromptAddEdit { get; set; }
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
        private int PromptId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (PromptDataService != null)
                {
                    var result = await PromptDataService!.GetAllPromptsAsync();
                    //var result = await PromptDataService.SearchPromptsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        PromptDTO = result.ToList();
                        FilteredPromptDTO = result.ToList();
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
            FilteredPromptDTO = PromptDTO;
            Title = $"Prompt ({FilteredPromptDTO?.Count})";

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
        private async Task AddNewPrompt()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<PromptAddEdit>("Add Prompt", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            PromptId = 0;
        }


        private void ApplyFilter()
        {
            if (FilteredPromptDTO == null || PromptDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredPromptDTO = PromptDTO.OrderBy(v => v.PromptText).ToList();
                Title = $"All Prompt ({FilteredPromptDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredPromptDTO = PromptDTO
                    .Where(v =>
                    v.PromptText != null && v.PromptText.ToLower().Contains(temporary)
                     || v.Description != null && v.Description.ToLower().Contains(temporary)
                    )
                    .ToList();
                Title = $"Filtered Prompts ({FilteredPromptDTO.Count})";
            }
        }
        protected void SortPrompt(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredPromptDTO == null)
            {
                return;
            }
            if (sortColumn == "PromptText")
            {
                FilteredPromptDTO = FilteredPromptDTO.OrderBy(v => v.PromptText).ToList();
            }
            else if (sortColumn == "PromptText Desc")
            {
                FilteredPromptDTO = FilteredPromptDTO.OrderByDescending(v => v.PromptText).ToList();
            }
            if (sortColumn == "IsDefault")
            {
                FilteredPromptDTO = FilteredPromptDTO.OrderBy(v => v.IsDefault).ToList();
            }
            else if (sortColumn == "IsDefault Desc")
            {
                FilteredPromptDTO = FilteredPromptDTO.OrderByDescending(v => v.IsDefault).ToList();
            }
        }
        private async Task DeletePrompt(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (PromptDataService != null)
            {
                var prompt = await PromptDataService.GetPromptById(Id);
                parameters.Add("Title", "Please Confirm, Delete Prompt");
                parameters.Add("Message", $"PromptText: {prompt?.PromptText}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Prompt ({prompt?.PromptText})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await PromptDataService.DeletePrompt(Id);
                        ToastService?.ShowSuccess("Prompt deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
            PromptId = Id;
        }

        private async void EditPrompt(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<PromptAddEdit>("Edit Prompt", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            PromptId = Id;
        }

    }
}