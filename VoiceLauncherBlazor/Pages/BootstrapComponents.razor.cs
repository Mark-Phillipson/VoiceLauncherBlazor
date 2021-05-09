using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
	public partial class BootstrapComponents
	{
		[Inject] IToastService ToastService { get; set; }

		protected override void OnAfterRender(bool firstRender)
		{
			if (firstRender)
			{
				ToastService.ShowInfo("To Blazor Server demo app", "Hello and Welcome");
				ToastService.ShowError("Demonstration of an error Toast Message", "Error Occurred");
				ToastService.ShowSuccess(" Demonstration of a success toast message ", "Success");
				ToastService.ShowWarning("demonstration of a warning toast message", "Warning");
			}
		}
	}
}