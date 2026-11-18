using Blazored.Toast.Services;
using DataAccessLibrary.Models.KnowbrainerCommands;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Components
{
	public partial class VoiceCommandContentComponent : ComponentBase
	{
		[Inject] public IJSRuntime? JavaScriptRuntime { get; set; }
		[Inject] public required IToastService ToastService { get; set; }
		[Parameter] public VoiceCommandContent? VoiceCommandContent { get; set; }
		[Parameter] public string? SearchTermCodeContains { get; set; } = "";
		public string? _result { get; set; }
		private async Task CopyTextToClipboard(string text)
		{
			if (await JavaScriptRuntime!.InvokeVoidAsyncSafe("clipboardCopy.copyText", text))
			{
				_result = $"Copied Successfully at {DateTime.Now:hh:mm}";
				ToastService.ShowSuccess(_result);
			}
			else
			{
				ToastService.ShowError("Unable to copy the text to the clipboard.");
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await JavaScriptRuntime!.InvokeVoidAsyncSafe("Prism.highlightAll");
		}
	}
}