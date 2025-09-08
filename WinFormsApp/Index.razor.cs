using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Linq;

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
		private bool languageAndCategoryListing = false;		private bool launcher = false;
		private bool refreshRequested;
		private bool showAIChat = false;
		private bool showTalonSearch = false;	// Property for dynamic AI Chat button caption
	private string AIChatButtonCaption => showAIChat ? 
		(arguments != null && arguments.Length > 1 && 
		 ((arguments.Length >= 2 && arguments[1].Contains("AIChat")) || 
		  (arguments.Length >= 3 && arguments[2].Contains("AIChat"))) ? "Close" : "← Back") : 
		"Chat";
	// Property for dynamic Talon Search button caption
	private string TalonSearchButtonCaption => showTalonSearch ?
		(arguments != null && arguments.Length > 1 &&
		 ((arguments.Length >= 2 && (arguments[1].Contains("Talon") || arguments[1].Contains("search"))) ||
		  (arguments.Length >= 3 && (arguments[2].Contains("Talon") || arguments[2].Contains("search")))) ? "Close" : "← Back") :
		"Talon Search";
		protected override async Task OnInitializedAsync()
		{
			arguments = Environment.GetCommandLineArgs();

			// Debug: Log all command line arguments
			System.Diagnostics.Debug.WriteLine($"Command line arguments count: {arguments?.Length ?? 0}");
			if (arguments != null)
			{
				for (int i = 0; i < arguments.Length; i++)
				{
					System.Diagnostics.Debug.WriteLine($"Argument {i}: '{arguments[i]}'");
				}
			}

			if (arguments == null || arguments.Length == 0)
			{
				return;
			}       // Check for search/Talon command line arguments
			if (arguments.Length >= 2 &&
				(arguments[1].Equals("search", StringComparison.OrdinalIgnoreCase) ||
				 arguments[1].Equals("Talon", StringComparison.OrdinalIgnoreCase)))
			{
				showTalonSearch = true;
				// Check if there are additional arguments to use as search terms
				if (arguments.Length >= 3)
				{
					// Join all arguments from index 2 onwards as the search term
					searchTerm = string.Join(" ", arguments.Skip(2));

					// Clean up the search term - remove forward slashes and trim
					searchTerm = searchTerm.Replace("/", "").Trim();

					System.Diagnostics.Debug.WriteLine($"Search term set to: '{searchTerm}'");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("No additional arguments for search term");
				}

					// Normalize arguments so downstream code sees the Talon search invocation
					var exeName = (arguments != null && arguments.Length > 0) ? arguments[0] : Environment.GetCommandLineArgs().FirstOrDefault() ?? string.Empty;
					arguments = new[] { exeName, "Talon", searchTerm ?? string.Empty };

					return;
			}

			if (arguments.Count() < 2)
			{               //  arguments = new string[] { arguments[0], "SearchIntelliSense", "Blazor", "Snippet" };
							// arguments = new string[] { arguments[0], "SearchIntelliSense", "Not Applicable", "Folders" };
							// arguments = new string[] { arguments[0], "Launcher", "AIChat" };
							// arguments = new string[] { arguments[0], "search" };
							//  arguments = new string[] { arguments[0], "Launcher", "Code Projects" };
							arguments = new string[] { arguments[0], "Talon", "" };
				
			}
			string categoryName = "";
			if (arguments.Count() >= 2 && arguments[1].Contains("AIChat"))
			{
				SetTitle("AI Chat");
				showAIChat = true;
			}
			else if (arguments.Count() >= 3 && arguments[2].Contains("AIChat"))
			{
				SetTitle("AI Chat");
				showAIChat = true;
			}
			else if (arguments.Count() >= 2 && (arguments[1].Contains("Talon") || arguments[1].Contains("search")))
			{
				SetTitle("Talon Voice Command Search");
				showTalonSearch = true;

				// Check if there are additional arguments to use as search terms
				if (arguments.Length >= 3)
				{
					// Join all arguments from index 2 onwards as the search term
					searchTerm = string.Join(" ", arguments.Skip(2));
					// Clean up the search term - remove forward slashes and trim
					searchTerm = searchTerm.Replace("/", "").Trim();

					// Normalize arguments for downstream components
					var exeName = (arguments != null && arguments.Length > 0) ? arguments[0] : Environment.GetCommandLineArgs().FirstOrDefault() ?? string.Empty;
					arguments = new[] { exeName, "Talon", searchTerm ?? string.Empty };
				}
			}
			else if (arguments.Count() >= 3 && (arguments[2].Contains("Talon") || arguments[2].Contains("search")))
			{
				SetTitle("Talon Voice Command Search");
				showTalonSearch = true;

				// Check if there are additional arguments to use as search terms  
				if (arguments.Length >= 4)
				{
					// Join all arguments from index 3 onwards as the search term
					searchTerm = string.Join(" ", arguments.Skip(3));
					// Clean up the search term - remove forward slashes and trim
					searchTerm = searchTerm.Replace("/", "").Trim();

					// Normalize arguments for downstream components
					var exeName2 = (arguments != null && arguments.Length > 0) ? arguments[0] : Environment.GetCommandLineArgs().FirstOrDefault() ?? string.Empty;
					arguments = new[] { exeName2, "Talon", searchTerm ?? string.Empty };
				}
			}
			else if (arguments.Count() > 3 && arguments[1].Contains("SearchIntelliSense"))
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
		}			private void ShowAIChat()
	{
		if (showAIChat)
		{
			// Turning off AI Chat - restore previous view based on arguments
			showAIChat = false;
			if (arguments != null && arguments.Length > 1)
			{
				if ((arguments.Length >= 2 && arguments[1].Contains("AIChat")) || 
				    (arguments.Length >= 3 && arguments[2].Contains("AIChat")))
				{
					// If we launched directly into AI chat, close the application when going back
					CloseWindow();
					return;
				}
				else if (arguments[1].Contains("SearchIntelliSense"))
				{
					languageAndCategoryListing = true;
					SetTitle("Search Snippets");
				}				else if (arguments[1].Contains("Launcher"))
				{
					launcher = true;
					SetTitle($"Launch Applications");
				}
				else if (arguments[1].Contains("Talon") || arguments[1].Contains("search"))
				{
					showTalonSearch = true;
					SetTitle("Talon Voice Command Search");
				}
				else
				{
					SetTitle("Filtering Snippets by Display Value");
				}
			}
		}
		else
		{
			// Turning on AI Chat
			showAIChat = true;
			// Reset other views when showing AI Chat
			languageAndCategoryListing = false;
			launcher = false;
			showTalonSearch = false;
			SetTitle("AI Chat Assistant");
		}
		StateHasChanged();
	}

	private void ShowTalonSearch()
	{		if (showTalonSearch)
		{
			// Turning off Talon Search - restore previous view based on arguments
			showTalonSearch = false;
			if (arguments != null && arguments.Length > 1)
			{
				if ((arguments.Length >= 2 && (arguments[1].Contains("Talon") || arguments[1].Contains("search"))) ||
				    (arguments.Length >= 3 && (arguments[2].Contains("Talon") || arguments[2].Contains("search"))))
				{
					// If we launched directly into Talon search, close the application when going back
					CloseWindow();
					return;
				}
				else if (arguments[1].Contains("SearchIntelliSense"))
				{
					languageAndCategoryListing = true;
					SetTitle("Search Snippets");
				}
				else if (arguments[1].Contains("Launcher"))
				{
					launcher = true;
					SetTitle($"Launch Applications");
				}
				else if (arguments[1].Contains("AIChat"))
				{
					showAIChat = true;
					SetTitle("AI Chat Assistant");
				}
				else
				{
					SetTitle("Filtering Snippets by Display Value");
				}
			}
		}
		else
		{
			// Turning on Talon Search
			showTalonSearch = true;
			// Reset other views when showing Talon Search
			languageAndCategoryListing = false;
			launcher = false;
			showAIChat = false;
			SetTitle("Talon Voice Command Search");
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