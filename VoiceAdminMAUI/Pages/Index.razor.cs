using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace VoiceAdminMAUI.Pages
{
    public partial class Index
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }

        
    }
}