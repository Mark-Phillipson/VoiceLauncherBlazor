using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Categories
    {
        public bool ShowDialog { get; set; }
        private int categoryIdDelete { get; set; }
        private List<DataAccessLibrary.Models.Category> categories;
        public string StatusMessage { get; set; }
        public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
        private string categoryTypeFilter { get; set; }
        private string searchTerm;
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414


        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                if (searchTerm != value)
                {
                    searchTerm = value;
                }
            }
        }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                categories = await CategoryService.GetCategoriesAsync();
                generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Category Types");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        async Task ApplyFilter()
        {
            if (SearchTerm != null)
            {
                try
                {
                    categories = await CategoryService.GetCategoriesAsync(SearchTerm.Trim());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }

        void HandleValidSubmit()
        {
            Console.WriteLine("OnValidSubmit");
        }
        void ConfirmDelete(int categoryId)
        {
            ShowDialog = true;
            categoryIdDelete = categoryId;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task SortCategories(string column, string sortType)
        {
            try
            {
                categories = await CategoryService.GetCategoriesAsync(searchTerm, column, sortType);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteCategory(int categoryId)
        {
            try
            {
                var result = await CategoryService.DeleteCategory(categoryId);
                StatusMessage = result;
                ShowDialog = false;
                categories = await CategoryService.GetCategoriesAsync(SearchTerm);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task FilterCategoryType()
        {
            if (categoryTypeFilter != null)
            {
                try
                {
                    categories = await CategoryService.GetCategoriesAsync(null, null, null, categoryTypeFilter);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }
        async Task SaveAllCategories()
        {
            try
            {
                categories = await CategoryService.SaveAllCategories(categories);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            StatusMessage = $"Categories Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        private async Task CallChangeAsync(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("CallChange", elementId);
            await ApplyFilter();
        }

    }
}
