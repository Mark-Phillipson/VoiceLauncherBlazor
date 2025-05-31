using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;

namespace WinFormsApp
{
	public partial class Index : ComponentBase
	{
		[Inject][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public required LanguageService LanguageService { get; set; }
		[Inject][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public required CategoryService CategoryService { get; set; }
		[Inject][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public required IJSRuntime JSRuntime { get; set; }
		[Inject][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public required LauncherService LauncherService { get; set; }
		[Inject][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public required ILauncherDataService LauncherDataService { get; set; }
		private int languageId;
		private int categoryId;
		private string message = "";
		private string[]? arguments;
		string searchTerm = "";
		private bool languageAndCategoryListing = false;
		private bool launcher = false;
		private bool refreshRequested;
		private bool showAIChat = false;

		protected override async Task OnInitializedAsync()
		{
			arguments = Environment.GetCommandLineArgs();
			if (arguments == null || arguments.Length == 0)
			{
				return;
			}
			if (arguments.Count() < 2)
			{
				//  arguments = new string[] { arguments[0], "SearchIntelliSense", "Blazor", "Snippet" };
				// arguments = new string[] { arguments[0], "SearchIntelliSense", "Not Applicable", "Folders" };
				 arguments = new string[] { arguments[0], "Launcher", "Code Projects" };
			}
			string categoryName = "";
			if (arguments.Count() > 3 && arguments[1].Contains("SearchIntelliSense"))
			{
				SetTitle("Search Snippets");
				string languageName = "";
				languageName = arguments[2].Replace("/", "").Trim();
				categoryName = arguments[3].Replace("/", "").Trim();
				var language = await LanguageService.GetLanguageAsync(languageName);
				var category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
				if (language != null && category != null)
				{
					languageId = language.Id;
					categoryId = category.Id;
				}
				languageAndCategoryListing = true;
				message = $"Got here line 38 With argument1 {arguments[1]} second argument {arguments[2]}";
			}
			else if (arguments.Length == 3 && arguments[1].Contains("Launcher"))
			{
				categoryName = arguments[2].Replace("/", "");
				var category = await CategoryService.GetCategoryAsync(categoryName, "Launch Applications");
				if (category != null)
				{
					categoryId = category.Id;
				}
				SetTitle($"Launch from category: {categoryName}");
				launcher = true;
			}
			else if (arguments.Length == 3)
			{
				searchTerm = arguments[2].Replace("/", "");
				SetTitle("Filtering Snippets by Display Value");
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
		}		private async void SetTitle(string title)
		{
			await SetTitleCallback.InvokeAsync(title);
		}
		
		private void ShowAIChat()
		{
			showAIChat = !showAIChat;
			if (showAIChat)
			{
				// Reset other views when showing AI Chat
				languageAndCategoryListing = false;
				launcher = false;
				SetTitle("AI Chat Assistant");
			}
			else
			{
				// Restore previous view based on arguments
				if (arguments != null && arguments.Length > 1)
				{
					if (arguments[1].Contains("SearchIntelliSense"))
					{
						languageAndCategoryListing = true;
						SetTitle("Search Snippets");
					}
					else if (arguments[1].Contains("Launcher"))
					{
						launcher = true;
						SetTitle($"Launch Applications");
					}
					else
					{
						SetTitle("Filtering Snippets by Display Value");
					}
				}
			}
			StateHasChanged();
		}
		
		private async Task RefreshCache()
		{
			// Invalidate both legacy and modern service caches
			LauncherService.InvalidateCache();
			await LauncherDataService.DeleteLauncher(-1); // This will trigger cache invalidation without deleting anything
			
			// Toggle refresh flag to force component reload
			refreshRequested = !refreshRequested;
			
			// Force refresh of current view
			StateHasChanged();
		}
		[Parameter][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public EventCallback CloseWindowCallback { get; set; }
		[Parameter][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public EventCallback MaximizeWindowCallback { get; set; }
		[Parameter][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public EventCallback MinimizeWindowCallback { get; set; }
		[Parameter][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public EventCallback RestoreWindowCallback { get; set; }
		[Parameter][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public EventCallback<string> SetTitleCallback { get; set; }
	}
}