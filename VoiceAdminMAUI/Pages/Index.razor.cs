using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoiceAdminMAUI.Pages
{
   public partial class Index
   {
      [Inject] protected NavigationManager NavigationManager { get; set; }
      [Inject] public required LanguageService LanguageService { get; set; }
      [Inject] public required CategoryService CategoryService { get; set; }
      [Inject] protected IJSRuntime JSRuntime { get; set; }
      private int languageNameId = 1;
      private int categoryNameId = 39;
      protected override async Task OnInitializedAsync()
      {
         string[] arguments = Environment.GetCommandLineArgs();
         if (arguments != null || arguments.Length == 0)
         {
            return;
         }
         if (arguments.Count() < 2)
         {
            arguments = new string[] { arguments[0], "SearchIntelliSense", "Not Applicable", "Snippet" };
         }
         if (arguments.Count() > 2 && arguments[1].Contains("SearchIntelliSense"))
         {
            string languageName = "";
            string categoryName = "";
            languageName = arguments[2].Replace("/", "").Trim();
            categoryName = arguments[3].Replace("/", "").Trim();
            var language = await LanguageService.GetLanguageAsync(languageName);
            var category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
            languageNameId = language.Id;
            categoryNameId = category.Id;

         }



      }
   }
}