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
using System.Security.Claims;

namespace RazorClassLibrary.Pages
{
	public partial class TalonAlphabetTable : ComponentBase
	{
		[Inject] public ITalonAlphabetDataService? TalonAlphabetDataService { get; set; }
		[Inject] public NavigationManager? NavigationManager { get; set; }
		[Inject] public ILogger<TalonAlphabetTable>? Logger { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		public string Title { get; set; } = "TalonAlphabet Items (TalonAlphabets)";
		public string EditTitle { get; set; } = "Edit TalonAlphabet Item (TalonAlphabets)";
		[Parameter] public int ParentId { get; set; }
		[Parameter] public bool ReportView { get; set; } = false;
		public List<TalonAlphabetDTO>? TalonAlphabetDTO { get; set; }
		public List<TalonAlphabetDTO>? FilteredTalonAlphabetDTO { get; set; }
		protected TalonAlphabetAddEdit? TalonAlphabetAddEdit { get; set; }
		ElementReference SearchInput;
#pragma warning disable 414, 649
		private bool _loadFailed = false;
		private string? searchTerm = null;
#pragma warning restore 414, 649
		public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
		[Parameter] public string? ServerSearchTerm { get; set; }
		public string ExceptionMessage { get; set; } = string.Empty;
		public List<string>? PropertyInfo { get; set; }
		[CascadingParameter] public ClaimsPrincipal? User { get; set; }
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		public bool ShowEdit { get; set; } = false;
		private bool ShowDeleteConfirm { get; set; }
		private int TalonAlphabetId { get; set; }
		protected override async Task OnInitializedAsync()
		{
			await LoadData();
		}

		private async Task LoadData()
		{
			try
			{
				if (TalonAlphabetDataService != null)
				{
					var result = await TalonAlphabetDataService!.GetAllTalonAlphabetsAsync();
					//var result = await TalonAlphabetDataService.SearchTalonAlphabetsAsync(ServerSearchTerm);
					if (result != null)
					{
						TalonAlphabetDTO = result.OrderBy(x => x.RandomSort).ToList();
						FilteredTalonAlphabetDTO = TalonAlphabetDTO;
						StateHasChanged();
					}
				}
			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
				_loadFailed = true;
				ExceptionMessage = e.Message;
			}
			FilteredTalonAlphabetDTO = TalonAlphabetDTO;
			Title = $"Talon Alphabet ({FilteredTalonAlphabetDTO?.Count})";

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
		private async Task AddNewTalonAlphabet()
		{
			var parameters = new ModalParameters();
			var formModal = Modal?.Show<TalonAlphabetAddEdit>("Add Talon Alphabet", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
				}
			}
			TalonAlphabetId = 0;
		}


		private void ApplyFilter()
		{
			if (FilteredTalonAlphabetDTO == null || TalonAlphabetDTO == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredTalonAlphabetDTO = TalonAlphabetDTO.OrderBy(v => v.Letter).ToList();
				Title = $"All Talon Alphabet ({FilteredTalonAlphabetDTO.Count})";
			}
			else
			{
				var temporary = SearchTerm.ToLower().Trim();
				FilteredTalonAlphabetDTO = TalonAlphabetDTO
					 .Where(v =>
					 v.Letter != null && v.Letter.ToLower().Contains(temporary)
					 )
					 .ToList();
				Title = $"Filtered Talon Alphabets ({FilteredTalonAlphabetDTO.Count})";
			}
		}
		protected void SortTalonAlphabet(string sortColumn)
		{
			Guard.Against.Null(sortColumn, nameof(sortColumn));
			if (FilteredTalonAlphabetDTO == null)
			{
				return;
			}
			if (sortColumn == "Letter")
			{
				FilteredTalonAlphabetDTO = FilteredTalonAlphabetDTO.OrderBy(v => v.Letter).ToList();
			}
			else if (sortColumn == "Letter Desc")
			{
				FilteredTalonAlphabetDTO = FilteredTalonAlphabetDTO.OrderByDescending(v => v.Letter).ToList();
			}
		}
		private async Task DeleteTalonAlphabet(int Id)
		{
			//TODO Optionally remove child records here or warn about their existence
			var parameters = new ModalParameters();
			if (TalonAlphabetDataService != null)
			{
				var talonAlphabet = await TalonAlphabetDataService.GetTalonAlphabetById(Id);
				parameters.Add("Title", "Please Confirm, Delete Talon Alphabet");
				parameters.Add("Message", $"Letter: {talonAlphabet?.Letter}");
				parameters.Add("ButtonColour", "danger");
				parameters.Add("Icon", "fa fa-trash");
				var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Talon Alphabet ({talonAlphabet?.Letter})?", parameters);
				if (formModal != null)
				{
					var result = await formModal.Result;
					if (!result.Cancelled)
					{
						await TalonAlphabetDataService.DeleteTalonAlphabet(Id);
						ToastService?.ShowSuccess("Talon Alphabet deleted successfully");
						await LoadData();
					}
				}
			}
			TalonAlphabetId = Id;
		}

		private async void EditTalonAlphabet(int Id)
		{
			var parameters = new ModalParameters();
			parameters.Add("Id", Id);
			var formModal = Modal?.Show<TalonAlphabetAddEdit>("Edit Talon Alphabet", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
				}
			}
			TalonAlphabetId = Id;
		}

	}
}