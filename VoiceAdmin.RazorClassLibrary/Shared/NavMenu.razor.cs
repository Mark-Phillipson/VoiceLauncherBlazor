using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace RazorClassLibrary.Shared
{
    public partial class NavMenu
    {
        [Inject] NavigationManager? NavigationManager { get; set; }
        private bool collapseNavMenu = true;

        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;


        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
        private void LoadScripts()
        {
            NavigationManager!.NavigateTo($"commandsetoverview");
        }
    }
}