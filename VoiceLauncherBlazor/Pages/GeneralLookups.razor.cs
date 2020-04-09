using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Pages
{
    public partial class GeneralLookups
    {
        public bool ShowDialog { get; set; }
        public bool ShowValidationWarning { get; set; }
        private int generalLookupIdDelete { get; set; }
        public string StatusMessage { get; set; }
        public List<VoiceLauncherBlazor.Models.GeneralLookup> generalLookups { get; set; }
        public List<string> generalLookupsCategories { get; set; }
        private string categoryFilter { get; set; }
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
                generalLookups = await GeneralLookupService.GetGeneralLookupsAsync();
                generalLookupsCategories = await GeneralLookupService.GetGeneralLookUpsCategoriesAsync();
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
                    generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(SearchTerm.Trim());
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
        void ConfirmDelete(int generalLookupId)
        {
            ShowDialog = true;
            generalLookupIdDelete = generalLookupId;
        }
        void CancelDialog()
        {
            ShowDialog = false;
        }
        async Task SortGeneralLookups(string column, string sortType)
        {
            try
            {
                generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm, column, sortType);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteGeneralLookup(int generalLookupId)
        {
            try
            {
                var result = await GeneralLookupService.DeleteGeneralLookup(generalLookupId);
                StatusMessage = result;
                ShowDialog = false;
                generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task FilterGeneralLookups()
        {
            if (categoryFilter != null)
            {
                try
                {
                    generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(null, null, null, categoryFilter);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }
        async Task SaveAllGeneralLookups()
        {
            try
            {
                generalLookups = await GeneralLookupService.SaveAllGeneralLookups(generalLookups);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            StatusMessage = $"General Lookups Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        private void NotifyInvalid()
        {
            ShowValidationWarning = true;
        }
        private async Task DuplicateRecord(int generalLookupId)
        {
            VoiceLauncherBlazor.Models.GeneralLookup generalLookupSource = await GeneralLookupService.GetGeneralLookupAsync(generalLookupId);
            VoiceLauncherBlazor.Models.GeneralLookup generalLookup = new VoiceLauncherBlazor.Models.GeneralLookup
            {
                Category = generalLookupSource?.Category,
                DisplayValue = generalLookupSource?.DisplayValue,
                ItemValue = "TBC!",
                SortOrder = generalLookupSource?.SortOrder + 1
            };
            generalLookups.Add(generalLookup);
            await JSRuntime.InvokeVoidAsync("setFocus", "0ItemValue");
        }
        private async Task CallChangeAsync(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("CallChange", elementId);
        }
    }
}
