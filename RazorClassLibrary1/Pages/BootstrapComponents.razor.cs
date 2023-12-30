using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Pages
{
    public partial class BootstrapComponents : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (ToastService != null)
                {
                    ToastService.ShowInfo("To Blazor Server demo app");
                    ToastService.ShowError("Demonstration of an error Toast Message");
                    ToastService.ShowSuccess(" Demonstration of a success toast message ");
                    ToastService.ShowWarning("demonstration of a warning toast message");
                }
            }
        }
    }
}