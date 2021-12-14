using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Pages
{
	public partial class GeneralLookup
	{
		[Inject] IToastService ToastService { get; set; }
		[Parameter] public int generalLookupId { get; set; }
		private EditContext _editContext;
		public DataAccessLibrary.Models.GeneralLookup generalLookup { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414

		protected override async Task OnInitializedAsync()
		{
			if (generalLookupId > 0)
			{
				try
				{
					generalLookup = await GeneralLookupService.GetGeneralLookupAsync(generalLookupId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				generalLookup = new DataAccessLibrary.Models.GeneralLookup
				{
					Category = "Default?",
					SortOrder = 1

				};
			}
			_editContext = new EditContext(generalLookup);
			try
			{
				generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync(generalLookup.Category);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		protected override async Task OnParametersSetAsync()
		{
			if (generalLookupId > 0)
			{
				try
				{
					generalLookup = await GeneralLookupService.GetGeneralLookupAsync(generalLookupId);
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
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await GeneralLookupService.SaveGeneralLookup(generalLookup);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			NavigationManager.NavigateTo("generallookups");
		}
	}
}
