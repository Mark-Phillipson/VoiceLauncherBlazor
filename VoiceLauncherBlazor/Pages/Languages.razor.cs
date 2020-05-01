using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Languages
    {
        public bool ShowDialog { get; set; }
        private int languageIdDelete { get; set; }
        private List<DataAccessLibrary.Models.Language> languages;
        public string StatusMessage { get; set; }
        //public List<VoiceLauncherBlazor.Models.GeneralLookup> generalLookups { get; set; }
        private bool? activeFilter { get; set; } = null;
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
                languages = await LanguageService.GetLanguagesAsync();
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
                    languages = await LanguageService.GetLanguagesAsync(SearchTerm.Trim());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
                StateHasChanged();
            }
        }

        //void HandleValidSubmit()
        //{
        //    Console.WriteLine("OnValidSubmit");
        //}
        void ConfirmDelete(int languageId)
        {
            ShowDialog = true;
            languageIdDelete = languageId;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task SortLanguages(string column, string sortType)
        {
            try
            {
                languages = await LanguageService.GetLanguagesAsync(searchTerm, column, sortType);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteLanguage(int languageId)
        {
            try
            {
                var result = await LanguageService.DeleteLanguage(languageId);
                StatusMessage = result;
                ShowDialog = false;
                languages = await LanguageService.GetLanguagesAsync();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task FilterActive()
        {
            try
            {
                languages = await LanguageService.GetLanguagesAsync(null, null, null, activeFilter);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task SaveAllLanguages()
        {
            try
            {
                languages = await LanguageService.SaveAllLanguages(languages);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            StatusMessage = $"Languages Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
        }
        async Task ShowAll()
        {
            activeFilter = null;
            await FilterActive();
        }

    }
}
