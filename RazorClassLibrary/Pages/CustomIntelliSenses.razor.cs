﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using RazorClassLibrary.helpers;
using VoiceLauncher.Services;
using WindowsInput;
using WindowsInput.Native;

namespace RazorClassLibrary.Pages
{
	public partial class CustomIntelliSenses : ComponentBase
	{
		[Parameter] public int? CategoryIdFilter { get; set; } = 0;
		[Parameter] public int? LanguageIdFilter { get; set; } = 0;
		[Parameter] public string SearchTermParameter { get; set; } = string.Empty;
		[Parameter] public bool RunningInBlazorHybrid { get; set; } = false;
		[Parameter] public EventCallback CloseApplication { get; set; }
		[Parameter] public EventCallback MaximizeApplication { get; set; }
		private string Title { get; set; } = "Snippets";
		private bool useSemanticMatching = false;
		private string? _languageFilter = "";
		private string? _categoryFilter = "";
		[Inject] public AdditionalCommandService? AdditionalCommandService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		[Inject] public required IJSRuntime JSRuntime { get; set; }
		private int currentAcceleratorKey = 0;
		[Inject]
		public required CustomIntellisenseService CustomIntellisenseService { get; set; }
		[Inject] public required ICustomIntelliSenseDataService CustomIntelliSenseDataService { get; set; }
		[Inject] public required CategoryService CategoryService { get; set; }
		[Inject] public required LanguageService LanguageService { get; set; }
		[Inject] public required GeneralLookupService GeneralLookupService { get; set; }
		[Inject] public required NavigationManager NavigationManager { get; set; }
		public required AlphabetHelper Alphabet { get; set; } = new AlphabetHelper();
		private int alphabetCounter = 0;
		private string? currentLetter = "";
		public bool ShowDialog { get; set; }
		private int? CustomIntellisenseIdDelete { get; set; }
		private List<CustomIntelliSense>? intellisenses;
		public string? StatusMessage { get; set; }
		public List<Category>? Categories { get; set; }
		public List<Language>? Languages { get; set; }
		public List<DataAccessLibrary.Models.GeneralLookup>? GeneralLookups { get; set; }
		private bool _showTiles = true;
		private bool _showOnlyLanguageAndCategory = false;
		public int MaximumRows { get; set; } = 2000;
#pragma warning disable 414
		readonly bool showCreateNewOrEdit = false;
		private bool _loadFailed = false;
#pragma warning restore 414
		private string searchTerm = "";
		int CustomIntelliSenseId;
		Category? category;
		Language? language;
		private CustomIntelliSense? customIntelliSenseCurrent;
		private string temporaryAccessKey = string.Empty;
		private int counter = 0;
		private bool showExtras = false;
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
				Categories = await CategoryService.GetCategoriesAsync(categoryTypeFilter: "IntelliSense Command");
				Languages = await LanguageService.GetLanguagesAsync();
				GeneralLookups = await GeneralLookupService.GetGeneralLookUpsAsync("Delivery Type");
				if (Alphabet != null)
				{
					Alphabet.BuildAlphabet();
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			if (language != null && category != null)
			{
				_languageFilter = language.LanguageName;
				_categoryFilter = category.CategoryName;
			}
			if (!string.IsNullOrWhiteSpace(SearchTermParameter))
			{
				SearchTerm = SearchTermParameter;
			}
			await LoadData();
		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender && JSRuntime != null)
			{
				try
				{
					await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
				}
			}
		}
		private async Task RemoveFilter()
		{
			// Clear all filters
			CategoryIdFilter = null;
			LanguageIdFilter = null;
			SearchTerm = string.Empty;
			category = null;
			language = null;
			_languageFilter = null;
			_categoryFilter = null;

			// Reload data
			await LoadData();

			// Refocus the search input
			try
			{
				await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
			}
			catch { }
		}
		private async Task LoadData()
		{
			var query = new Uri(NavigationManager.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("language", out var languageName) && !string.IsNullOrWhiteSpace(languageName.ToString()))
			{
				var language1 = languageName.ToString();
				language = await LanguageService.GetLanguageAsync(language1);
				LanguageIdFilter = language?.Id;
				_languageFilter = language?.LanguageName;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("category", out var categoryName))
			{
				string category1 = categoryName.ToString();
				category = await CategoryService.GetCategoryAsync(category1, "IntelliSense Command");
				CategoryIdFilter = category?.Id;
				_categoryFilter = category?.CategoryName;
			}
			if (CategoryIdFilter != null && LanguageIdFilter != null)
			{
				await FilterByLanguageAndCategory();
				category = await CategoryService.GetCategoryAsync(CategoryIdFilter.Value);
				_categoryFilter = category?.CategoryName;
				language = await LanguageService.GetLanguageAsync(LanguageIdFilter.Value);
				_languageFilter = language?.LanguageName;
			}
			else if (CategoryIdFilter != null)
			{
				await FilterByCategory();
				category = await CategoryService.GetCategoryAsync(CategoryIdFilter.Value);
				_categoryFilter = category?.CategoryName;
			}
			else if (LanguageIdFilter != null)
			{
				await FilterByLanguage();
				language = await LanguageService.GetLanguageAsync(LanguageIdFilter.Value);
				_languageFilter = language?.LanguageName;
			}
			else
			{
				try
				{
					_loadFailed = false;
					//intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(maximumRows: MaximumRows);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			await ApplyFilter();
		}
		// 
		async Task ApplyFilter()
		{
			if (!string.IsNullOrWhiteSpace(SearchTerm) || !string.IsNullOrWhiteSpace(_languageFilter) || !string.IsNullOrWhiteSpace(_categoryFilter))
			{
				try
				{
					intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(SearchTerm?.Trim(), categoryIdFilter: CategoryIdFilter, languageIdFilter: LanguageIdFilter, maximumRows: MaximumRows, languageFilter: _languageFilter, categoryFilter: _categoryFilter, useSemanticMatching: useSemanticMatching);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
				await PopulateFilters();
				Title = $"Snippets ({intellisenses?.Count})";
				StateHasChanged();
			}
		}
		async Task PopulateFilters()
		{
			if (_languageFilter != null)
			{
				language = await LanguageService.GetLanguageAsync(_languageFilter);
				LanguageIdFilter = language?.Id;
			}
			if (_categoryFilter != null)
			{
				category = await CategoryService.GetCategoryAsync(_categoryFilter, "IntelliSense Command");
				CategoryIdFilter = category?.Id;
			}
		}
		private async Task ApplySpecialFilter(int languageId, int categoryId)
		{
			CategoryIdFilter = categoryId;
			category = await CategoryService.GetCategoryAsync(categoryId);
			_categoryFilter = category!.CategoryName;
			LanguageIdFilter = languageId;
			language = await LanguageService.GetLanguageAsync(languageId);
			_languageFilter = language!.LanguageName;
			await ApplyFilter();
		}
		void ConfirmDelete(int customIntellisenseId)
		{
			ShowDialog = true;
			CustomIntellisenseIdDelete = customIntellisenseId;
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
			if (Environment.MachineName != "J40L4V3")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!");
				return;
			}
			var additionalCommands = await AdditionalCommandService!.GetAdditionalCommandsAsync(customIntellisenseId);
			if (additionalCommands.Count > 0)
			{
				ShowDialog = false;
				ToastService!.ShowWarning("Please remove additional commands before deleting this record!");
				return;
			}
			try
			{
				var result = await CustomIntellisenseService.DeleteCustomIntelliSense(customIntellisenseId);
				StatusMessage = result;
				ShowDialog = false;
				//intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(searchTerm, maximumRows: MaximumRows);
				var ci = intellisenses!.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
				if (ci != null)
					intellisenses!.Remove(ci);
				await LoadData();

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		async Task SaveAllCustomIntelliSenses()
		{
			if (Environment.MachineName != "J40L4V3")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!");
				return;
			}
			try
			{
				if (intellisenses != null)
				{
					var temporary = await CustomIntellisenseService.SaveAllCustomIntelliSenses(intellisenses);
				}
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
				intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(null, null, null, CategoryIdFilter, LanguageIdFilter, maximumRows: MaximumRows);
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
		private async Task CreateNew()
		{
			var parameters = new ModalParameters();
			if (LanguageIdFilter != null)
			{
				parameters.Add("LanguageId", LanguageIdFilter);
			}
			if (CategoryIdFilter != null)
			{
				parameters.Add("CategoryId", CategoryIdFilter);
			}
			parameters.Add("Title", "Add Snippet");
			var options = new ModalOptions()
			{
				Class = "blazored-modal-custom",
				Size = ModalSize.ExtraLarge
			};
			var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Add Snippet", parameters, options);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
					if (searchTerm != null)
					{
						await ApplyFilter();
					}
				}
			}
		}
		private async Task CallChangeAsync(string elementId)
		{
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("CallChange", elementId);
			}
			await ApplyFilter();
		}
		public async Task<IEnumerable<Language>> FilterLanguages(string searchText)
		{
			var languages = await LanguageService.GetLanguagesAsync(searchText);
			return languages;
		}
		public int? GetLanguageId(Language language)
		{
			return language?.Id;
		}
		public Language? LoadSelectedLanguage(int? languageId)
		{
			if (languageId != null && Languages != null)
			{
				var language = Languages.FirstOrDefault(l => l.Id == languageId);
				return language;
			}
			return null;
		}
		public async Task<IEnumerable<Category>> FilterCategories(string searchText)
		{
			var categories = await CategoryService.GetCategoriesAsync(searchText);
			return categories;
		}
		public int? GetCategoryId(Category category)
		{
			return category?.Id;
		}
		public Category? LoadSelectedCategory(int? categoryId)
		{
			if (categoryId != null && Categories != null)
			{
				var category = Categories.FirstOrDefault(c => c.Id == categoryId);
				return category;
			}
			return null;
		}
		private async Task CopyItemAsync(string itemToCopy, int customIntellisenseId)
		{
			customIntelliSenseCurrent = intellisenses!.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
			if (customIntelliSenseCurrent != null)
			{
				itemToCopy = FillInVariables(itemToCopy, customIntelliSenseCurrent);
			}
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);
				var message = $"Copied Successfully: '{itemToCopy}'";
				ToastService!.ShowSuccess(message);
			}
			await CloseApplication.InvokeAsync();
		}
		private async Task CopyAndPasteAsync(string itemToCopyAndPaste, int customIntellisenseId)
		{
			customIntelliSenseCurrent = intellisenses!.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
			if (customIntelliSenseCurrent != null)
			{
				itemToCopyAndPaste = FillInVariables(itemToCopyAndPaste, customIntelliSenseCurrent);
				if (JSRuntime != null)
				{
					await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopyAndPaste);
					var message = $"Copied Successfully: '{itemToCopyAndPaste}'";
					ToastService!.ShowSuccess(message);
				}
				CustomIntelliSenseDTO customIntelliSenseDTO = new CustomIntelliSenseDTO();
				customIntelliSenseDTO.SelectCharactersLeft = customIntelliSenseCurrent.SelectCharactersLeft;
				customIntelliSenseDTO.SelectWordFromRight = customIntelliSenseCurrent.SelectWordFromRight;
				customIntelliSenseDTO.MoveCharactersLeft = customIntelliSenseCurrent.MoveCharactersLeft;
				CustomIntelliSenseDataService.SendSnippet(itemToCopyAndPaste, customIntelliSenseDTO);
				if (RunningInBlazorHybrid)
				{
					await CloseApplication.InvokeAsync();
				}
			}
		}
		private async Task EditAsync(int id)
		{
			await MaximizeApplication.InvokeAsync();
			var parameters = new ModalParameters();
			parameters.Add("Id", id);
			var options = new ModalOptions()
			{
				Class = "blazored-modal-custom",
				Size = ModalSize.ExtraLarge
			};
			var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Edit Custom IntelliSense", parameters, options);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
					await ApplyFilter();
				}
			}
			CustomIntelliSenseId = id;

		}
		private void ShowValue(int customInTeleSenseId)
		{
			customIntelliSenseCurrent = intellisenses!.Where(i => i.Id == customInTeleSenseId).FirstOrDefault();
			if (customIntelliSenseCurrent != null && !customIntelliSenseCurrent!.Category!.CategoryName.ToLower().Contains("password") && !customIntelliSenseCurrent!.DisplayValue!.ToLower().Contains("password"))
			{
				if (customIntelliSenseCurrent.SendKeysValue != null)
				{
					customIntelliSenseCurrent.SendKeysValue = FillInVariables(customIntelliSenseCurrent.SendKeysValue, customIntelliSenseCurrent);
				}
			}
			else if (customIntelliSenseCurrent != null)
			{
				customIntelliSenseCurrent.SendKeysValue = "********";
			}
		}
		private string FillInVariables(string itemToCopy, CustomIntelliSense CustomIntelliSenseDTO)
		{
			if (itemToCopy.Contains("`Variable1`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable1))
			{
				itemToCopy = itemToCopy.Replace("`Variable1`", CustomIntelliSenseDTO.Variable1);
			}
			if (itemToCopy.Contains("`Variable2`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable2))
			{
				itemToCopy = itemToCopy.Replace("`Variable2`", CustomIntelliSenseDTO.Variable2);
			}
			if (itemToCopy.Contains("`Variable3`") && !string.IsNullOrWhiteSpace(CustomIntelliSenseDTO.Variable3))
			{
				itemToCopy = itemToCopy.Replace("`Variable3`", CustomIntelliSenseDTO.Variable3);
			}

			return itemToCopy;
		}
	}
}
