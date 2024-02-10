using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using RazorClassLibrary.helpers;

using VoiceLauncher.Services;

using WindowsInput;
using WindowsInput.Native;

namespace RazorClassLibrary.Pages
{
	public partial class CustomIntelliSenseAddEdit : ComponentBase
	{
		[Inject] public required IToastService ToastService { get; set; }
		[CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
		[Inject] public required NavigationManager NavigationManager { get; set; }
		[Parameter] public string? Title { get; set; }
		[Inject] public ILogger<CustomIntelliSenseAddEdit>? Logger { get; set; }
		[Inject] public required IJSRuntime JSRuntime { get; set; }
		[Parameter] public int? Id { get; set; }
		[Parameter] public int CategoryID { get; set; }
		[Parameter] public int LanguageId { get; set; }
		public CustomIntelliSenseDTO CustomIntelliSenseDTO { get; set; } = new CustomIntelliSenseDTO();//{ };
		[Inject] public required ICategoryDataService CategoryDataService { get; set; }

		private List<CategoryDTO> categories { get; set; } = new List<CategoryDTO>();
		[Inject] public required LanguageService LanguageService { get; set; }
		private List<DataAccessLibrary.Models.Language> languages { get; set; } = new List<DataAccessLibrary.Models.Language>();
		[Inject] public ICustomIntelliSenseDataService? CustomIntelliSenseDataService { get; set; }
		private string variable1 { get; set; } = "";
		private string variable2 { get; set; } = "";
		private string variable3 { get; set; } = "";
		private List<DataAccessLibrary.Models.GeneralLookup>? generalLookups { get; set; }
		private string example { get; set; } = "";
		[Inject] public required GeneralLookupService GeneralLookupDataService { get; set; }
		bool _saveOnly = false;
#pragma warning disable 414, 649
		bool TaskRunning = false;
#pragma warning restore 414, 649
		string Message = "";
		protected override async Task OnInitializedAsync()
		{
			if (CustomIntelliSenseDataService == null)
			{
				return;
			}
			if (Id > 0)
			{
				var result = await CustomIntelliSenseDataService.GetCustomIntelliSenseById((int)Id);
				if (result != null)
				{
					CustomIntelliSenseDTO = result;
				}
			}
			else
			{

				if (CategoryID > 0)
				{
					CustomIntelliSenseDTO.CategoryId = CategoryID;
				}
				if (LanguageId > 0)
				{
					CustomIntelliSenseDTO.LanguageId = LanguageId;
				}
				CustomIntelliSenseDTO.DeliveryType = "Copy and Paste";
				CustomIntelliSenseDTO.CommandType = "SendKeys";
			}
			categories = await CategoryDataService.GetAllCategoriesByTypeAsync("IntelliSense Command");
			languages = await LanguageService.GetLanguagesAsync();
			generalLookups = await GeneralLookupDataService.GetGeneralLookUpsAsync("Delivery Type");
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				try
				{
					if (JSRuntime != null)
					{
						await JSRuntime.InvokeVoidAsync("window.setFocus", "LanguageId");
					}
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
				}
			}
		}
		public async Task CloseAsync()
		{
			if (ModalInstance != null)
				await ModalInstance.CancelAsync();
		}
		protected async Task HandleValidSubmit()
		{
			TaskRunning = true;
			if ((Id == 0 || Id == null) && CustomIntelliSenseDataService != null)
			{
				CustomIntelliSenseDTO? result = await CustomIntelliSenseDataService.AddCustomIntelliSense(CustomIntelliSenseDTO);
				if (result == null && Logger != null)
				{
					Logger.LogError("Custom Intelli Sense failed to add, please investigate Error Adding New Custom Intelli Sense");
					ToastService?.ShowError("Custom Intelli Sense failed to add, please investigate Error Adding New Custom Intelli Sense");
					return;
				}
				ToastService?.ShowSuccess("Custom Intelli Sense added successfully");
			}
			else
			{
				if (CustomIntelliSenseDataService != null)
				{
					await CustomIntelliSenseDataService!.UpdateCustomIntelliSense(CustomIntelliSenseDTO, "");
					ToastService?.ShowSuccess("The Custom Intelli Sense updated successfully");
				}
			}
			if (ModalInstance != null && !_saveOnly)
			{
				await ModalInstance.CloseAsync(ModalResult.Ok(true));
			}
			_saveOnly = false;
			TaskRunning = false;
		}
		private async Task CopyItemAsync(string itemToCopy)
		{
			if (string.IsNullOrEmpty(itemToCopy)) { return; }
			itemToCopy = FillInVariables(itemToCopy);
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);
				var message = $"Copied Successfully: '{itemToCopy}'";
				ToastService!.ShowSuccess(message + " Copy Item");
			}

		}

		private string FillInVariables(string itemToCopy)
		{
			if (itemToCopy.Contains("`Variable1`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable1))
			{
				itemToCopy = itemToCopy.Replace("`Variable1`", CustomIntelliSenseDTO.Variable1);
			}
			if (itemToCopy.Contains("`Variable2`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable2))
			{
				itemToCopy = itemToCopy.Replace("`Variable2`", CustomIntelliSenseDTO.Variable2);
			}
			if (itemToCopy.Contains("`Variable3`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable3))
			{
				itemToCopy = itemToCopy.Replace("`Variable3`", CustomIntelliSenseDTO.Variable3);
			}

			return itemToCopy;
		}

		private async Task CopyAndPasteAsync(string itemToCopyAndPaste)
		{
			itemToCopyAndPaste = FillInVariables(itemToCopyAndPaste);
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopyAndPaste);
				var message = $"Copied Successfully: '{itemToCopyAndPaste}'";
				InputSimulator simulator = new InputSimulator();
				simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
				simulator.Keyboard.Sleep(100);
				simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
				simulator.Keyboard.Sleep(100);
				simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
				ToastService!.ShowSuccess(message + " Success");
			}
		}
		private async Task CallChangeAsync(string elementId)
		{
			if (JSRuntime == null)
			{
				return;
			}
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			example = FillInVariables(CustomIntelliSenseDTO.SendKeysValue);
		}
		private async Task DeleteRecord()
		{
			if (Id > 0)
			{
				await CustomIntelliSenseDataService!.DeleteCustomIntelliSense((int)Id);
				NavigationManager.NavigateTo("/intellisenses");
			}
		}
		private void CreateSnippet()
		{
			var result = ManageSnippets.CreateSnippet(CustomIntelliSenseDTO);
			Message = result;
		}
		private async Task CreateVisualStudioCodeSnippetAsync()
		{
			var result = ManageSnippets.CreateVisualStudioCodeSnippet(CustomIntelliSenseDTO);
			await CopyItemAsync(result);
			Message = result;
		}
		private void ResetMessage()
		{
			Message = string.Empty;
		}
		private void ToggleSaveOnly()
		{
			_saveOnly = !_saveOnly;
		}
	}
}