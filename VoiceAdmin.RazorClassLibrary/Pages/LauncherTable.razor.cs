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

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
	public partial class LauncherTable : ComponentBase
	{
		[Inject] public ILauncherDataService? LauncherDataService { get; set; }
		[Inject] public ICategoryDataService? CategoryDataService { get; set; }
		[Inject] public LauncherMultipleLauncherBridgeDataService? LauncherMultipleLauncherBridgeDataService { get; set; }
		[Inject] public NavigationManager? NavigationManager { get; set; }
		[Inject] public ILogger<LauncherTable>? Logger { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		public string Title { get; set; } = "Launcher Items (Launchers)";
		private List<CategoryDTO> _categories = new List<CategoryDTO>();
		[Parameter] public int CategoryId { get; set; } = 0;
		public List<LauncherDTO>? LauncherDTO { get; set; }
		public List<LauncherDTO>? FilteredLauncherDTO { get; set; }
		protected LauncherAddEdit? LauncherAddEdit { get; set; }
		ElementReference SearchInput;
#pragma warning disable 414, 649
		private bool _loadFailed = false;
		private string? searchTerm = null;
		private string? _randomColor1;
		string Message = "";
#pragma warning restore 414, 649
		private  int ? counter = 0;
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
					var result = await LauncherDataService!.GetAllLaunchersAsync(CategoryId);
					//var result = await LauncherDataService.SearchLaunchersAsync(ServerSearchTerm);
					if (result != null)
					{
						LauncherDTO = result.ToList();
					}
				}

				if (CategoryDataService != null)
				{
					category = await CategoryDataService.GetCategoryById(CategoryId);
					_categories = await CategoryDataService.GetAllCategoriesAsync("Launch Applications", 0);
				}
			}
			catch (Exception e)
			{
				Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
				_loadFailed = true;
				ExceptionMessage = e.Message;
			}
			FilteredLauncherDTO = LauncherDTO;
			Title = $"Launcher Category: {category?.CategoryName} ({FilteredLauncherDTO?.Count})";

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
				FilteredLauncherDTO = LauncherDTO.OrderBy(v => v.Name).ToList();
				Title = $"All Launcher ({FilteredLauncherDTO.Count})";
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
				Title = $"Filtered Launchers ({FilteredLauncherDTO.Count})";
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
				Message = "";
				var launcher = await LauncherDataService.GetLauncherById(Id);
				parameters.Add("Title", "Please Confirm, Delete Launcher");
				parameters.Add("Message", $"Name: {launcher?.Name}");
				parameters.Add("ButtonColour", "danger");
				parameters.Add("Icon", "fa fa-trash");
				if (LauncherMultipleLauncherBridgeDataService != null )
				{
					var multiple = await LauncherMultipleLauncherBridgeDataService.GetLMLBsAsync(launcher?.Id, null); 
					if (multiple != null && multiple.Count > 0)
					{
						parameters.Add("Message", $"Name: {launcher?.Name} is linked to {multiple.Count} Multiple Launchers. Please delete the links first.");
						parameters.Add("ButtonColour", "warning");
						parameters.Add("Icon", "fa fa-exclamation-triangle");
					}
				}
				var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Launcher ({launcher?.Name})?", parameters);
				if (formModal != null)
				{
					var result = await formModal.Result;
					if (!result.Cancelled)
					{
						try
						{
							await LauncherDataService.DeleteLauncher(Id);
						}
						catch (Exception exception)
						{
							Message = exception.Message;
							ToastService?.ShowError($"Error deleting Launcher: {Message}");
							await LoadData();
							return;
						}
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
		private void MAUILaunching(int id)
		{
			var launcher = LauncherDTO?.FirstOrDefault(l => l.Id == id);
			if (launcher == null) { return; }
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

		private async Task LaunchItemAsync(string commandLine)
		{

			if (JSRuntime == null)
			{
				return;
			}
			if (commandLine.Trim().ToLower().StartsWith("http") && NavigationManager != null)
			{
				NavigationManager.NavigateTo(commandLine, true, false);
			}
			else
			{
				await JSRuntime.InvokeVoidAsync(
					 "clipboardCopy.copyText", commandLine);
				var message = $"Copied Successfully: '{commandLine}'";
				ToastService!.ShowSuccess(message+"Copy Commandline");
			}
		}
		public string RandomColour { get { _randomColor1 = GetColour(); return _randomColor1; } set => _randomColor1 = value; }
		public string GetColour()
		{
			var random = new Random();
			return string.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

		}
		private async Task OnCategorySelectedAsync(int value)
		{
			CategoryId = value;
			await LoadData();
		}

	}
}