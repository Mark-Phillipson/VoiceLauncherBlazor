using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Pages
{
    public partial class CustomIntelliSenses
    {
        [Parameter] public int? categoryIdFilter { get; set; }
        [Parameter] public int? languageIdFilter { get; set; }
        public bool ShowDialog { get; set; }
        private int customIntellisenseIdDelete { get; set; }
        private List<VoiceLauncherBlazor.Models.CustomIntelliSense> intellisenses;
        public string StatusMessage { get; set; }
        public List<VoiceLauncherBlazor.Models.Category> categories { get; set; }
        public List<VoiceLauncherBlazor.Models.Language> languages { get; set; }
        public List<VoiceLauncherBlazor.Models.GeneralLookup> generalLookups { get; set; }
        public int MaximumRows { get; set; } = 10;
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414
        private string searchTerm;
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
            if (categoryIdFilter != null)
            {
                await FilterByCategory();
            }
            else if (languageIdFilter != null)
            {
                await FilterByLanguage();
            }
            else
            {
                try
                {
                    _loadFailed = false;
                    intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(maximumRows: MaximumRows);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            try
            {
                categories = await CategoryService.GetCategoriesAsync();
                languages = await LanguageService.GetLanguagesAsync();
                generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");

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
                    intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(SearchTerm.Trim(), maximumRows: MaximumRows);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
                StateHasChanged();
            }
        }

        void ConfirmDelete(int customIntellisenseId)
        {
            ShowDialog = true;
            customIntellisenseIdDelete = customIntellisenseId;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task SortCustomIntelliSenses(string column, string sortType)
        {
            try
            {
                intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, column, sortType, maximumRows: MaximumRows);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteCustomIntelliSense(int customIntellisenseId)
        {
            try
            {
                var result = await CustomIntellisenseService.DeleteCustomIntelliSense(customIntellisenseId);
                StatusMessage = result;
                ShowDialog = false;
                intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, maximumRows: MaximumRows);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

        async Task SaveAllCustomIntelliSenses()
        {
            try
            {
                var temporary = await CustomIntellisenseService.SaveAllCustomIntelliSenses(intellisenses);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            StatusMessage = $"Custom IntelliSenses Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        async Task FilterByCategory()
        {
            try
            {
                intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, categoryIdFilter, maximumRows: MaximumRows);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task FilterByLanguage()
        {
            try
            {
                intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, null, languageIdFilter, maximumRows: MaximumRows);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }

    }
}
