using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
	public partial class NavMenu : ComponentBase
	{
		[Inject] NavigationManager? NavigationManager { get; set; }
		private bool collapseNavMenu = true;

		private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
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
	}
}