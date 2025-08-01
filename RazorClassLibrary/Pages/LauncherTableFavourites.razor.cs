using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Diagnostics;
using System.Security.Claims;
using DataAccessLibrary.Repositories;

namespace RazorClassLibrary.Pages
{
	public partial class LauncherTableFavourites : ComponentBase
	{
		[Inject] public required ILauncherDataService LauncherDataService { get; set; }
		[Inject] public required ICategoryDataService CategoryDataService { get; set; }
		[Inject] public required NavigationManager NavigationManager { get; set; }
		[Inject] public required ILogger<LauncherTableFavourites> Logger { get; set; }
		[Inject] public required IToastService ToastService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		public string Title { get; set; } = "Favourite Launcher Items";
		private int? counter = 0;
		[Parameter] public int CategoryId { get; set; }
		public List<LauncherDTO>? LauncherDTO { get; set; }
		public List<LauncherDTO>? FilteredLauncherDTO { get; set; }
		protected LauncherAddEdit? LauncherAddEdit { get; set; }
		public string Message { get; set; } = "";
		ElementReference SearchInput;
#pragma warning disable 414, 649
		private bool _loadFailed = false;
		private string? searchTerm = null;
		private string? _randomColor1;
#pragma warning restore 414, 649
		public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
		[Parameter] public string? ServerSearchTerm { get; set; }
		public string ExceptionMessage { get; set; } = string.Empty;
		public List<string>? PropertyInfo { get; set; }
		[CascadingParameter] public ClaimsPrincipal? User { get; set; }
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		protected override async Task OnInitializedAsync()
		{
			await LoadData();
		}

		private async Task LoadData()
		{
			CategoryDTO? category = null;
			try
			{
				if (LauncherDataService != null)
				{
					var result = await LauncherDataService!.GetFavoriteLaunchersAsync();
					//var result = await LauncherDataService.SearchLaunchersAsync(ServerSearchTerm);
					if (result != null)
					{
						LauncherDTO = result.ToList();
					}
				}

				if (CategoryDataService != null)
				{
					category = await CategoryDataService.GetCategoryById(CategoryId);

				}
			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
				_loadFailed = true;
				ExceptionMessage = e.Message;
			}
			FilteredLauncherDTO = LauncherDTO;
			Title = $"Favourites: {category?.CategoryName} ({FilteredLauncherDTO?.Count})";

		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				try
				{
					if (JSRuntime != null)
					{
						await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
					}
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
				}
			}
		}
		protected async Task AddNewLauncherAsync()
		{
			var parameters = new ModalParameters();

			parameters.Add(nameof(CategoryId), CategoryId);
			var formModal = Modal?.Show<LauncherAddEdit>("Add Launcher", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
				}
			}
		}

		private void ApplyFilter()
		{
			if (FilteredLauncherDTO == null || LauncherDTO == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredLauncherDTO = LauncherDTO.OrderBy(v => v.SortOrder).ThenBy(x => x.Name).ToList();
				Title = $"All Favourites ({FilteredLauncherDTO.Count})";
			}
			else
			{
				var temporary = SearchTerm.ToLower().Trim();
				FilteredLauncherDTO = LauncherDTO
					 .Where(v =>
					 v.Name != null && v.Name.ToLower().Contains(temporary)
					  || v.CommandLine != null && v.CommandLine.ToLower().Contains(temporary)
					 )
					 .ToList();
				Title = $"Filtered Favourites ({FilteredLauncherDTO.Count})";
			}
		}
		protected void SortLauncher(string sortColumn)
		{
			Guard.Against.Null(sortColumn, nameof(sortColumn));
			if (FilteredLauncherDTO == null)
			{
				return;
			}
			if (sortColumn == "Name")
			{
				FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(v => v.Name).ToList();
			}
			else if (sortColumn == "Name Desc")
			{
				FilteredLauncherDTO = FilteredLauncherDTO.OrderByDescending(v => v.Name).ToList();
			}
			if (sortColumn == "CommandLine")
			{
				FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(v => v.CommandLine).ToList();
			}
			else if (sortColumn == "CommandLine Desc")
			{
				FilteredLauncherDTO = FilteredLauncherDTO.OrderByDescending(v => v.CommandLine).ToList();
			}
		}
		async Task DeleteLauncherAsync(int Id)
		{
			//Optionally remove child records here or warn about their existence
			//var ? = await ?DataService.GetAllLauncher(Id);
			//if (? != null)
			//{
			//	ToastService.ShowWarning($"It is not possible to delete a launcher that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
			//	return;
			//}
			var parameters = new ModalParameters();
			if (LauncherDataService != null)
			{
				var launcher = await LauncherDataService.GetLauncherById(Id);
				parameters.Add("Title", "Please Confirm, Delete Launcher");
				parameters.Add("Message", $"Name: {launcher?.Name}");
				parameters.Add("ButtonColour", "danger");
				parameters.Add("Icon", "fa fa-trash");
				var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Launcher ({launcher?.Name})?", parameters);
				if (formModal != null)
				{
					var result = await formModal.Result;
					if (!result.Cancelled)
					{
						await LauncherDataService.DeleteLauncher(Id);
						ToastService?.ShowSuccess(" Launcher deleted successfully");
						await LoadData();
					}
				}
			}
		}
		async Task EditLauncherAsync(int Id)
		{
			var parameters = new ModalParameters();
			parameters.Add("Id", Id);
			var formModal = Modal?.Show<LauncherAddEdit>("Edit Launcher", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
				}
			}
		}
		private void LaunchItem(LauncherDTO launcher)
		{
			if (JSRuntime == null)
			{
				return;
			}
			if (launcher.CommandLine.Trim().ToLower().StartsWith("http") && NavigationManager != null)
			{
				NavigationManager.NavigateTo(launcher.CommandLine, true, false);
			}
			else
			{
				var psi = new ProcessStartInfo();
				psi.UseShellExecute = true;
				psi.FileName = launcher.CommandLine;
				psi.WorkingDirectory = launcher.WorkingDirectory;
				psi.Arguments = launcher.Arguments;
				psi.UseShellExecute = true;
				try
				{
					Process.Start(psi);
				}
				catch (Exception exception)
				{
					Message = exception.Message;
				}
			}
		}
		public string RandomColour { get { _randomColor1 = GetColour(); return _randomColor1; } set => _randomColor1 = value; }
		public string GetColour()
		{
			var random = new Random();
			return string.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

		}
		private async Task UpdateFavorite(LauncherDTO launcher)
		{
			await LauncherDataService.UpdateLauncher(launcher, "TBC");
			await LoadData();
		}
        protected async Task ResetFilter()
        {
            // Clear the search term and message
            searchTerm = null;
            Message = string.Empty;
            
            // Reload data from the service to reset cache
            await LoadData();

            // Reapply default filter (no search term)
            ApplyFilter();

            // Set focus back to search input
            try
            {
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
                }
            }
            catch { }
        }
	}
}