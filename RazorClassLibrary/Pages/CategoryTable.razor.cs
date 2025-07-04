using Ardalis.GuardClauses;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using RazorClassLibrary.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;
using SmartComponents.LocalEmbeddings;
using DataAccessLibrary.Services;

namespace RazorClassLibrary.Pages
{
	public partial class CategoryTable : ComponentBase
	{
		[Inject] public ICategoryDataService? CategoryDataService { get; set; }
		[Inject] public NavigationManager? NavigationManager { get; set; }
		[Inject] public ILogger<CategoryTable>? Logger { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		public string Title { get; set; } = "Category Items (Categories)";
		public string pageTitle { get; set; } = "Snippet Categories";
		public List<CategoryDTO>? CategoryDTO { get; set; }
		public List<CategoryDTO>? FilteredCategoryDTO { get; set; }
		public List<CategoryGroupedByLanguageDTO>? GroupedCategoryDTO { get; set; }
		public List<CategoryGroupedByLanguageDTO>? FilteredGroupedCategoryDTO { get; set; }
		protected CategoryAddEdit? CategoryAddEdit { get; set; }
		private bool useSemanticMatching = false;
		private bool _groupByLanguage = true; // Default to grouped view
		ElementReference SearchInput;
#pragma warning disable 414, 649
		private bool _loadFailed = false;
		private string? searchTerm = null;
#pragma warning restore 414, 649
		private int? counter = 0;
		private int maxResults = 5;

		public string? SearchTerm
		{
			get => searchTerm; set
			{
				searchTerm = value;
				if (searchTerm != null)
				{
					if (searchTerm.Length > 0)
					{
						ApplyFilter();
					}
				}
				if (string.IsNullOrWhiteSpace(searchTerm))
				{
					FilteredCategoryDTO = CategoryDTO;
				}
			}
		}
		[Parameter]
		public string GlobalSearchTerm { get; set; } = "";
		public string ExceptionMessage { get; set; } = string.Empty;
		public List<string>? PropertyInfo { get; set; }
		[CascadingParameter] public ClaimsPrincipal? User { get; set; }
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		[Parameter] public required string CategoryType { get; set; } = "Launch Applications";
		private string drillDownUrl { get; set; } = "/intellisensesC";
		private bool _ShowCards { get; set; } = true;
		protected override async Task OnInitializedAsync()
		{
			await LoadData();
		}
		protected override async Task OnParametersSetAsync() { await LoadData(); }
		private async Task LoadData()
		{
			try
			{
				var query = new Uri(NavigationManager!.Uri).Query;
				if (QueryHelpers.ParseQuery(query).TryGetValue("categorytype", out var categoryType))
				{
					CategoryType = categoryType.ToString() ?? "Launch Applications";
				}
				if (CategoryType == "Launch Applications")
				{
					drillDownUrl = "/launcherstable";
				}
				if (CategoryDataService != null)
				{
					pageTitle = "Snippet Categories";
					if (CategoryType == "Launch Applications")
					{
						pageTitle = "Launcher Categories";
					}
					
					if (string.IsNullOrWhiteSpace(GlobalSearchTerm))
					{
						// Load grouped data for normal view
						var groupedResult = await CategoryDataService.GetCategoriesGroupedByLanguageAsync(CategoryType);
						GroupedCategoryDTO = groupedResult.ToList();
						FilteredGroupedCategoryDTO = groupedResult.ToList();
						
						// Also load flat data for compatibility
						var flatResult = await CategoryDataService.GetAllCategoriesAsync(CategoryType, 0);
						CategoryDTO = flatResult.ToList();
						FilteredCategoryDTO = flatResult.ToList();
					}
					else
					{
						// For search, use flat view
						_groupByLanguage = false;
						_ShowCards = false;
						var result = await CategoryDataService.SearchCategoriesAsync(GlobalSearchTerm);
						CategoryDTO = result.ToList();
						FilteredCategoryDTO = result.ToList();
					}
				}

			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
				_loadFailed = true;
				ExceptionMessage = e.Message;
			}
			
			// Update title based on current view
			if (_groupByLanguage && FilteredGroupedCategoryDTO != null)
			{
				var totalCategories = FilteredGroupedCategoryDTO.Sum(g => g.Categories.Count);
				Title = $"Categories ({totalCategories})";
			}
			else if (FilteredCategoryDTO != null)
			{
				Title = $"Categories ({FilteredCategoryDTO.Count})";
			}
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
		protected async Task AddNewCategoryAsync()
		{
			var parameters = new ModalParameters();
			var formModal = Modal?.Show<CategoryAddEdit>("Add Category", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
					if (searchTerm != null)
					{
						ApplyFilter();
					}
				}
			}
		}

		private void ApplyFilter()
		{
			if (_groupByLanguage)
			{
				ApplyFilterGrouped();
			}
			else
			{
				ApplyFilterFlat();
			}
		}
		
		private void ApplyFilterGrouped()
		{
			if (FilteredGroupedCategoryDTO == null || GroupedCategoryDTO == null)
			{
				return;
			}
			
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredGroupedCategoryDTO = GroupedCategoryDTO.ToList();
				var totalCategories = FilteredGroupedCategoryDTO.Sum(g => g.Categories.Count);
				Title = $"All Categories ({totalCategories})";
			}
			else
			{
				var temporary = SearchTerm.ToLower().Trim();
				var filteredGroups = new List<CategoryGroupedByLanguageDTO>();
				
				foreach (var group in GroupedCategoryDTO)
				{
					var filteredCategories = group.Categories.Where(c =>
						(c.CategoryName != null && c.CategoryName.ToLower().Contains(temporary)) ||
						(c.CategoryType != null && c.CategoryType.ToLower().Contains(temporary))
					).ToList();
					
					if (filteredCategories.Any())
					{
						filteredGroups.Add(new CategoryGroupedByLanguageDTO
						{
							LanguageId = group.LanguageId,
							LanguageName = group.LanguageName,
							LanguageColour = group.LanguageColour,
							Categories = filteredCategories
						});
					}
				}
				
				FilteredGroupedCategoryDTO = filteredGroups;
				var totalCategories = FilteredGroupedCategoryDTO.Sum(g => g.Categories.Count);
				Title = $"Filtered Categories ({totalCategories})";
			}
		}
		
