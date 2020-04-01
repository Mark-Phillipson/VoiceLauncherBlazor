using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Pages
{
    public partial class CustomIntelliSense
    {
        [Parameter] public int customIntellisenseId { get; set; }
        private EditContext _editContext;
#pragma warning disable 414
        private bool _loadFailed = false;
#pragma warning restore 414


        public VoiceLauncherBlazor.Models.CustomIntelliSense intellisense { get; set; }
        public List<VoiceLauncherBlazor.Models.GeneralLookup> generalLookups { get; set; }
        public List<VoiceLauncherBlazor.Models.Language> languages { get; set; }
        public List<VoiceLauncherBlazor.Models.Category> categories { get; set; }
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
                intellisense = new Models.CustomIntelliSense
                {
                    DeliveryType = "SendKeys"
                };
            }
            _editContext = new EditContext(intellisense);
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

        private async Task HandleValidSubmit()
        {
            var result = await CustomIntellisenseService.SaveCustomIntelliSense(intellisense);
            NavigationManager.NavigateTo("intellisenses");
        }


    }
}
