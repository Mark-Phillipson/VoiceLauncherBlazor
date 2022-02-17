using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
	public partial class Launchers
	{
		[Parameter] public int? CategoryIdFilter { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		public bool ShowDialog { get; set; }
		private int LauncherIdDelete { get; set; }
		private List<DataAccessLibrary.Models.Launcher>? launchers;
		public string? StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.Category>? Categories { get; set; }
		public List<DataAccessLibrary.Models.Computer>? Computers { get; set; }
		public int MaximumRows { get; set; } = 26;
		public bool ShowCreateNewOrEdit { get; set; }
#pragma warning disable 414
		private int? _launcherId;
		private bool _loadFailed = false;
#pragma warning restore 414
		private string searchTerm="";
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
			await LoadData();
		}
		async Task<int?> CheckForQueryStringAsync()
		{
			var query = new Uri(NavigationManager.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("category", out var categoryName))
			{
				var category = await CategoryService.GetCategoryAsync(categoryName, "Launch Applications");
				return category?.Id;
			}
			return null;
		}
		private async Task LoadData()
		{
			CategoryIdFilter = await CheckForQueryStringAsync();
			if (CategoryIdFilter != null)
			{
				await FilterByCategory();
				try
				{
					Categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					StatusMessage = exception.Message;
					_loadFailed = true;
				}
			}
			else
			{
				try
				{
					_loadFailed = false;
					launchers = await LauncherService.GetLaunchersAsync(maximumRows: MaximumRows);
					Categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					StatusMessage=exception.Message;
					_loadFailed = true;
				}
			}
		}

		async Task ApplyFilter()
		{
			if (SearchTerm != null)
			{
				try
				{
					launchers = await LauncherService.GetLaunchersAsync(SearchTerm.Trim(), maximumRows: MaximumRows);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					StatusMessage = exception.Message;
					_loadFailed = true;
				}
				StateHasChanged();
			}
		}

		void ConfirmDelete(int launcherId)
		{
			ShowDialog = true;
			LauncherIdDelete = launcherId;
		}
		void CancelDelete()
		{
			ShowDialog = false;
		}
		async Task SortLaunchers(string column, string sortType)
		{
			try
			{
				launchers = await LauncherService.GetLaunchersAsync(searchTerm, column, sortType, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task DeleteLauncher(int launcherId)
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				StatusMessage = "This demo application does not allow Deleting!";
				return;
			}
			try
			{
				var result = await LauncherService.DeleteLauncher(launcherId);
				StatusMessage = result;
				ShowDialog = false;
				launchers = await LauncherService.GetLaunchersAsync(searchTerm, maximumRows: MaximumRows);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		async Task SaveAllLaunchers()
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				StatusMessage = "This demo application does not allow saving!";
				return;
			}
			try
			{
				var temporary = await LauncherService.SaveAllLaunchers(launchers);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			StatusMessage = $"Launchers Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		async Task FilterByCategory()
		{
			try
			{
				launchers = await LauncherService.GetLaunchersAsync(null, null, null, CategoryIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		private void EditRecord(int launcherId)
		{
			_launcherId = launcherId;
			ShowCreateNewOrEdit = true;
		}
		private async Task CloseDialog()
		{
			ShowCreateNewOrEdit = false;
			launchers = null;
			await LoadData(); 
		}
		private void CreateRecord()
		{
			ShowCreateNewOrEdit = true;
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}
		private async Task LaunchItemAsync(string commandLine)
		{
			if (commandLine.ToLower().StartsWith("http"))
			{
				await JSRuntime.InvokeAsync<object>("open", commandLine, "_blank");
			}
			else
			{
				await JSRuntime.InvokeVoidAsync(
					"clipboardCopy.copyText", commandLine);
				var message = $"Copied Successfully: '{commandLine}'";
				ToastService!.ShowSuccess(message, "Copy Commandline");
			}
		}
	}
}
