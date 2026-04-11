using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
	public partial class NavMenu : ComponentBase
	{
		[Inject] NavigationManager? NavigationManager { get; set; }
		[Parameter] public bool IsDarkTheme { get; set; }
		[Parameter] public string ThemeToggleLabel { get; set; } = "Switch theme";
		[Parameter] public EventCallback OnToggleTheme { get; set; }
		[Parameter] public bool IsSidebarCollapsed { get; set; }
		[Parameter] public EventCallback OnToggleSidebarCollapse { get; set; }
		private bool collapseNavMenu = true;
		private bool accessKeysEnabled = true;
		// Keep meaningful icons in collapsed mode; only show access-key glyphs when expanded.
		private bool ShowAccessKeys => accessKeysEnabled && !IsSidebarCollapsed;
		// Always show menu; avoid Bootstrap collapse hiding on widescreen
		private string? NavRootClass => IsSidebarCollapsed ? "collapsed" : null;
		protected override void OnInitialized()
		{
			if (Environment.MachineName == "J40L4V3")
			{
				accessKeysEnabled = true;
			}
			else
			{
				accessKeysEnabled = false;
			}

			// Default to expanded on initialization so the menu is visible on widescreen
			collapseNavMenu = false;
		}
		private void MenuItemSelected()
		{
			//enableAccessKeys = false;
			StateHasChanged();
		}
		private void ToggleNavMenu()
		{
			collapseNavMenu = !collapseNavMenu;
		}
		private void LoadCategories(string categoryType)
		{
			NavigationManager!.NavigateTo($"/categoriestable?categorytype={categoryType}", true);
			MenuItemSelected();
		}
		private void ToggleAccessKeys()
		{
			accessKeysEnabled = !accessKeysEnabled;
		}
		private void StartNewLauncher()
		{
			// Use client-side routing to avoid a full page reload which causes a theme flash
			NavigationManager!.NavigateTo("/launcheradd", false);
			MenuItemSelected();
		}
		private void StartNewCustomIntelliSense()
		{
			// Use client-side routing to avoid a full page reload which causes a theme flash
			NavigationManager!.NavigateTo("/customintellisense", false);
			MenuItemSelected();
		}
		private string? GetAccessKey(string key)
		{
			var result = accessKeysEnabled ? key : null;
			// StateHasChanged();
			return result;

		}

		private bool IsTalonSearchEnabled()
		{
			// Show Talon Search in all environments.
			return true;
		}
	}
}