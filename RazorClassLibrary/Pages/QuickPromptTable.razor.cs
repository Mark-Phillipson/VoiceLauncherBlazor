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
using DataAccessLibrary.DTO;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class QuickPromptTable : ComponentBase
    {
        [Inject] public IQuickPromptDataService? QuickPromptDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<QuickPromptTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        
        public string Title { get; set; } = "Quick Prompt Items (QuickPrompts)";
        public List<QuickPromptDTO>? QuickPromptDTO { get; set; }
        public List<QuickPromptDTO>? FilteredQuickPromptDTO { get; set; }
        public List<string>? AvailableTypes { get; set; }
        
        ElementReference SearchInput;
        
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
        private string? selectedType = null;
        public string? ExceptionMessage { get; set; }
#pragma warning restore 414, 649

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (QuickPromptDataService != null)
                {
                    var result = await QuickPromptDataService.GetAllQuickPromptsAsync(1, 1000, "");
                    if (result != null)
                    {
                        QuickPromptDTO = result.ToList();
                        FilteredQuickPromptDTO = result.ToList();
                        
                        // Get available types for filter dropdown
                        AvailableTypes = QuickPromptDTO.Select(x => x.Type).Distinct().OrderBy(x => x).ToList();
                        
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            
            Title = $"Quick Prompts ({FilteredQuickPromptDTO?.Count})";
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

        private void OnSearchInput(ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString();
            ApplyFilter();
        }

        private string GetTypeColorClass(string type)
        {
            return type?.ToUpper().Replace(" ", "-") switch
            {
                "FIXES" => "type-fixes",
                "FORMATTING" => "type-formatting", 
                "TEXT-GENERATION" => "type-text-generation",
                "FILE-CONVERSIONS" => "type-file-conversions",
                "CHECKERS" => "type-checkers",
                "TRANSLATIONS" => "type-translations",
                "CODE-GENERATION" => "type-code-generation",
                "WRITING-HELPERS" => "type-writing-helpers",
                _ => ""
            };
        }

        private async Task AddNewQuickPrompt()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<QuickPromptAddEdit>("Add Quick Prompt", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    if (searchTerm != null)
                    {
                        ApplyFilter();
                    }
                }
            }
        }

        private void ApplyFilter()
        {
            if (QuickPromptDTO != null)
            {
                if (string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(selectedType))
                {
                    FilteredQuickPromptDTO = QuickPromptDTO;
                }
                else
                {
                    var filtered = QuickPromptDTO.AsQueryable();
                    
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var temporary = searchTerm.ToLower().Trim();
                        filtered = filtered.Where(v => 
                            (v.Type != null && v.Type.ToLower().Contains(temporary)) ||
                            (v.Command != null && v.Command.ToLower().Contains(temporary)) ||
                            (v.PromptText != null && v.PromptText.ToLower().Contains(temporary)) ||
                            (v.Description != null && v.Description.ToLower().Contains(temporary)));
                    }
                    
                    if (!string.IsNullOrWhiteSpace(selectedType))
                    {
                        filtered = filtered.Where(v => v.Type == selectedType);
                    }
                    
                    FilteredQuickPromptDTO = filtered.ToList();
                }
                
                Title = $"Quick Prompts ({FilteredQuickPromptDTO.Count})";
            }
        }

        private void FilterByType(ChangeEventArgs e)
        {
            selectedType = e.Value?.ToString();
            ApplyFilter();
        }

        protected void SortQuickPrompt(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredQuickPromptDTO == null)
            {
                return;
            }

            switch (sortColumn)
            {
                case "Type":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderBy(v => v.Type).ToList();
                    break;
                case "Type Desc":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderByDescending(v => v.Type).ToList();
                    break;
                case "Command":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderBy(v => v.Command).ToList();
                    break;
                case "Command Desc":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderByDescending(v => v.Command).ToList();
                    break;
                case "CreatedDate":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderBy(v => v.CreatedDate).ToList();
                    break;
                case "CreatedDate Desc":
                    FilteredQuickPromptDTO = FilteredQuickPromptDTO.OrderByDescending(v => v.CreatedDate).ToList();
                    break;
            }
        }

        async Task DeleteQuickPromptAsync(int Id)
        {
            var parameters = new ModalParameters();
            if (QuickPromptDataService != null)
            {
                var quickPrompt = await QuickPromptDataService.GetQuickPromptById(Id);
                parameters.Add("Title", "Please Confirm, Delete Quick Prompt");
                parameters.Add("Message", $"Command: {quickPrompt?.Command}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Quick Prompt ({quickPrompt?.Command})?", parameters);
                
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await QuickPromptDataService.DeleteQuickPrompt(Id);
                        ToastService?.ShowSuccess("Quick Prompt deleted successfully");
                        await LoadData();
                        if (searchTerm != null)
                        {
                            ApplyFilter();
                        }
                    }
                }
            }
        }

        async Task EditQuickPromptAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<QuickPromptAddEdit>("Edit Quick Prompt", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    if (searchTerm != null)
                    {
                        ApplyFilter();
                    }
                }
            }
        }
    }
}
