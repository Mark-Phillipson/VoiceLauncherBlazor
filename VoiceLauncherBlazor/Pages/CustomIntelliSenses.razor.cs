using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;

namespace VoiceLauncherBlazor.Pages
{
	public partial class CustomIntelliSenses
	{
		[Parameter] public int? categoryIdFilter { get; set; }
		[Parameter] public int? languageIdFilter { get; set; }
		[Inject] public AdditionalCommandService AdditionalCommandService { get; set; }
		[Inject] IToastService ToastService { get; set; }
		public bool ShowDialog { get; set; }
		private int customIntellisenseIdDelete { get; set; }
		private List<DataAccessLibrary.Models.CustomIntelliSense> intellisenses;
		public string StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.Category> categories { get; set; }
		public List<DataAccessLibrary.Models.Language> languages { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
		public int MaximumRows { get; set; } = 10;
#pragma warning disable 414
		private bool showCreateNewOrEdit = false;
		private bool _loadFailed = false;
#pragma warning restore 414
		private string searchTerm;
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
			var query = new Uri(NavigationManager.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("language",out var languageName))
			{
				var language = await LanguageService.GetLanguageAsync(languageName);
				languageIdFilter = language?.Id;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("category",out var categoryName))
			{
				var category = await CategoryService.GetCategoryAsync(categoryName,"IntelliSense Command");
				categoryIdFilter=category?.Id;
			}
			if (categoryIdFilter!= null  && languageIdFilter!= null )
			{
				 await FilterByLanguageAndCategory();
			}
			else if (categoryIdFilter != null)
			{
				await FilterByCategory();
			}
			else if (languageIdFilter != null)
			{
				await FilterByLanguage();
			}
			else
			{
				try
				{
					_loadFailed = false;
					intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(maximumRows: MaximumRows);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			try
			{
				categories = await CategoryService.GetCategoriesAsync(categoryTypeFilter: "IntelliSense Command");
				languages = await LanguageService.GetLanguagesAsync();
				generalLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");

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
					intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(SearchTerm.Trim(), maximumRows: MaximumRows);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
				StateHasChanged();
			}
		}

		void ConfirmDelete(int customIntellisenseId)
		{
			ShowDialog = true;
			customIntellisenseIdDelete = customIntellisenseId;
		}
		void CancelDelete()
		{
			ShowDialog = false;
		}
		async Task SortCustomIntelliSenses(string column, string sortType)
		{
			try
			{
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, column, sortType, categoryIdFilter, languageIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task DeleteCustomIntelliSense(int customIntellisenseId)
		{
			var additionalCommands =  await AdditionalCommandService.GetAdditionalCommandsAsync(customIntellisenseId);
			if (additionalCommands.Count>0)
			{
				ShowDialog = false;
				ToastService.ShowWarning("Please remove additional commands before deleting this record!", "Delete Cancelled");
				return;
			}
			try
			{
				var result = await CustomIntellisenseService.DeleteCustomIntelliSense(customIntellisenseId);
				StatusMessage = result;
				ShowDialog = false;
				//intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, maximumRows: MaximumRows);
				var ci = intellisenses.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
				intellisenses.Remove(ci);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		async Task SaveAllCustomIntelliSenses()
		{
			try
			{
				var temporary = await CustomIntellisenseService.SaveAllCustomIntelliSenses(intellisenses);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			StatusMessage = $"Custom IntelliSenses Successfully Saved {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		async Task FilterByLanguageAndCategory()
		{
			try
			{
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, categoryIdFilter,languageIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task FilterByCategory()
		{
			try
			{
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, categoryIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task FilterByLanguage()
		{
			try
			{
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, null, languageIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		private void CreateNew()
		{
			NavigationManager.NavigateTo("/intellisense");
		}
		private void CloseCreateNew()
		{
			showCreateNewOrEdit = false;
		}
		private void EditRecord(int customIntellisenseId)
		{
			NavigationManager.NavigateTo($"/intellisense/{customIntellisenseId}");

		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			await ApplyFilter();
		}

	}
}
