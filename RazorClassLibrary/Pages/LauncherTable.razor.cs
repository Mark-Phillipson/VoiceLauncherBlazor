using Ardalis.GuardClauses;
using WindowsInput;
using WindowsInput.Native;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.helpers;
using RazorClassLibrary.Shared;

using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

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
		[Parameter] public EventCallback CloseApplication { get; set; }
		[Parameter] public bool RunningInBlazorHybrid { get; set; }
		[Parameter] public string GlobalSearchTerm { get; set; } = "";
		public required AlphabetHelper Alphabet { get; set; } = new AlphabetHelper();
		private int alphabetCounter = 0;
		private string? currentLetter = "";

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
		public string? SearchTerm {
			get => searchTerm;
			set {
				searchTerm = value;
				_ = LoadData(); // Always reload from backend when search changes
			}
		}
		[Parameter] public string? ServerSearchTerm { get; set; }
		public string ExceptionMessage { get; set; } = string.Empty;
		public List<string>? PropertyInfo { get; set; }
		[CascadingParameter] public ClaimsPrincipal? User { get; set; }
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		private List<LauncherDTO>? _cachedLauncherDTO;

		protected override async Task OnInitializedAsync()
		{
			Alphabet.BuildAlphabet();
			await LoadData();
		}
		[Parameter] public bool RefreshData { get; set; }

		protected override async Task OnParametersSetAsync()
		{
			// If CategoryId is supplied (not zero), always force reload from DB (bypass cache)
			if (CategoryId != 0)
			{
				await LoadData(true);
			}
			else
			{
				await LoadData(RefreshData);
			}
		}
		private async Task LoadData(bool forceRefresh = false)
		{
			// Always load categories fresh
			if (CategoryDataService != null)
			{
				try
				{
					_categories = await CategoryDataService.GetAllCategoriesAsync("Launch Applications", 0);
				}
				catch (Exception ex)
				{
					Logger?.LogError(ex, "Failed to load categories.");
				}
			}

			// If no filters, show no results
			bool hasFilter = CategoryId != 0 || !string.IsNullOrWhiteSpace(GlobalSearchTerm) || !string.IsNullOrWhiteSpace(SearchTerm);

			if (!hasFilter)
			{
				LauncherDTO = new List<LauncherDTO>();
				FilteredLauncherDTO = new List<LauncherDTO>();
				Title = "No launchers to display (no filter applied)";
				StateHasChanged();
				return;
			}

			CategoryDTO? category = null;
			try
			{
				List<LauncherDTO> result = new List<LauncherDTO>();
				if (LauncherDataService != null)
				{
					// If a local SearchTerm is present and a category is selected, load that
					// category's launchers and apply the filter locally so searches stay
					// scoped to the current category. If no category is selected, fall
					// back to the server-side search which searches across all categories.
					if (!string.IsNullOrWhiteSpace(SearchTerm))
					{
						if (CategoryId != 0)
						{
							result = await LauncherDataService.GetAllLaunchersAsync(CategoryId);
						}
						else
						{
							result = await LauncherDataService.SearchLaunchersAsync(SearchTerm);
						}
					}
					else if (!string.IsNullOrWhiteSpace(GlobalSearchTerm))
					{
						// GlobalSearchTerm is intended to search across everything
						result = await LauncherDataService.SearchLaunchersAsync(GlobalSearchTerm);
					}
					else
					{
						result = await LauncherDataService.GetAllLaunchersAsync(CategoryId);
					}
					LauncherDTO = result.ToList();
				}
			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
				_loadFailed = true;
				ExceptionMessage = e.Message;
				LauncherDTO = new List<LauncherDTO>();
			}

				LauncherDTO ??= new List<LauncherDTO>();
				FilteredLauncherDTO = new List<LauncherDTO>(LauncherDTO);
				// If we loaded category results for a local SearchTerm, apply the
				// local filter here so the search is scoped to the selected category.
				if (!string.IsNullOrWhiteSpace(SearchTerm) && CategoryId != 0)
				{
					ApplyFilter();
					FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(l => l.Name).ToList();
				}
				else
				{
					// Backend already returned the correctly filtered list (global search or full category list)
					FilteredLauncherDTO = FilteredLauncherDTO.OrderBy(l => l.Name).ToList();
				}

			if (CategoryDataService != null && CategoryId != 0)
			{
				category = await CategoryDataService.GetCategoryById(CategoryId);
			}

			if (!string.IsNullOrWhiteSpace(GlobalSearchTerm))
			{
				Title = $"Launchers matching '{GlobalSearchTerm}' ({FilteredLauncherDTO.Count})";
				if (!string.IsNullOrWhiteSpace(SearchTerm) && FilteredLauncherDTO.Count != LauncherDTO.Count)
				{
					Title += $" and '{SearchTerm}'";
				}
			}
			else if (category != null)
			{
				Title = $"Launcher Category: {category.CategoryName} ({FilteredLauncherDTO.Count})";
				if (!string.IsNullOrWhiteSpace(SearchTerm)) Title += $" (filtered by '{SearchTerm}')";
			}
			else
			{
				Title = $"Filtered Launchers ({FilteredLauncherDTO.Count})";
			}
			StateHasChanged();
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
		void AddNewLauncherAsync()
		{
			if (NavigationManager != null)
			{
				NavigationManager.NavigateTo("/launcheradd");
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
				if (FilteredLauncherDTO.Count == 1 && RunningInBlazorHybrid)
				{
					// Fire and Forget
					_ = ProcessLaunching(FilteredLauncherDTO.First().Id);
				}
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
				if (LauncherMultipleLauncherBridgeDataService != null)
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
						if (searchTerm != null)
						{
							ApplyFilter();
						}
					}
				}
			}
		}
		void EditLauncherAsync(int Id)
		{
			if (NavigationManager != null)
			{
				NavigationManager.NavigateTo($"/launcheredit/{Id}");
			}
		}
		private async Task ProcessLaunching(int id)
		{
			var launcher = LauncherDTO?.FirstOrDefault(l => l.Id == id);
			if (launcher == null) { return; }
			var rawCmd = launcher.CommandLine ?? string.Empty;
			// Trim and strip surrounding quotes if present
			var cmd = rawCmd.Trim();
			if ((cmd.StartsWith("\"") && cmd.EndsWith("\"")) || (cmd.StartsWith("'") && cmd.EndsWith("'")))
			{
				cmd = cmd.Substring(1, cmd.Length - 2).Trim();
			}
			// Remove file:// or file:/// prefix if present
			if (cmd.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
			{
				// For file:///C:/path style, remove the scheme
				var idx = cmd.IndexOf(':', 5);
				if (idx >= 0)
				{
					cmd = cmd.Substring(5);
				}
				else
				{
					cmd = cmd.Substring(5);
				}
			}
			var lower = cmd.ToLowerInvariant();
			// If it's an HTTP link, navigate to it
			if (lower.StartsWith("http") && NavigationManager != null)
			{
				NavigationManager.NavigateTo(launcher.CommandLine ?? string.Empty, true, false);
			}
			// If it's a markdown file, navigate to the MarkdownViewer page with the file path as a query parameter
			else if ((lower.EndsWith(".md") || lower.EndsWith(".markdown")) && NavigationManager != null)
			{
				// URL encode the file path and navigate to the viewer
				var encoded = Uri.EscapeDataString(cmd);
				NavigationManager.NavigateTo($"/markdownviewer?file={encoded}");
			}
			else
			{
				var psi = new ProcessStartInfo();
				psi.FileName = launcher.CommandLine;
				psi.WorkingDirectory = launcher.WorkingDirectory;
				psi.Arguments = launcher.Arguments;
				psi.WindowStyle = ProcessWindowStyle.Maximized;
				psi.UseShellExecute = true;
				if (psi.FileName.EndsWith(".exe"))
				{
					psi.UseShellExecute = false;
				}
				try
				{
					Process.Start(psi);
				}
				catch (Exception exception)
				{
					Message = exception.Message;
					return;
				}
				await CloseApplication.InvokeAsync();
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
			await LoadData(true); // Ensure data is reloaded and filtered by the new CategoryId
		}
		private async Task SaveDataToJsonFile(List<LauncherDTO> data)
		{
			// Cache logic removed
		}
		private async Task<List<LauncherDTO>?> LoadDataFromJsonFile()
		{
			// Cache logic removed
			return null;
		}
        protected async Task ResetFilter()
        {
            // Clear the local search term and message
            searchTerm = null;
            Message = string.Empty;

            // Reload data from service and cache using forceRefresh
            await LoadData(true);

            // Apply default filter (no search term)
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