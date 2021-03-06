﻿using Blazored.Toast.Services;
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
		[Parameter] public int? CategoryIdFilter { get; set; } = 0;
		[Parameter] public int? LanguageIdFilter { get; set; } = 0;
		[Inject] public AdditionalCommandService AdditionalCommandService { get; set; }
		[Inject] IToastService ToastService { get; set; }
		public bool ShowDialog { get; set; }
		private int customIntellisenseIdDelete { get; set; }
		private List<DataAccessLibrary.Models.CustomIntelliSense> intellisenses;
		public string StatusMessage { get; set; }
		public List<DataAccessLibrary.Models.Category> categories { get; set; }
		public List<DataAccessLibrary.Models.Language> languages { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup> generalLookups { get; set; }
		public int MaximumRows { get; set; } = 100;
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
			await LoadData();
		}

		private async Task LoadData()
		{
			var query = new Uri(NavigationManager.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("language", out var languageName))
			{
				var language = await LanguageService.GetLanguageAsync(languageName);
				LanguageIdFilter = language?.Id;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("category", out var categoryName))
			{
				var category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
				CategoryIdFilter = category?.Id;
			}
			if (CategoryIdFilter != null && LanguageIdFilter != null)
			{
				await FilterByLanguageAndCategory();
			}
			else if (CategoryIdFilter != null)
			{
				await FilterByCategory();
			}
			else if (LanguageIdFilter != null)
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
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, column, sortType, CategoryIdFilter, LanguageIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		async Task DeleteCustomIntelliSense(int customIntellisenseId)
		{
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
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
			if (Environment.MachineName != "DESKTOP-UROO8T1")
			{
				ToastService.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
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
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, CategoryIdFilter,LanguageIdFilter, maximumRows: MaximumRows);
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
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, CategoryIdFilter, maximumRows: MaximumRows);
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
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, null, LanguageIdFilter, maximumRows: MaximumRows);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}
		private void CreateNew()
		{
			// By default set the language and category defaults to whatever is filtered
			if (LanguageIdFilter!= null  && CategoryIdFilter!= null )
			{
				NavigationManager.NavigateTo($"/intellisense?languageId={LanguageIdFilter}&categoryId={CategoryIdFilter}");
				return;
			}
			if (LanguageIdFilter!= null)
			{
				NavigationManager.NavigateTo($"/intellisense?languageId={LanguageIdFilter}");
				return;
			}
			if (CategoryIdFilter!= null )
			{
				NavigationManager.NavigateTo($"/intellisense?categoryId={CategoryIdFilter}");
				return;
			}

			NavigationManager.NavigateTo($"/intellisense");
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
		public async Task<IEnumerable<DataAccessLibrary.Models.Language>> FilterLanguages(string searchText)
		{
			var languages= await LanguageService.GetLanguagesAsync(searchText);
			return languages;
		}
		public int? GetLanguageId(DataAccessLibrary.Models.Language language)
		{
			return language?.Id;
		}
		public DataAccessLibrary.Models.Language LoadSelectedLanguage(int? languageId)
		{
			if (languageId!= null  && languages!= null )
			{
				var language = languages.FirstOrDefault(l => l.Id == languageId);
				return language;
			}
			return null;
		}
		public async Task<IEnumerable<DataAccessLibrary.Models.Category>> FilterCategories(string searchText)
		{
			var categories = await CategoryService.GetCategoriesAsync(searchText);
			return categories;
		}
		public int? GetCategoryId(DataAccessLibrary.Models.Category category)
		{
			return category?.Id;
		}
		public DataAccessLibrary.Models.Category LoadSelectedCategory(int? categoryId)
		{
			if (categoryId!= null  && categories!= null )
			{
				var category = categories.FirstOrDefault(c => c.Id == categoryId);
				return category;
			}
			return null ;
		}

	}
}
