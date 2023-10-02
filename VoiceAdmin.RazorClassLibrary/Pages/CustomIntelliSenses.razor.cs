﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

using WindowsInput;
using WindowsInput.Native;

namespace RazorClassLibrary.Pages
{
	public partial class CustomIntelliSenses : ComponentBase
	{
		[Parameter] public int? CategoryIdFilter { get; set; } = 0;
		[Parameter] public int? LanguageIdFilter { get; set; } = 0;
		 private  string ? _languageFilter;
		 private  string ? _categoryFilter;
		[Inject] public AdditionalCommandService? AdditionalCommandService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		[Inject] public required IJSRuntime? JSRuntime { get; set; }
		[Inject] public required CustomIntellisenseService CustomIntellisenseService { get; set; }
		[Inject] public required CategoryService CategoryService { get; set; }
		[Inject] public required LanguageService LanguageService { get; set; }
		[Inject] public required GeneralLookupService GeneralLookupService { get; set; }
		[Inject] public required NavigationManager NavigationManager { get; set; }

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

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			await LoadData();
		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender && JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
			}
		}
		private async Task RemoveFilter()
		{
			CategoryIdFilter = null;
			LanguageIdFilter = null;
			SearchTerm = "";
			category = null;
			language = null;
			_languageFilter = null;
			_categoryFilter = null;
			await LoadData();
		}
		private async Task LoadData()
		{
			var query = new Uri(NavigationManager.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("language", out var languageName))
			{
				language = await LanguageService.GetLanguageAsync(languageName);
				LanguageIdFilter = language?.Id;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("category", out var categoryName))
			{
				category = await CategoryService.GetCategoryAsync(categoryName, "IntelliSense Command");
				CategoryIdFilter = category?.Id;
			}
			if (CategoryIdFilter != null && LanguageIdFilter != null)
			{
				await FilterByLanguageAndCategory();
				category = await CategoryService.GetCategoryAsync(CategoryIdFilter.Value);
				language = await LanguageService.GetLanguageAsync(LanguageIdFilter.Value);
			}
			else if (CategoryIdFilter != null)
			{
				await FilterByCategory();
				category = await CategoryService.GetCategoryAsync(CategoryIdFilter.Value);
			}
			else if (LanguageIdFilter != null)
			{
				await FilterByLanguage();
				language = await LanguageService.GetLanguageAsync(LanguageIdFilter.Value);
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
			if (SearchTerm != null||_languageFilter != null ||_categoryFilter != null )
			{
				try
				{
					intellisenses = await CustomIntellisenseService.GetCustomIntelliSensesAsync(SearchTerm?.Trim(), categoryIdFilter: CategoryIdFilter, languageIdFilter: LanguageIdFilter, maximumRows: MaximumRows,languageFilter: _languageFilter,categoryFilter: _categoryFilter);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
				StateHasChanged();
			}
		}
		private async Task ApplySpecialFilter(int languageId, int categoryId)
		{
			CategoryIdFilter = categoryId;
			category= await CategoryService.GetCategoryAsync(categoryId);
			LanguageIdFilter = languageId;
			language = await LanguageService.GetLanguageAsync(languageId);
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
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			var additionalCommands = await AdditionalCommandService!.GetAdditionalCommandsAsync(customIntellisenseId);
			if (additionalCommands.Count > 0)
			{
				ShowDialog = false;
				ToastService!.ShowWarning("Please remove additional commands before deleting this record!", "Delete Cancelled");
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
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
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
		private void CreateNew()
		{
			// By default set the language and category defaults to whatever is filtered
			if (LanguageIdFilter != null && CategoryIdFilter != null)
			{
				NavigationManager.NavigateTo($"/intellisense?languageId={LanguageIdFilter}&categoryId={CategoryIdFilter}");
				return;
			}
			if (LanguageIdFilter != null)
			{
				NavigationManager.NavigateTo($"/intellisense?languageId={LanguageIdFilter}");
				return;
			}
			if (CategoryIdFilter != null)
			{
				NavigationManager.NavigateTo($"/intellisense?categoryId={CategoryIdFilter}");
				return;
			}

			NavigationManager.NavigateTo($"/intellisense");
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
		private async Task CopyItemAsync(string itemToCopy)
		{
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);
				var message = $"Copied Successfully: '{itemToCopy}'";
				ToastService!.ShowSuccess(message, "Copy Item");
			}
		}
		private async Task CopyAndPasteAsync(string itemToCopyAndPaste)
		{
			if (JSRuntime != null)
			{
				await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopyAndPaste);
				var message = $"Copied Successfully: '{itemToCopyAndPaste}'";
				InputSimulator simulator = new InputSimulator();
				simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
				simulator.Keyboard.Sleep(100);
				simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
				simulator.Keyboard.Sleep(100);
				simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
				ToastService!.ShowSuccess(message, "Success");
			}
		}
		private async Task EditAsync(int id)
		{
			var parameters = new ModalParameters();
			parameters.Add("Id", id);
			var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Edit Custom IntelliSense", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
				}
			}
			CustomIntelliSenseId = id;

		}
	}
}