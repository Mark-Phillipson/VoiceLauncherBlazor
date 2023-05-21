using Blazored.Toast.Services;

using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class Languages : ComponentBase
    {
        public Languages()
        {

        }
        [Inject] IToastService? ToastService { get; set; }
        public bool ShowDialog { get; set; }
        public bool ShowAsCards { get; set; } = true;
        private int LanguageIdDelete { get; set; }
        private List<DataAccessLibrary.Models.Language>? LanguagesModel;
        public string? StatusMessage { get; set; }
        //public List<VoiceLauncherBlazor.Models.GeneralLookup> generalLookups { get; set; }
        private bool? ActiveFilter { get; set; } = null;
        private string searchTerm = "";
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
                LanguagesModel = await LanguageService.GetLanguagesAsync();
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
                    LanguagesModel = await LanguageService.GetLanguagesAsync(SearchTerm.Trim());
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
            LanguageIdDelete = languageId;
        }
        void CancelDelete()
        {
            ShowDialog = false;
        }
        async Task SortLanguages(string column, string sortType)
        {
            try
            {
                LanguagesModel = await LanguageService.GetLanguagesAsync(searchTerm, column, sortType);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task DeleteLanguage(int languageId)
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            try
            {
                var result = await LanguageService.DeleteLanguage(languageId);
                StatusMessage = result;
                ShowDialog = false;
                LanguagesModel = await LanguageService.GetLanguagesAsync();

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
                LanguagesModel = await LanguageService.GetLanguagesAsync(null, null, null, ActiveFilter);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
        }
        async Task SaveAllLanguages()
        {
            if (Environment.MachineName != "J40L4V3")
            {
                ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
                return;
            }
            try
            {
                LanguagesModel = await LanguageService.SaveAllLanguages(LanguagesModel);
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
            ActiveFilter = null;
            await FilterActive();
        }
        public void EditLanguage(int languageId)
        {
            if (NavigationManager != null)
            {
                NavigationManager.NavigateTo($"language/{languageId}");
            }
        }
    }
}
