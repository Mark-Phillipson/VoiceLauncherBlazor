using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
	public partial class GeneralLookups
	{
		public bool ShowDialog { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		public bool ShowValidationWarning { get; set; }
		private int? GeneralLookupIdDelete { get; set; }
		public string? StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookupsModel { get; set; }
		public List<string>? GeneralLookupsCategories { get; set; }
		public int AlertDuration { get; set; }
		private string? CategoryFilter { get; set; }
		private string searchTerm="";
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
				GeneralLookupsModel = await GeneralLookupService.GetGeneralLookupsAsync();
				GeneralLookupsCategories = await GeneralLookupService.GetGeneralLookUpsCategoriesAsync();
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
					GeneralLookupsModel = await GeneralLookupService.GetGeneralLookupsAsync(SearchTerm.Trim());
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}

		void ConfirmDelete(int generalLookupId)
		{
			ShowDialog = true;
			GeneralLookupIdDelete = generalLookupId;
		}
		void CancelDialog()
		{
			ShowDialog = false;
		}
		async Task SortGeneralLookups(string column, string sortType)
		{
			try
			{
				GeneralLookupsModel = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm, column, sortType);
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
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
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
				GeneralLookupsModel = await GeneralLookupService.GetGeneralLookupsAsync(searchTerm);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task FilterGeneralLookups()
		{
			if (CategoryFilter != null)
			{
				try
				{
					GeneralLookupsModel = await GeneralLookupService.GetGeneralLookupsAsync(null, null, null, CategoryFilter);
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
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				GeneralLookupsModel = await GeneralLookupService.SaveAllGeneralLookups(GeneralLookupsModel);
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
			DataAccessLibrary.Models.GeneralLookup generalLookup = new()
            {
				Category = generalLookupSource?.Category,
				DisplayValue = generalLookupSource?.DisplayValue,
				ItemValue = "TBC!",
				SortOrder = generalLookupSource?.SortOrder + 1
			};
			GeneralLookupsModel!.Add(generalLookup);
			await JSRuntime.InvokeVoidAsync("setFocus", "0ItemValue");
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}
	}
}
