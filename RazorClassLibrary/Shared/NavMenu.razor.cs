﻿using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
	public partial class NavMenu : ComponentBase
	{
		[Inject] NavigationManager? NavigationManager { get; set; }
		private bool collapseNavMenu = true;
		private bool accessKeysEnabled = true;
		private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
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
		private string? GetAccessKey(string key)
		{
			var result = accessKeysEnabled ? key : null;
			// StateHasChanged();
			return result;

		}
	}
}