using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace VoiceAdminMAUI.Pages
{
    public partial class Index
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }

        private void LaunchVoiceAdmin()
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = @"C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceLauncher\bin\Release\net7.0\publish\VoiceLauncher.exe";
            psi.WorkingDirectory = @"C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceLauncher\bin\Release\net7.0\publish\";
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
            Process.Start(psi);
            string commandIdParameter = "";
            var uri = $"http://localhost:5000/windowsspeechvoicecommands{commandIdParameter}";
            psi = new System.Diagnostics.ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = uri;
            Process.Start(psi);
        }
    }
}