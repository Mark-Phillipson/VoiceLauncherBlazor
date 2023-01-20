using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;


namespace VoiceLauncher.Pages
{
	public partial class CategoryEdit
	{
		[Parameter] public int CategoryId { get; set; }
		[Inject] IToastService? ToastService { get; set; }
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
					CategoryType = "IntelliSense Command"
				};
			}

            if (Category!= null )
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
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
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
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
		}

	}
}
