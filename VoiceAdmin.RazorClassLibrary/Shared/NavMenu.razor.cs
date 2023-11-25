﻿using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared
{
	public partial class NavMenu
	{
		[Inject] NavigationManager? NavigationManager { get; set; }
		private bool collapseNavMenu = true;

		private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
		private bool enableAccessKeys = true;
		private void MenuItemSelected()
		{
			enableAccessKeys = false;
			StateHasChanged();
		}
		private void EnableAccessKeys()
		{
			enableAccessKeys = true;
			StateHasChanged();
		}
		private void ToggleNavMenu()
		{
			collapseNavMenu = !collapseNavMenu;
		}
		private void LoadScripts()
		{
			enableAccessKeys = false;
			StateHasChanged();
			NavigationManager!.NavigateTo($"commandsetoverview");
		}
	}
}