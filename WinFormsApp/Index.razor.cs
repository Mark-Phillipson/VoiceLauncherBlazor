using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;


namespace WinFormsApp
{
	public partial class Index
	{
		[Inject] public required LanguageService LanguageService { get; set; }
		[Inject] public required CategoryService CategoryService { get; set; }

		private int languageId;
		private int categoryId;
		private string message = "";
		private string[]? arguments;
		private int counter = 0;
		string searchTerm = "";
		private bool languageAndCategoryListing = false;
		protected override async Task OnInitializedAsync()
		{
			arguments = Environment.GetCommandLineArgs();
			if (arguments == null || arguments.Length == 0)
			{
				return;
			}
			if (arguments.Count() < 2)
			{
				arguments = new string[] { arguments[0], "SearchIntelliSense", "Blazor", "Snippet" };
			}
			if (arguments.Count() > 3 && arguments[1].Contains("SearchIntelliSense"))
			{
				string languageName = "";
				string categoryName = "";
				languageName = arguments[2].Replace("/", "").Trim();
				categoryName = arguments[3].Replace("/", "").Trim();
				var language = await LanguageService.GetLanguageAsync(languageName);
				var category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
				languageId = language.Id;
				categoryId = category.Id;
				languageAndCategoryListing = true;
				message = $"Got here line 38 With argument1 {arguments[1]} second argument {arguments[2]}";
			}
			else if (arguments.Length == 3)
			{
				searchTerm = arguments[2].Replace("/", "");
			}



		}
	}
}