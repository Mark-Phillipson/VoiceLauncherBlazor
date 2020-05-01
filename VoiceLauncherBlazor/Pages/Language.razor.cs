using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
    public partial class Language
    {
        [Parameter] public int languageId { get; set; }
        private EditContext _editContext;
        public DataAccessLibrary.Models.Language language { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414


        protected override async Task OnInitializedAsync()
        {
            if (languageId > 0)
            {
                try
                {
                    language = await LanguageService.GetLanguageAsync(languageId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            else
            {
                language = new DataAccessLibrary.Models.Language
                {
                    Active = true
                };
            }
            _editContext = new EditContext(language);

        }

        protected override async Task OnParametersSetAsync()
        {
            if (languageId > 0)
            {
                try
                {
                    language = await LanguageService.GetLanguageAsync(languageId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
        }
        private async Task HandleValidSubmit()
        {
            try
            {
                var result = await LanguageService.SaveLanguage(language);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                _loadFailed = true;
            }
            NavigationManager.NavigateTo("languages");
        }
    }
}
