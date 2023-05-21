using DataAccessLibrary.Models.KnowbrainerCommands;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Components
{
    public partial class VoiceCommandContentComponent : ComponentBase
    {
        [Inject] public IJSRuntime? JavaScriptRuntime { get; set; }
        [Parameter] public VoiceCommandContent? VoiceCommandContent { get; set; }
        public string? Result { get; set; }
        private async Task CopyTextToClipboard(string text)
        {
            await JavaScriptRuntime!.InvokeVoidAsync(
                "clipboardCopy.copyText", text);
            Result = $"Copied Successfully at {DateTime.Now:hh:mm}";
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JavaScriptRuntime!.InvokeVoidAsync("Prism.highlightAll");
        }
    }
}