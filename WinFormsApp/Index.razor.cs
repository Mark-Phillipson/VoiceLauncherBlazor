using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinFormsApp
{
	public partial class Index : ComponentBase
	{
		[Inject] public required LanguageService LanguageService { get; set; }
		[Inject] public required CategoryService CategoryService { get; set; }
		[Inject] public required IJSRuntime JSRuntime { get; set; }

		private int languageId;
		private int categoryId;
		private string message = "";
		private string[]? arguments;
		string searchTerm = "";
		private bool languageAndCategoryListing = false;
		private bool launcher = false;
		protected override async Task OnInitializedAsync()
		{
			arguments = Environment.GetCommandLineArgs();
			if (arguments == null || arguments.Length == 0)
			{
				return;
			}
			if (arguments.Count() < 2)
			{
				arguments = new string[] { arguments[0], "SearchIntelliSense", "Blazor" };
				//arguments = new string[] { arguments[0], "SearchIntelliSense", "Not Applicable", "Folders" };
				//arguments = new string[] { arguments[0], "Launcher", "Folders" };
			}
			string categoryName = "";
			if (arguments.Count() > 3 && arguments[1].Contains("SearchIntelliSense"))
			{
				SetTitle("Search IntelliSense");
				string languageName = "";
				languageName = arguments[2].Replace("/", "").Trim();
				categoryName = arguments[3].Replace("/", "").Trim();
				var language = await LanguageService.GetLanguageAsync(languageName);
				var category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
				languageId = language.Id;
				categoryId = category.Id;
				languageAndCategoryListing = true;
				message = $"Got here line 38 With argument1 {arguments[1]} second argument {arguments[2]}";
			}
			else if (arguments.Length == 3 && arguments[1].Contains("Launcher"))
			{
				categoryName = arguments[2].Replace("/", "");
				var category = await CategoryService.GetCategoryAsync(categoryName, "Launch Applications");
				categoryId = category.Id;
				SetTitle($"Launch from category: {categoryName}");
				launcher = true;
			}
			else if (arguments.Length == 3)
			{
				searchTerm = arguments[2].Replace("/", "");
				SetTitle("Filtering by Display Value");
			}
		}
		private async void CloseWindow()
		{
			await CloseWindowCallback.InvokeAsync();
		}
		private async void MaximizeWindow()
		{
			await MaximizeWindowCallback.InvokeAsync();
		}
		private async void MinimizeWindow()
		{
			await MinimizeWindowCallback.InvokeAsync();
		}
		private async void RestoreWindow()
		{
			await RestoreWindowCallback.InvokeAsync();
		}
		private async void SetTitle(string title)
		{
			await SetTitleCallback.InvokeAsync(title);
		}
		[Parameter] public EventCallback CloseWindowCallback { get; set; }
		[Parameter] public EventCallback MaximizeWindowCallback { get; set; }
		[Parameter] public EventCallback MinimizeWindowCallback { get; set; }
		[Parameter] public EventCallback RestoreWindowCallback { get; set; }
		[Parameter] public EventCallback<string> SetTitleCallback { get; set; }
	}
}