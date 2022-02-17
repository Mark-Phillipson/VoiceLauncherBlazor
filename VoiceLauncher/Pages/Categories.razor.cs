using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
	public partial class Categories
	{
		[Inject] IToastService? ToastService { get; set; }
		public bool ShowDialog { get; set; }
		private int? CategoryIdDelete { get; set; }
		private List<DataAccessLibrary.Models.Category>? categories;
		public string? StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
		private string? CategoryTypeFilter { get; set; }
		private string? searchTerm;
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414


		public string? SearchTerm
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
				categories = await CategoryService.GetCategoriesAsync();
				GeneralLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Category Types");
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
					categories = await CategoryService.GetCategoriesAsync(SearchTerm.Trim());
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}

		void ConfirmDelete(int categoryId)
		{
			ShowDialog = true;
			CategoryIdDelete = categoryId;
		}
		void CancelDelete()
		{
			ShowDialog = false;
		}
		async Task SortCategories(string column, string sortType)
		{
			try
			{
				categories = await CategoryService.GetCategoriesAsync(searchTerm, column, sortType);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task DeleteCategory(int categoryId)
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await CategoryService.DeleteCategory(categoryId);
				StatusMessage = result;
				ShowDialog = false;
				categories = await CategoryService.GetCategoriesAsync(SearchTerm);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task FilterCategoryType()
		{
			if (CategoryTypeFilter != null)
			{
				try
				{
					categories = await CategoryService.GetCategoriesAsync(null, null, null, CategoryTypeFilter);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}
		async Task SaveAllCategories()
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				categories = await CategoryService.SaveAllCategories(categories);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			StatusMessage = $"Categories Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}

	}
}
