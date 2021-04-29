using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace VoiceLauncherBlazor.Pages
{
	public partial class PopularCommands
	{
		[Inject] NavigationManager NavigationManager { get; set; }
		void FilterCommand(string commandToFilter,string applicationToFilter="devenv")
		{
			commandToFilter = commandToFilter.Replace(" ", "%20");
			NavigationManager.NavigateTo($"commandsetoverview?name={commandToFilter}&application={applicationToFilter}");
		}
	}
}