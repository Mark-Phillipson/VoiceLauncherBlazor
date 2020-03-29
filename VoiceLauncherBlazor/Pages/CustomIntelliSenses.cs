using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;
using VoiceLauncherBlazor.Models;

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
        public int MaximumRows { get; set; } = 8;
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
                intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(maximumRows: MaximumRows);
            }
            categories = await VoiceLauncherService.GetCategoriesAsync();
            languages = await VoiceLauncherService.GetLanguagesAsync();
            generalLookups = await VoiceLauncherService.GetGeneralLookUpsAsync("Delivery Type");
        }

        async Task ApplyFilter()
        {
            if (SearchTerm != null)
            {
                intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(SearchTerm.Trim(), maximumRows: MaximumRows);
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
            intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(searchTerm, column, sortType, maximumRows: MaximumRows);
        }
        async Task DeleteCustomIntelliSense(int customIntellisenseId)
        {
            var result = await VoiceLauncherService.DeleteCustomIntelliSense(customIntellisenseId);
            StatusMessage = result;
            ShowDialog = false;
            intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(searchTerm, maximumRows: MaximumRows);
        }

        async Task SaveAllCustomIntelliSenses()
        {
            var temporary = await VoiceLauncherService.SaveAllCustomIntelliSenses(intellisenses);
            StatusMessage = $"Custom IntelliSenses Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        async Task FilterByCategory()
        {
            intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(null, null, null, categoryIdFilter, maximumRows: MaximumRows);
        }
        async Task FilterByLanguage()
        {
            intellisenses = await VoiceLauncherService.GetCustomIntelliSensesAsync(null, null, null, null, languageIdFilter, maximumRows: MaximumRows);
        }

    }
}
