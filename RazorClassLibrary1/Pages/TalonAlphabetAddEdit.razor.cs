
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using System.Security.Claims;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.DTO;

namespace RazorClassLibrary.Pages
{
	public partial class TalonAlphabetAddEdit : ComponentBase
	{
		[Inject] IToastService? ToastService { get; set; }
		[CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
		[Parameter] public string? Title { get; set; }
		[Inject] public ILogger<TalonAlphabetAddEdit>? Logger { get; set; }
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		[Parameter] public int? Id { get; set; }
		public TalonAlphabetDTO TalonAlphabetDTO { get; set; } = new TalonAlphabetDTO();//{ };
		[Inject] public ITalonAlphabetDataService? TalonAlphabetDataService { get; set; }
		[Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
		bool TaskRunning = false;
#pragma warning restore 414, 649
		protected override async Task OnInitializedAsync()
		{
			if (TalonAlphabetDataService == null)
			{
				return;
			}
			if (Id != null && Id != 0)
			{
				var result = await TalonAlphabetDataService.GetTalonAlphabetById((int)Id);
				if (result != null)
				{
					TalonAlphabetDTO = result;
				}
			}
			else
			{
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				try
				{
					if (JSRuntime != null)
					{
						await JSRuntime.InvokeVoidAsync("window.setFocus", "Letter");
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
			if (Environment.MachineName != "J40L4V3")
			{
				ToastService?.ShowWarning("This is a demo application and editing is not allowed");
				return;
			}
			TaskRunning = true;
			if ((Id == 0 || Id == null) && TalonAlphabetDataService != null)
			{
				TalonAlphabetDTO? result = await TalonAlphabetDataService.AddTalonAlphabet(TalonAlphabetDTO);
				if (result == null && Logger != null)
				{
					Logger.LogError("Talon Alphabet failed to add, please investigate Error Adding New Talon Alphabet");
					ToastService?.ShowError("Talon Alphabet failed to add, please investigate Error Adding New Talon Alphabet");
					return;
				}
				ToastService?.ShowSuccess("Talon Alphabet added successfully");
			}
			else
			{
				if (TalonAlphabetDataService != null)
				{
					await TalonAlphabetDataService!.UpdateTalonAlphabet(TalonAlphabetDTO, "");
					ToastService?.ShowSuccess("The Talon Alphabet updated successfully");
				}
			}
			if (ModalInstance != null)
			{
				await ModalInstance.CloseAsync(ModalResult.Ok(true));
			}
			TaskRunning = false;
		}
	}
}