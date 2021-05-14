using Microsoft.AspNetCore.Components;

namespace VoiceLauncherBlazor.Pages
{
	public partial class PopularCommands
	{
		[Inject] NavigationManager NavigationManager { get; set; }
		public bool ShowVideo { get; set; } = false;
		void FilterCommand(string commandToFilter, string applicationToFilter = "devenv", bool showLists = false, string youtubeUrl= null )
		{
			if (ShowVideo && youtubeUrl!= null )
			{
				NavigationManager.NavigateTo(youtubeUrl,true);
			}
			else
			{
				var showListsString = showLists ? "true" : "false";
				commandToFilter = commandToFilter.Replace(" ", "%20");
				NavigationManager.NavigateTo($"commandsetoverview?name={commandToFilter}&application={applicationToFilter}&viewnew=false&showcommands=true&showlists={showListsString}&showcode=true");
			}
		}
	}
}