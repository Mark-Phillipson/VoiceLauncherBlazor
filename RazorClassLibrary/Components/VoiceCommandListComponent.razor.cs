using Blazored.Toast.Services;
using SharedContracts.Models;
using CommandSet = SharedContracts.Models.CommandSetDto;
using VoiceCommand = SharedContracts.Models.VoiceCommandDto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Components
{
	public partial class VoiceCommandListComponent : ComponentBase
	{
		string _result = "";
		[Parameter] public CommandSet? CommandSet { get; set; }
		[Inject] public IJSRuntime? JavaScriptRuntime { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[Parameter] public VoiceCommand? VoiceCommand { get; set; }
		[Parameter] public bool Show { get; set; }
		List<string> ListNames { get; set; } = new List<string>();
		string? _listContents;
		protected override void OnParametersSet()
		{
			var name = VoiceCommand!.Name;
			var startCharacter = "<";
			var endCharacter = ">";
			if (VoiceCommand!.TargetApplication!.CommandSource == "Dragon")
			{
				startCharacter = "[";
				endCharacter = "]";
			}
			ListNames = new List<string>();
			if (name != null && !string.IsNullOrWhiteSpace(name) && name.Contains(startCharacter) && name.Contains(endCharacter))
			{
				do
				{
					var position1 = name.IndexOf(startCharacter);
					var position2 = name.IndexOf(endCharacter);
					position1++;
					var listName = name.Substring(position1, position2 - position1);
					ListNames.Add(listName);
					name = name.Substring(position2 + 1);
				} while (name.Contains(startCharacter) && name.Contains(endCharacter));
			}
		}
		private async Task CopyTextToClipboard(string? text)
		{
			if (text == null) return;
			await JavaScriptRuntime!.InvokeVoidAsync(
				 "clipboardCopy.copyText", text);
			_result = $"Copied List Contents Successfully at {DateTime.Now:hh:mm}";
			ToastService?.ShowSuccess(_result);
		}

	}
}