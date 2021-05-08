using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace VoiceLauncherBlazor.Pages
{
	public partial class Loading
	{
		[Inject] NavigationManager NavigationManager { get; set; }
		void GoHome()
		{
			NavigationManager.NavigateTo("/");
		}
	}
}