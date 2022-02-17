using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace VoiceLauncher.Pages
{
	public partial class LanguageEdit
	{
		[Parameter] public int LanguageId { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		public DataAccessLibrary.Models.Language? LanguageModel { get; set; }
#pragma warning disable 414
		private EditContext? _editContext;
		private bool _loadFailed = false;
#pragma warning restore 414


		protected override async Task OnInitializedAsync()
		{
			if (LanguageId > 0)
			{
				try
				{
					LanguageModel = await LanguageService.GetLanguageAsync(LanguageId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				LanguageModel = new DataAccessLibrary.Models.Language
				{
					Active = true
				};
			}
            if (LanguageModel!= null )
            {
                _editContext = new EditContext(LanguageModel); 
            }

		}

		protected override async Task OnParametersSetAsync()
		{
			if (LanguageId > 0)
			{
				try
				{
					LanguageModel = await LanguageService.GetLanguageAsync(LanguageId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}
		private async Task HandleValidSubmit()
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await LanguageService.SaveLanguage(LanguageModel);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			NavigationManager.NavigateTo("languages");
		}
	}
}
