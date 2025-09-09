using Blazored.Toast.Services;
using SharedContracts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Components
{
	public partial class VoiceCommandContentComponent : ComponentBase
	{
		[Inject] public IJSRuntime? JavaScriptRuntime { get; set; }
		[Inject] public required IToastService ToastService { get; set; }
	[Parameter] public SharedContracts.Models.VoiceCommandContentDto? VoiceCommandContent { get; set; }
		[Parameter] public string? SearchTermCodeContains { get; set; } = "";
		public string? _result { get; set; }
		private async Task CopyTextToClipboard(string text)
		{
			await JavaScriptRuntime!.InvokeVoidAsync(
				 "clipboardCopy.copyText", text);
			_result = $"Copied Successfully at {DateTime.Now:hh:mm}";
			ToastService.ShowSuccess(_result);
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await JavaScriptRuntime!.InvokeVoidAsync("Prism.highlightAll");
		}
	}
}