		private void ApplyFilterFlat()
		{
			if (FilteredCategoryDTO == null || CategoryDTO == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredCategoryDTO = CategoryDTO.OrderBy(v => v.CategoryName).ToList();
				Title = $"All Category ({FilteredCategoryDTO.Count})";
			}
			else
			{
				var temporary = SearchTerm.ToLower().Trim();
				if (useSemanticMatching && searchTerm != null && searchTerm.Length > 4)
				{
					var categories = CategoryDTO.Where(f => f.CategoryName != null).Select(v => v.CategoryName).ToList();
					using var localCommandEmbedder = new LocalEmbedder();
					IList<(string Item, EmbeddingF32 Embedding)> matchedResults;
					matchedResults = localCommandEmbedder.EmbedRange(
						categories.ToList());
					SimilarityScore<string>[] results = LocalEmbedder.FindClosestWithScore(localCommandEmbedder.Embed(searchTerm), matchedResults, maxResults: maxResults);
					foreach (var result in results)
					{
						Console.WriteLine($"Item: {result.Item} Score: {result.Similarity}");
					}
					if (results.Length > 0)
					{
						var resultItems = results.Select(r => r.Item).ToList();
						var similarityScores = results.ToDictionary(r => r.Item, r => r.Similarity);

						FilteredCategoryDTO = CategoryDTO
							.Where(v => resultItems.Contains(v.CategoryName))
							.OrderByDescending(v => similarityScores[v.CategoryName])
							.ToList();
					}
				}
				else
				{
					FilteredCategoryDTO = CategoryDTO
						 .Where(v =>
						 v.CategoryName != null && v.CategoryName.ToLower().Contains(temporary)
						  || v.CategoryType != null && v.CategoryType.ToLower().Contains(temporary)
						 )
						 .ToList();
					Title = $"Filtered Categorys ({FilteredCategoryDTO.Count})";
				}
			}
		}
		private void OnGroupingChanged(ChangeEventArgs e)
		{
			_groupByLanguage = bool.Parse(e.Value?.ToString() ?? "false");
			if (_groupByLanguage)
			{
				ApplyFilterGrouped();
			}
			else
			{
				ApplyFilterFlat();
			}
			StateHasChanged();
		}
		
		protected void SortCategory(string sortColumn)
		{
			Guard.Against.Null(sortColumn, nameof(sortColumn));
			if (FilteredCategoryDTO == null)
			{
				return;
			}
			if (sortColumn == "Category")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.CategoryName).ToList();
			}
			else if (sortColumn == "Category Desc")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.CategoryName).ToList();
			}
			if (sortColumn == "CategoryType")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.CategoryType).ToList();
			}
			else if (sortColumn == "CategoryType Desc")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.CategoryType).ToList();
			}
			if (sortColumn == "Sensitive")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderBy(v => v.Sensitive).ToList();
			}
			else if (sortColumn == "Sensitive Desc")
			{
				FilteredCategoryDTO = FilteredCategoryDTO.OrderByDescending(v => v.Sensitive).ToList();
			}
		}
		async Task DeleteCategoryAsync(int Id)
		{
			//Optionally remove child records here or warn about their existence
			//var ? = await ?DataService.GetAllCategory(Id);
			//if (? != null)
			//{
			//	ToastService.ShowWarning($"It is not possible to delete a category that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
			//	return;
			//}
			var parameters = new ModalParameters();
			if (CategoryDataService != null)
			{
				var category = await CategoryDataService.GetCategoryById(Id);
				parameters.Add("Title", "Please Confirm, Delete Category");
				parameters.Add("Message", $"Category: {category?.CategoryName}");
				parameters.Add("ButtonColour", "danger");
				parameters.Add("Icon", "fa fa-trash");
				var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Category ({category?.CategoryName})?", parameters);
				if (formModal != null)
				{
					var result = await formModal.Result;
					if (!result.Cancelled)
					{
						await CategoryDataService.DeleteCategory(Id);
						ToastService?.ShowSuccess(" Category deleted successfully");
						await LoadData();
						if (searchTerm != null)
						{
							ApplyFilter();
						}
					}
				}
			}
		}
		async Task EditCategoryAsync(int Id)
		{
			var parameters = new ModalParameters();
			parameters.Add("Id", Id);
			var formModal = Modal?.Show<CategoryAddEdit>("Edit Category", parameters);
			if (formModal != null)
			{
				var result = await formModal.Result;
				if (!result.Cancelled)
				{
					await LoadData();
					if (searchTerm != null)
					{
						ApplyFilter();
					}
				}
			}
		}
	}
}