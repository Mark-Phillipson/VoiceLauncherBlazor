
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
using VoiceLauncher.Shared;
using VoiceLauncher.Services;
using VoiceLauncher.DTOs;
using System.Diagnostics.Eventing.Reader;
using Humanizer;

namespace VoiceLauncher.Pages
{
    public partial class CategoryIntelisenseTable : ComponentBase
    {
        [Parameter] public int LanguageId { get; set; } = 8;
        [Inject] public ICategoryDataService? CategoryDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<CategoryTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Category Items (Categories)";
        public List<CategoryDTO>? CategoryDTO { get; set; }
        public List<CategoryDTO>? FilteredCategoryDTO { get; set; }
        protected CategoryAddEdit? CategoryAddEdit { get; set; }
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
        [Parameter] public string CategoryType { get; set; } = "IntelliSense Command";
        private bool _ShowCards { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (CategoryDataService != null)
                {
                    var result = await CategoryDataService!.GetAllCategoriesAsync(CategoryType,LanguageId);
                    //var result = await CategoryDataService.SearchCategoriesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        CategoryDTO = result.ToList();
                    }
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredCategoryDTO = CategoryDTO;
            Title = $"Category ({FilteredCategoryDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime!= null )
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
        protected async Task AddNewCategoryAsync()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<CategoryAddEdit>("Add Category", parameters);
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
            if (FilteredCategoryDTO == null || CategoryDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredCategoryDTO = CategoryDTO.OrderBy(v => v.CategoryName).ToList();
                Title = $"All Category ({FilteredCategoryDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredCategoryDTO = CategoryDTO
                    .Where(v => 
                    (v.CategoryName != null  && v.CategoryName.ToLower().Contains(temporary))
                     || (v.CategoryType!= null  &&  v.CategoryType.ToLower().Contains(temporary))
                    )
                    .ToList();
                Title = $"Filtered Categorys ({FilteredCategoryDTO.Count})";
            }
        }
        protected void SortCategory(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
                        if (FilteredCategoryDTO == null)
            {
                return;
            }
            if (sortColumn == "Category")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.CategoryName).ToList();
            }
            else if (sortColumn == "Category Desc")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.CategoryName).ToList();
            }
            if (sortColumn == "CategoryType")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.CategoryType).ToList();
            }
            else if (sortColumn == "CategoryType Desc")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.CategoryType).ToList();
            }
            if (sortColumn == "Sensitive")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.Sensitive).ToList();
            }
            else if (sortColumn == "Sensitive Desc")
            {
                FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.Sensitive).ToList();
            }
        }
        async Task DeleteCategoryAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllCategory(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a category that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (CategoryDataService != null)
            {
                var category = await CategoryDataService.GetCategoryById(Id);
                parameters.Add("Title", "Please Confirm, Delete Category");
                parameters.Add("Message", $"Category: {category?.CategoryName}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Category ({category?.CategoryName})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await CategoryDataService.DeleteCategory(Id);
                        ToastService?.ShowSuccess(" Category deleted successfully", "SUCCESS");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditCategoryAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<CategoryAddEdit>("Edit Category", parameters);
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