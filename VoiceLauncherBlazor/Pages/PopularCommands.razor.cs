using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace VoiceLauncherBlazor.Pages
{
	public partial class PopularCommands
	{
		[Inject] NavigationManager NavigationManager { get; set; }
		void FilterCommand(string commandToFilter,string applicationToFilter="devenv",bool showLists=false)
		{
			var showListsString = showLists ? "true" : "false";
			commandToFilter = commandToFilter.Replace(" ", "%20");
			NavigationManager.NavigateTo($"commandsetoverview?name={commandToFilter}&application={applicationToFilter}&viewnew=false&showcommands=true&showlists={showListsString}&showcode=true");
		}
	}
}