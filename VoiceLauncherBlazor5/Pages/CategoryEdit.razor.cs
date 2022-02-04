using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace VoiceLauncherBlazor.Pages
{
	public partial class CategoryEdit
	{
		[Parameter] public int categoryId { get; set; }
		[Inject] IToastService ToastService { get; set; }
		private EditContext _editContext;
		public DataAccessLibrary.Models.Category category { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414
		public string StatusMessage { get; set; }
		protected override async Task OnInitializedAsync()
		{
			if (categoryId > 0)
			{
				try
				{
					category = await CategoryService.GetCategoryAsync(categoryId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				category = new DataAccessLibrary.Models.Category
				{
					CategoryType = "IntelliSense Command"
				};
			}
			_editContext = new EditContext(category);
			try
			{
				generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Category Types");
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		protected override async Task OnParametersSetAsync()
		{
			if (categoryId > 0)
			{
				try
				{
					category = await CategoryService.GetCategoryAsync(categoryId);
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
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await CategoryService.SaveCategory(category);
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
