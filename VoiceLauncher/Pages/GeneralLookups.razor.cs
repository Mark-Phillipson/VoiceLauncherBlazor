using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceLauncher.Pages
{
	public partial class GeneralLookups
	{
		public bool ShowDialog { get; set; }
		[Inject] IToastService ToastService { get; set; }
		public bool ShowValidationWarning { get; set; }
		private int generalLookupIdDelete { get; set; }
		public string StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
		public List<string> generalLookupsCategories { get; set; }
		public int AlertDuration { get; set; }
		private string categoryFilter { get; set; }
		private string searchTerm;
		public string AlertType { get; set; } = "success";
		public bool ShowAlert { get; set; } = true;
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414

		public string SearchTerm
		{
			get => searchTerm;
			set
			{
				if (searchTerm != value)
				{
					searchTerm = value;
				}
			}
		}
		protected override async Task OnInitializedAsync()
		{
			try
			{
				generalLookups = await GeneralLookupService.GetGeneralLookupsAsync();
				generalLookupsCategories = await GeneralLookupService.GetGeneralLookUpsCategoriesAsync();
				StatusMessage = "Welcome to the general lookups page!";
				ShowAlert = true;
				AlertDuration = 6000;
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		async Task ApplyFilter()
		{
			if (SearchTerm != null)
			{
				try
				{
					generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(SearchTerm.Trim());
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}

		void HandleValidSubmit()
		{
			Console.WriteLine("OnValidSubmit");
		}
		void ConfirmDelete(int generalLookupId)
		{
			ShowDialog = true;
			generalLookupIdDelete = generalLookupId;
		}
		void CancelDialog()
		{
			ShowDialog = false;
		}
		async Task SortGeneralLookups(string column, string sortType)
		{
			try
			{
				generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm, column, sortType);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task DeleteGeneralLookup(int generalLookupId)
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await GeneralLookupService.DeleteGeneralLookup(generalLookupId);
				StatusMessage = result;
				AlertDuration = 7000;
				AlertType = "danger";
				ShowAlert = true;
				ShowDialog = false;
				generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task FilterGeneralLookups()
		{
			if (categoryFilter != null)
			{
				try
				{
					generalLookups = await GeneralLookupService.GetGeneralLookupsAsync(null, null, null, categoryFilter);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}
		async Task SaveAllGeneralLookups()
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				generalLookups = await GeneralLookupService.SaveAllGeneralLookups(generalLookups);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			StatusMessage = $"General Lookups Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
			AlertDuration = 6000;
			ShowAlert = true;
			AlertType = "info";
		}
		private void NotifyInvalid()
		{
			ShowValidationWarning = true;
		}
		private async Task DuplicateRecord(int generalLookupId)
		{
			DataAccessLibrary.Models.GeneralLookup generalLookupSource = await GeneralLookupService.GetGeneralLookupAsync(generalLookupId);
			DataAccessLibrary.Models.GeneralLookup generalLookup = new DataAccessLibrary.Models.GeneralLookup
			{
				Category = generalLookupSource?.Category,
				DisplayValue = generalLookupSource?.DisplayValue,
				ItemValue = "TBC!",
				SortOrder = generalLookupSource?.SortOrder + 1
			};
			generalLookups.Add(generalLookup);
			await JSRuntime.InvokeVoidAsync("setFocus", "0ItemValue");
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}
	}
}
