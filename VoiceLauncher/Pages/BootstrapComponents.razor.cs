using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;

namespace VoiceLauncher.Pages
{
    public partial class BootstrapComponents
    {
        [Inject] IToastService? ToastService { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (ToastService != null)
                {
                    ToastService.ShowInfo("To Blazor Server demo app", "Hello and Welcome");
                    ToastService.ShowError("Demonstration of an error Toast Message", "Error Occurred");
                    ToastService.ShowSuccess(" Demonstration of a success toast message ", "Success");
                    ToastService.ShowWarning("demonstration of a warning toast message", "Warning");
                }
            }
        }
    }
}