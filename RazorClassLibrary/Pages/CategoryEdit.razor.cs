using Blazored.Toast.Services;

using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;


namespace RazorClassLibrary.Pages
{
	public partial class CategoryEdit : ComponentBase
	{
		[Parameter] public int CategoryId { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		[Inject] public required IJSRuntime JSRuntime { get; set; }
		[Inject] public required CategoryService CategoryService { get; set; }
		[Inject] public required GeneralLookupService GeneralLookupService { get; set; }
		[Inject] public required NavigationManager NavigationManager { get; set; }
		public DataAccessLibrary.Models.Category? Category { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
#pragma warning disable 414
		private EditContext? _editContext;
		private bool _loadFailed = false;
#pragma warning restore 414
		public string? StatusMessage { get; set; }
		protected override async Task OnInitializedAsync()
		{
			if (CategoryId > 0)
			{
				try
				{
					Category = await CategoryService.GetCategoryAsync(CategoryId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				Category = new DataAccessLibrary.Models.Category
				{
					CategoryType = "IntelliSense Command",
					CategoryName = "A New Category",
					Sensitive = false,
					Colour = "#000000"
				};
			}

			if (Category != null)
			{
				_editContext = new EditContext(Category);
			}
			try
			{
				GeneralLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Category Types");
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		protected override async Task OnParametersSetAsync()
		{
			if (CategoryId > 0)
			{
				try
				{
					Category = await CategoryService.GetCategoryAsync(CategoryId);
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
			if (Environment.MachineName != "J40L4V3")
			{
				ToastService!.ShowError("This demo application does not allow editing of data! Demo Only");
				return;
			}
			if (Category == null)
			{
				return;
			}
			try
			{
				var result = await CategoryService.SaveCategory(Category);
				if (result.ToLower().Contains("success"))
				{
					NavigationManager.NavigateTo("categories");
				}
				StatusMessage = result;
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

	}
}
