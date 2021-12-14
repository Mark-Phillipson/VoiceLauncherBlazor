using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace VoiceLauncher.Pages
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