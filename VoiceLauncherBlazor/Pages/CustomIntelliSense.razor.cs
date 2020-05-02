using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Services;

namespace VoiceLauncherBlazor.Pages
{
    public partial class CustomIntelliSense
    {
        [Parameter] public int customIntellisenseId { get; set; }
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414
        [Parameter] public EventCallback OnClose { get; set; }

        public DataAccessLibrary.Models.CustomIntelliSense intellisense { get; set; }
        public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
        public List<DataAccessLibrary.Models.Language> languages { get; set; }
        public List<DataAccessLibrary.Models.Category> categories { get; set; }
        private List<string> customValidationErrors = new List<string>();
        protected override async Task OnInitializedAsync()
        {
            if (customIntellisenseId > 0)
            {
                try
                {
                    intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(customIntellisenseId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    _loadFailed = true;
                }
            }
            else
            {
                intellisense = new DataAccessLibrary.Models.CustomIntelliSense
                {
                    DeliveryType = "Send Keys"
                };
            }
            //_editContext = new EditContext(intellisense);
            generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");
            languages = await LanguageService.GetLanguagesAsync();
            categories = await CategoryService.GetCategoriesAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (intellisense.Id > 0)
            {
                intellisense = await CustomIntellisenseService.GetCustomIntelliSenseAsync(intellisense.Id);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("setFocus", "LanguageSelect");
            }
        }

        private async Task HandleValidSubmit()
        {
            customValidationErrors.Clear();
            if (intellisense.LanguageId == 0)
            {
                customValidationErrors.Add("Language is required");
            }
            if (intellisense.CategoryId == 0)
            {
                customValidationErrors.Add("Category is required");
            }
            if (customValidationErrors.Count == 0)
            {
                var result = await CustomIntellisenseService.SaveCustomIntelliSense(intellisense);
                NavigationManager.NavigateTo("intellisenses");
            }
        }
        private async Task CallChangeAsync(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("CallChange", elementId);
        }

    }
}
