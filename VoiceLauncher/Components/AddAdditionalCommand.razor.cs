using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;


namespace VoiceLauncher.Components
{
	public partial class AddAdditionalCommand: ComponentBase 
	{
		[CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
		[Parameter] public ClaimsPrincipal? User { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[Parameter] public int CustomIntelliSenseId { get; set; }
		[Parameter] public int? AdditionalCommandId { get; set; }
		public AdditionalCommand AdditionalCommand { get; set; } = new AdditionalCommand();
		[Inject] public AdditionalCommandService? AdditionalCommandService { get; set; }
		[Inject] public GeneralLookupService? GeneralLookupService { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
		protected override async Task OnInitializedAsync()
		{
			if (AdditionalCommandId!= null )
			{
				AdditionalCommand =  await AdditionalCommandService!.GetAdditionalCommandAsync((int)AdditionalCommandId);
			}
			else
			{
				AdditionalCommand.CustomIntelliSenseId = CustomIntelliSenseId;
				AdditionalCommand.DeliveryType = "Send Keys";
			}
			GeneralLookups = await GeneralLookupService!.GetGeneralLookUpsAsync("Delivery Type"); 
		}
		public void Close()
		{
			ModalInstance!.CancelAsync();
		}

		protected async Task HandleValidSubmit()
		{
			await AdditionalCommandService!.SaveAdditionalCommand(AdditionalCommand);
			await ModalInstance!.CloseAsync(ModalResult.Ok(true));
			ToastService!.ShowSuccess($"{AdditionalCommand.SendKeysValue} Saved successfully.","SUCCESS");
		}
	}
}
