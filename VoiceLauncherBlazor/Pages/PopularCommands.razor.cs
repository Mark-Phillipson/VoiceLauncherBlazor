using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoiceLauncherBlazor.Pages
{
	public partial class PopularCommands
	{
		[Inject] public IJSRuntime JSRuntime { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }
		public bool ShowVideo { get; set; } = true;
		async System.Threading.Tasks.Task FilterCommandAsync(string commandToFilter, string applicationToFilter = "devenv", bool showLists = false, string youtubeUrl = null)
		{
			if (ShowVideo && youtubeUrl != null)
			{
				//NavigationManager.NavigateTo(youtubeUrl,true);
				await JSRuntime.InvokeAsync<object>("open", youtubeUrl, "_blank");
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