using Microsoft.AspNetCore.Components;

namespace VoiceLauncher.Pages
{
	public partial class Loading
	{
		[Inject] NavigationManager? NavigationManager { get; set; }
		void GoHome()
		{
			NavigationManager!.NavigateTo("/");
		}
	}
}