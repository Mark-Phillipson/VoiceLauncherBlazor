using Ardalis.GuardClauses;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

using System.Security.Claims;
using System.Linq;
using VoiceLauncher.Services;
using RazorClassLibrary.Services;

namespace RazorClassLibrary.Pages
{
   public partial class CustomIntelliSenseTable : ComponentBase, IDisposable
   {
      [Inject] public required ICustomIntelliSenseDataService CustomIntelliSenseDataService { get; set; }
      [Inject] public required NavigationManager NavigationManager { get; set; }
      [Inject] public ILogger<CustomIntelliSenseTable>? Logger { get; set; }
      [Inject] public required ILanguageDataService LanguageDataService { get; set; }
      [Inject] public required ICategoryDataService CategoryDataService { get; set; }
      [Inject] public required IToastService ToastService { get; set; }
      [CascadingParameter] public IModalService? Modal { get; set; }
      [Inject] public required ComponentCacheService Cache { get; set; }
      public string Title { get; set; } = "CustomIntelliSense Items (CustomIntelliSenses)";
      public string EditTitle { get; set; } = "Edit CustomIntelliSense Item (CustomIntelliSenses)";
      [Parameter] public string GlobalSearchTerm { get; set; } = "";
      [Parameter] public int CategoryId { get; set; }
      [Parameter] public int LanguageId { get; set; }
      [Parameter] public bool RunningInBlazorHybrid { get; set; } = false;
      [Parameter] public EventCallback CloseApplication { get; set; }
      [Parameter] public EventCallback MaximizeApplication { get; set; }
      
      private LanguageDTO? currentLanguage { get; set; }
      private CategoryDTO? currentCategory { get; set; }
      public List<CustomIntelliSenseDTO>? CustomIntelliSenseDTO { get; set; }
      public List<CustomIntelliSenseDTO>? FilteredCustomIntelliSenseDTO { get; set; }
      protected CustomIntelliSenseAddEdit? CustomIntelliSenseAddEdit { get; set; }
      ElementReference SearchInput;
#pragma warning disable 414, 649
      private bool _loadFailed = false;
      private string? searchTerm = null;
#pragma warning restore 414, 649
      public string? SearchTerm
      {
         get => searchTerm;
         set
         {
            searchTerm = value;
            _ = InvokeAsync(() => ApplyFilter());
         }
      }
      // Lookup lists and selection state for header typeahead controls
      public List<LanguageDTO> Languages { get; set; } = new List<LanguageDTO>();
      public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
      public int SelectedLanguageId { get; set; }
      public int SelectedCategoryId { get; set; }
      public string LanguageQuery { get; set; } = "";
      public string CategoryQuery { get; set; } = "";
      public List<LanguageDTO> LanguageSuggestions { get; set; } = new List<LanguageDTO>();
      public List<CategoryDTO> CategorySuggestions { get; set; } = new List<CategoryDTO>();
      public bool ShowLanguageSuggestions { get; set; } = false;
      public bool ShowCategorySuggestions { get; set; } = false;
      private int LanguageSuggestionIndex { get; set; } = -1;
      private int CategorySuggestionIndex { get; set; } = -1;

      private async Task LoadLookups()
      {
         try
         {
            // Load languages first so we can fetch categories for the selected language
            Languages = await LanguageDataService.GetAllLanguagesAsync();
            SelectedLanguageId = LanguageId;
            SelectedCategoryId = CategoryId;
            // Load categories filtered by the selected language when possible
            if (SelectedLanguageId > 0)
            {
               Categories = await CategoryDataService.GetAllCategoriesAsync("IntelliSense Command", SelectedLanguageId);
            }
               else
            {
               Categories = await CategoryDataService.GetAllCategoriesByTypeAsync("IntelliSense Command");
            }

            LanguageQuery = Languages.FirstOrDefault(l => l.Id == SelectedLanguageId)?.LanguageName ?? "";
            CategoryQuery = Categories.FirstOrDefault(c => c.Id == SelectedCategoryId)?.CategoryName ?? "";
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
         }

      
      private async Task OnLanguageInput(ChangeEventArgs e)
      {
         LanguageQuery = e?.Value?.ToString() ?? "";
         if (string.IsNullOrWhiteSpace(LanguageQuery))
         {
            LanguageSuggestions.Clear();
            ShowLanguageSuggestions = false;
            // If no language query, load all categories
            Categories = await CategoryDataService.GetAllCategoriesByTypeAsync("IntelliSense Command");
         }
         else
         {
            LanguageSuggestions = Languages.Where(l => (!string.IsNullOrEmpty(l.LanguageName) && l.LanguageName.Contains(LanguageQuery, StringComparison.OrdinalIgnoreCase))).Take(20).ToList();
            ShowLanguageSuggestions = LanguageSuggestions.Any();

            // If the query exactly matches a language, proactively load categories for that language
            var exactMatch = Languages.FirstOrDefault(l => string.Equals(l.LanguageName?.Trim(), LanguageQuery.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
            {
               SelectedLanguageId = exactMatch.Id;
               // Load categories filtered to this language
                  await LoadCategoriesForLanguage(SelectedLanguageId);
            }
         }
      }

         private async Task OnLanguageKeyDown(KeyboardEventArgs e)
         {
            if ((e.Key == "ArrowDown" || e.Code == "ArrowDown") && ShowLanguageSuggestions && LanguageSuggestions?.Count > 0)
            {
               LanguageSuggestionIndex = 0;
               await FocusLanguageSuggestion(LanguageSuggestionIndex);
            }
            else if (e.Key == "Enter")
            {
               // If a suggestion is highlighted, activate it.
               if (LanguageSuggestionIndex >= 0 && LanguageSuggestions != null && LanguageSuggestionIndex < LanguageSuggestions.Count)
               {
                  var lang = LanguageSuggestions[LanguageSuggestionIndex];
                  await SelectLanguageById(lang.Id, lang.LanguageName);
               }
               // If nothing is highlighted but suggestions exist, activate the first suggestion.
               else if (LanguageSuggestions != null && LanguageSuggestions.Count > 0)
               {
                  var lang = LanguageSuggestions[0];
                  await SelectLanguageById(lang.Id, lang.LanguageName);
               }
               // Fallback: if no suggestions but the input exactly matches a language, activate it.
               else
               {
                  var exact = Languages.FirstOrDefault(l => string.Equals(l.LanguageName?.Trim(), LanguageQuery.Trim(), StringComparison.OrdinalIgnoreCase));
                  if (exact != null)
                  {
                     await SelectLanguageById(exact.Id, exact.LanguageName);
                  }
               }
               // Prevent default browser behavior when possible (handled on client side if needed)
            }
            else if (e.Key == "Escape")
            {
               ShowLanguageSuggestions = false;
               LanguageSuggestionIndex = -1;
            }
         }

      private void OnCategoryInput(ChangeEventArgs e)
      {
         CategoryQuery = e?.Value?.ToString() ?? "";
         if (string.IsNullOrWhiteSpace(CategoryQuery))
         {
            CategorySuggestions.Clear();
            ShowCategorySuggestions = false;
         }
         else
         {
            CategorySuggestions = Categories.Where(c => (!string.IsNullOrEmpty(c.CategoryName) && c.CategoryName.Contains(CategoryQuery, StringComparison.OrdinalIgnoreCase))).Take(20).ToList();
            ShowCategorySuggestions = CategorySuggestions.Any();
         }
      }

      private async Task OnCategoryKeyDown(KeyboardEventArgs e)
      {
         if ((e.Key == "ArrowDown" || e.Code == "ArrowDown") && ShowCategorySuggestions && CategorySuggestions?.Count > 0)
         {
            CategorySuggestionIndex = 0;
            await FocusCategorySuggestion(CategorySuggestionIndex);
         }
         else if (e.Key == "Enter")
         {
            if (CategorySuggestionIndex >= 0 && CategorySuggestions != null && CategorySuggestionIndex < CategorySuggestions.Count)
            {
               var cat = CategorySuggestions[CategorySuggestionIndex];
               await SelectCategoryById(cat.Id, cat.CategoryName);
            }
            else if (CategorySuggestions != null && CategorySuggestions.Count > 0)
            {
               var cat = CategorySuggestions[0];
               await SelectCategoryById(cat.Id, cat.CategoryName);
            }
            else
            {
               var exact = Categories.FirstOrDefault(c => string.Equals(c.CategoryName?.Trim(), CategoryQuery.Trim(), StringComparison.OrdinalIgnoreCase));
               if (exact != null)
               {
                  await SelectCategoryById(exact.Id, exact.CategoryName);
               }
            }
         }
         else if (e.Key == "Escape")
         {
            ShowCategorySuggestions = false;
            CategorySuggestionIndex = -1;
         }
      }

      private async Task LoadCategoriesForLanguage(int languageId)
      {
         try
         {
            if (languageId <= 0)
            {
               Categories = await CategoryDataService.GetAllCategoriesByTypeAsync("IntelliSense Command");
            }
            else
            {
               Categories = await CategoryDataService.GetAllCategoriesAsync("IntelliSense Command", languageId);
            }
            // Update CategoryQuery to keep selected category visible if still applicable
            CategoryQuery = Categories.FirstOrDefault(c => c.Id == SelectedCategoryId)?.CategoryName ?? CategoryQuery;
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      private async Task OnLanguageSuggestionKeyDown(KeyboardEventArgs e, int index)
      {
         // Guard against stale or out-of-range indices (suggestions can change between render and event)
         if (LanguageSuggestions == null || index < 0 || index >= LanguageSuggestions.Count)
         {
            ShowLanguageSuggestions = false;
            LanguageSuggestionIndex = -1;
            try { await JSRuntime.InvokeVoidAsync("setFocus", "LanguageInput"); } catch { }
            return;
         }
         if (e.Key == "ArrowDown")
         {
            var next = index + 1;
            if (next < LanguageSuggestions.Count)
            {
               LanguageSuggestionIndex = next;
               await FocusLanguageSuggestion(next);
            }
         }
         else if (e.Key == "ArrowUp")
         {
            var prev = index - 1;
            if (prev >= 0)
            {
               LanguageSuggestionIndex = prev;
               await FocusLanguageSuggestion(prev);
            }
            else
            {
               LanguageSuggestionIndex = -1;
               await JSRuntime.InvokeVoidAsync("setFocus", "LanguageInput");
            }
         }
         else if (e.Key == "Enter")
         {
            if (index >= 0 && index < LanguageSuggestions.Count)
            {
               var lang = LanguageSuggestions[index];
               await SelectLanguageById(lang.Id, lang.LanguageName);
            }
         }
         else if (e.Key == "Escape")
         {
            ShowLanguageSuggestions = false;
            LanguageSuggestionIndex = -1;
            await JSRuntime.InvokeVoidAsync("setFocus", "LanguageInput");
         }
      }

      private async Task OnCategorySuggestionKeyDown(KeyboardEventArgs e, int index)
      {
         // Guard against stale or out-of-range indices
         if (CategorySuggestions == null || index < 0 || index >= CategorySuggestions.Count)
         {
            ShowCategorySuggestions = false;
            CategorySuggestionIndex = -1;
            try { await JSRuntime.InvokeVoidAsync("setFocus", "CategoryInput"); } catch { }
            return;
         }
         if (e.Key == "ArrowDown")
         {
            var next = index + 1;
            if (next < CategorySuggestions.Count)
            {
               CategorySuggestionIndex = next;
               await FocusCategorySuggestion(next);
            }
         }
         else if (e.Key == "ArrowUp")
         {
            var prev = index - 1;
            if (prev >= 0)
            {
               CategorySuggestionIndex = prev;
               await FocusCategorySuggestion(prev);
            }
            else
            {
               CategorySuggestionIndex = -1;
               await JSRuntime.InvokeVoidAsync("setFocus", "CategoryInput");
            }
         }
         else if (e.Key == "Enter")
         {
            if (index >= 0 && index < CategorySuggestions.Count)
            {
               var cat = CategorySuggestions[index];
               await SelectCategoryById(cat.Id, cat.CategoryName);
            }
         }
         else if (e.Key == "Escape")
         {
            ShowCategorySuggestions = false;
            CategorySuggestionIndex = -1;
            await JSRuntime.InvokeVoidAsync("setFocus", "CategoryInput");
         }
      }

      private async Task FocusLanguageSuggestion(int index)
      {
         try
         {
            await JSRuntime.InvokeVoidAsync("setFocus", $"LanguageSuggestion_{index}");
         }
         catch { }
      }

      private async Task FocusCategorySuggestion(int index)
      {
         try
         {
            await JSRuntime.InvokeVoidAsync("setFocus", $"CategorySuggestion_{index}");
         }
         catch { }
      }

      // Handle Enter/Arrow/Escape when the suggestions list container has focus
      private async Task OnLanguageSuggestionsListKeyDown(KeyboardEventArgs e)
      {
         if (e.Key == "Enter")
         {
            if (LanguageSuggestionIndex >= 0 && LanguageSuggestions != null && LanguageSuggestionIndex < LanguageSuggestions.Count)
            {
               var lang = LanguageSuggestions[LanguageSuggestionIndex];
               await SelectLanguageById(lang.Id, lang.LanguageName);
            }
            else if (LanguageSuggestions != null && LanguageSuggestions.Count > 0)
            {
               var lang = LanguageSuggestions[0];
               await SelectLanguageById(lang.Id, lang.LanguageName);
            }
            return;
         }

         if (e.Key == "ArrowDown")
         {
            var next = Math.Min(LanguageSuggestionIndex + 1, (LanguageSuggestions?.Count ?? 1) - 1);
            if (next >= 0 && LanguageSuggestions != null && next < LanguageSuggestions.Count)
            {
               LanguageSuggestionIndex = next;
               await FocusLanguageSuggestion(next);
            }
         }
         else if (e.Key == "ArrowUp")
         {
            var prev = LanguageSuggestionIndex - 1;
            if (prev >= 0)
            {
               LanguageSuggestionIndex = prev;
               await FocusLanguageSuggestion(prev);
            }
            else
            {
               LanguageSuggestionIndex = -1;
               try { await JSRuntime.InvokeVoidAsync("setFocus", "LanguageInput"); } catch { }
            }
         }
         else if (e.Key == "Escape")
         {
            ShowLanguageSuggestions = false;
            LanguageSuggestionIndex = -1;
            try { await JSRuntime.InvokeVoidAsync("setFocus", "LanguageInput"); } catch { }
         }
      }

      private async Task OnCategorySuggestionsListKeyDown(KeyboardEventArgs e)
      {
         if (e.Key == "Enter")
         {
            if (CategorySuggestionIndex >= 0 && CategorySuggestions != null && CategorySuggestionIndex < CategorySuggestions.Count)
            {
               var cat = CategorySuggestions[CategorySuggestionIndex];
               await SelectCategoryById(cat.Id, cat.CategoryName);
            }
            else if (CategorySuggestions != null && CategorySuggestions.Count > 0)
            {
               var cat = CategorySuggestions[0];
               await SelectCategoryById(cat.Id, cat.CategoryName);
            }
            return;
         }

         if (e.Key == "ArrowDown")
         {
            var next = Math.Min(CategorySuggestionIndex + 1, (CategorySuggestions?.Count ?? 1) - 1);
            if (next >= 0 && CategorySuggestions != null && next < CategorySuggestions.Count)
            {
               CategorySuggestionIndex = next;
               await FocusCategorySuggestion(next);
            }
         }
         else if (e.Key == "ArrowUp")
         {
            var prev = CategorySuggestionIndex - 1;
            if (prev >= 0)
            {
               CategorySuggestionIndex = prev;
               await FocusCategorySuggestion(prev);
            }
            else
            {
               CategorySuggestionIndex = -1;
               try { await JSRuntime.InvokeVoidAsync("setFocus", "CategoryInput"); } catch { }
            }
         }
         else if (e.Key == "Escape")
         {
            ShowCategorySuggestions = false;
            CategorySuggestionIndex = -1;
            try { await JSRuntime.InvokeVoidAsync("setFocus", "CategoryInput"); } catch { }
         }
      }

      private async Task SelectLanguageById(int id, string? name = null)
      {
         SelectedLanguageId = id;
         LanguageQuery = name ?? Languages.FirstOrDefault(l => l.Id == id)?.LanguageName ?? "";
         ShowLanguageSuggestions = false;
         LanguageSuggestionIndex = -1;
         LanguageId = SelectedLanguageId;
         await InvalidateCache();
      }

      private async Task SelectCategoryById(int id, string? name = null)
      {
         SelectedCategoryId = id;
         CategoryQuery = name ?? Categories.FirstOrDefault(c => c.Id == id)?.CategoryName ?? "";
         ShowCategorySuggestions = false;
         CategorySuggestionIndex = -1;
         CategoryId = SelectedCategoryId;
         await InvalidateCache();
      }
      public string ExceptionMessage { get; set; } = string.Empty;
      public List<string>? PropertyInfo { get; set; }
      [CascadingParameter] public ClaimsPrincipal? User { get; set; }
      [Inject] public required IJSRuntime JSRuntime { get; set; }
      public bool ShowEdit { get; set; } = false;
      private bool ShowDeleteConfirm { get; set; }
      private int CustomIntelliSenseId { get; set; }
      private int pageNumber = 1;
      private int pageSize = 10;
      private int counter = 0;
      private int shortcutValue = 0;
      private CancellationTokenSource? _searchCancellation;
      private bool _isLoading = false;
      private Task? _prefetchTask;
      private const int SEARCH_DEBOUNCE_MS = 300;
      protected override async Task OnInitializedAsync()
      {
         Console.WriteLine($"CustomIntelliSenseTable OnInitializedAsync - GlobalSearchTerm: '{GlobalSearchTerm}', LanguageId: {LanguageId}, CategoryId: {CategoryId}");
         // Load data first (it may set defaults), then load lookups so the autocomplete textboxes are prefilled
         await LoadData();
         await LoadLookups();
      }
      
      protected override async Task OnParametersSetAsync()
      {
         Console.WriteLine($"CustomIntelliSenseTable OnParametersSetAsync - GlobalSearchTerm: '{GlobalSearchTerm}', LanguageId: {LanguageId}, CategoryId: {CategoryId}");
         if (!string.IsNullOrWhiteSpace(GlobalSearchTerm))
         {
            Console.WriteLine($"Global search term detected, triggering LoadData");
            await LoadData();
         }
      }
      private string GetCacheKey(string suffix = "", int? pageNum = null) =>
          $"CustomIntelliSenseTable_{LanguageId}_{CategoryId}_{pageNum ?? pageNumber}_{pageSize}_{GlobalSearchTerm}{suffix}";

      private string GetFilterCacheKey() =>
          $"CustomIntelliSenseTable_Filter_{LanguageId}_{CategoryId}_{pageNumber}_{pageSize}_{GlobalSearchTerm}_{SearchTerm}";

      private async Task InvalidateCache()
      {
         Cache.InvalidateCache($"CustomIntelliSenseTable_{LanguageId}_{CategoryId}");
         await LoadData();
         // Refresh lookup lists and update the input query text to reflect current language/category
         await LoadLookups();
      }

      // Reset the filter and clear cache
      protected async Task ResetFilter()
      {
         // Clear local search term
         searchTerm = null;

         // Invalidate cache and reload data
         await InvalidateCache();

         // Apply default filter (no search term)
         await ApplyFilter();

         // Refocus search input
         try
         {
            if (JSRuntime != null)
            {
               await JSRuntime.InvokeVoidAsync("setFocus", "SearchInput");
            }
         }
         catch { }
      }

        private void PrefetchNextPage()
        {
            try
            {
                // Cancel any existing prefetch
                if (_prefetchTask != null && !_prefetchTask.IsCompleted)
                {
                    return;
                }

                var nextPageKey = GetCacheKey("", pageNumber + 1);
                if (!string.IsNullOrWhiteSpace(GlobalSearchTerm))
                {
                    return; // Don't prefetch during global search
                }

                _prefetchTask = Cache.GetOrSetAsync(nextPageKey, async () =>
                {
                    var data = await CustomIntelliSenseDataService.GetAllCustomIntelliSensesAsync(
                   LanguageId, CategoryId, pageNumber + 1, pageSize);
                    return new { Data = data, Language = currentLanguage, Category = currentCategory };
                });
            }
            catch
            {
                // Ignore prefetch errors
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
                  await JSRuntime.InvokeVoidAsync("setFocus", "SearchInput");
               }
            }
            catch (Exception exception)
            {
               Console.WriteLine(exception.Message);
            }
         }
      }
      private async Task AddNewCustomIntelliSense()
      {
         await MaximizeApplication.InvokeAsync();
         var parameters = new ModalParameters();

         parameters.Add(nameof(CategoryId), CategoryId);
         parameters.Add(nameof(LanguageId), LanguageId);
         parameters.Add("RunningInBlazorHybrid", RunningInBlazorHybrid);
         var options = new ModalOptions()
         {
            Class = "blazored-modal-custom",
            Size = ModalSize.ExtraLarge
         };

         var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Add Custom Intelli Sense", parameters, options);
         if (formModal != null)
         {
            var result = await formModal.Result;
            if (!result.Cancelled)
            {
               await InvalidateCache();
            }
         }
         CustomIntelliSenseId = 0;
      }
      private async Task ApplyFilter()
      {
         // Cancel any previous search operation
         _searchCancellation?.Cancel();
         _searchCancellation?.Dispose();
         _searchCancellation = new CancellationTokenSource();

         try
         {
            // Debounce the search
            await Task.Delay(SEARCH_DEBOUNCE_MS, _searchCancellation.Token);

            if (FilteredCustomIntelliSenseDTO == null || CustomIntelliSenseDTO == null)
            {
               return;
            }

            if (string.IsNullOrEmpty(SearchTerm))
            {
               return;
            }
            else
            {
               var temporary = SearchTerm.ToLower().Trim();
               CustomIntelliSenseDTO =  await CustomIntelliSenseDataService.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, 1, 2000);
               var filtered = CustomIntelliSenseDTO
                   .Where(v =>
                       v.DisplayValue != null && v.DisplayValue.ToLower().Contains(temporary) ||
                       v.SendKeysValue != null && v.SendKeysValue.ToLower().Contains(temporary) ||
                       v.CommandType != null && v.CommandType.ToLower().Contains(temporary) ||
                       v.DeliveryType != null && v.DeliveryType.ToLower().Contains(temporary)
                   )
                   .ToList();
               FilteredCustomIntelliSenseDTO = filtered;
               StateHasChanged();
               return;
            }

         }
         catch (OperationCanceledException)
         {
            // Search was cancelled, ignore
         }
      }
      protected void SortCustomIntelliSense(string sortColumn)
      {
         Guard.Against.Null(sortColumn, nameof(sortColumn));
         if (FilteredCustomIntelliSenseDTO == null)
         {
            return;
         }
         if (sortColumn == "DisplayValue")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.DisplayValue).ToList();
         }
         else if (sortColumn == "DisplayValue Desc")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.DisplayValue).ToList();
         }
         if (sortColumn == "SendKeysValue")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.SendKeysValue).ToList();
         }
         else if (sortColumn == "SendKeysValue Desc")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.SendKeysValue).ToList();
         }
         if (sortColumn == "CommandType")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.CommandType).ToList();
         }
         else if (sortColumn == "CommandType Desc")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.CommandType).ToList();
         }
         if (sortColumn == "DeliveryType")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.DeliveryType).ToList();
         }
         else if (sortColumn == "DeliveryType Desc")
         {
            FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.DeliveryType).ToList();
         }
      }
      private async Task DeleteCustomIntelliSense(int Id)
      {
         var parameters = new ModalParameters();
         if (CustomIntelliSenseDataService != null)
         {
            var customIntelliSense = await CustomIntelliSenseDataService.GetCustomIntelliSenseById(Id);
            parameters.Add("Title", "Please Confirm, Delete Custom Intelli Sense");
            parameters.Add("Message", $"DisplayValue: {customIntelliSense?.DisplayValue}");
            parameters.Add("ButtonColour", "danger");
            parameters.Add("Icon", "fa fa-trash");
            var formModal = Modal?.Show<RazorClassLibrary.Shared.BlazoredModalConfirmDialog>($"Delete Custom Intelli Sense ({customIntelliSense?.DisplayValue})?", parameters);
            if (formModal != null)
            {
               var result = await formModal.Result;
               if (!result.Cancelled)
               {
                  await CustomIntelliSenseDataService.DeleteCustomIntelliSense(Id);
                  ToastService?.ShowSuccess("Custom Intelli Sense deleted successfully");
                  await InvalidateCache();
               }
            }
         }
         CustomIntelliSenseId = Id;
      }

      private async void EditCustomIntelliSense(int Id)
      {
         await MaximizeApplication.InvokeAsync();
         var parameters = new ModalParameters();
         parameters.Add("Id", Id);
         parameters.Add("RunningInBlazorHybrid", RunningInBlazorHybrid);
         var options = new ModalOptions()
         {
            Class = "blazored-modal-custom",
            Size = ModalSize.ExtraLarge
         };
         var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Edit Custom Intelli Sense", parameters, options);
         if (formModal != null)
         {
            var result = await formModal.Result;
            if (!result.Cancelled)
            {
               await InvalidateCache();
            }
         }
         CustomIntelliSenseId = Id;
      }
      private async Task CopyItemAsync(int customIntellisenseId)
      {
         var customIntelliSenseCurrent = FilteredCustomIntelliSenseDTO!.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
         if (customIntelliSenseCurrent != null)
         {
            customIntelliSenseCurrent.SendKeysValue = FillInVariables(customIntelliSenseCurrent.SendKeysValue, customIntelliSenseCurrent);
         }
         if (JSRuntime != null && customIntelliSenseCurrent != null)
         {
            await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", customIntelliSenseCurrent.SendKeysValue);
            var message = $"Copied Successfully: '{customIntelliSenseCurrent.SendKeysValue}'";
            ToastService!.ShowSuccess(message);
         }
         if (RunningInBlazorHybrid)
         {
            await CloseApplication.InvokeAsync();
         }

      }
      private string FillInVariables(string itemToCopy, CustomIntelliSenseDTO CustomIntelliSenseDTO)
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
      private async Task CopyAndPasteAsync(int customIntellisenseId)
      {
         var customIntelliSenseCurrent = FilteredCustomIntelliSenseDTO!.Where(i => i.Id == customIntellisenseId).FirstOrDefault();
         if (customIntelliSenseCurrent != null)
         {
            customIntelliSenseCurrent.SendKeysValue = FillInVariables(customIntelliSenseCurrent.SendKeysValue, customIntelliSenseCurrent);
            CustomIntelliSenseDataService.SendSnippet(customIntelliSenseCurrent.SendKeysValue, customIntelliSenseCurrent);
         }
         await CopyItemAsync(customIntellisenseId);

         // if (JSRuntime != null && customIntelliSenseCurrent != null)
         // {
         //    try
         //    {
         //       // await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopyAndPaste);
         //       await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", customIntelliSenseCurrent.SendKeysValue);
         //    }
         //    catch (System.Exception exception)
         //    {
         //       System.Console.WriteLine(exception.Message);
         //    }
         //    var message = $"Copied Successfully: '{customIntelliSenseCurrent.SendKeysValue}'";
         //    ToastService!.ShowSuccess(message);
         // }
         await Task.Delay(3000);
         if (RunningInBlazorHybrid)
         {
            await CloseApplication.InvokeAsync();
         }

      }

      // Helper method to highlight search terms
      private string HighlightSearchTerm(string text, string searchTerm)
      {
         if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchTerm))
            return text ?? "";
            
         var index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
         if (index >= 0)
         {
            var before = text.Substring(0, index);
            var match = text.Substring(index, searchTerm.Length);
            var after = text.Substring(index + searchTerm.Length);
            return $"{before}<mark class='bg-warning'>{match}</mark>{after}";
         }
         return text;
      }      private async Task LoadData()
      {
         Console.WriteLine($"=== LoadData called with GlobalSearchTerm: '{GlobalSearchTerm}' ===");
         try
         {
            _isLoading = true;
            StateHasChanged();

            if (CustomIntelliSenseDataService != null)
            {
               if (LanguageId == 0) LanguageId = 8;
               if (CategoryId == 0) CategoryId = 34;

               // Try to get the complete state from cache
               var cacheKey = GetCacheKey();
               var filterCacheKey = GetFilterCacheKey();

               // Get the main data (language, category, and items)
               var cachedState = await Cache.GetOrSetAsync(cacheKey, async () =>
               {
                  // Load language and category data in parallel
                  var languageTask = LanguageDataService.GetLanguageById(LanguageId);
                  var categoryTask = CategoryDataService.GetCategoryById(CategoryId);                  // Load the data page
                  var dataTask = string.IsNullOrWhiteSpace(GlobalSearchTerm)
                      ? CustomIntelliSenseDataService.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, pageNumber, pageSize)
                      : CustomIntelliSenseDataService.SearchCustomIntelliSensesAsync(GlobalSearchTerm); // Global search without language/category restrictions

                  await Task.WhenAll(languageTask, categoryTask, dataTask);

                  return new
                  {
                     Language = await languageTask,
                     Category = await categoryTask,
                     Data = await dataTask
                  };
               });

               if (cachedState?.Data == null)
               {
                  // Invalidate cache if we got null data
                  await InvalidateCache();
                  throw new InvalidOperationException("Cache returned null data");
               }

               currentLanguage = cachedState.Language;
               currentCategory = cachedState.Category;
               // Ensure the autocomplete inputs reflect the currently loaded language/category
               if (currentLanguage != null)
               {
                  SelectedLanguageId = currentLanguage.Id;
                  LanguageQuery = currentLanguage.LanguageName ?? LanguageQuery;
               }
               if (currentCategory != null)
               {
                  SelectedCategoryId = currentCategory.Id;
                  CategoryQuery = currentCategory.CategoryName ?? CategoryQuery;
               }
               CustomIntelliSenseDTO = cachedState.Data;

               // Now apply filter and cache the filtered result
               var filteredResult = await Cache.GetOrSetAsync(filterCacheKey, () =>
               {
                  if (CustomIntelliSenseDTO == null)
                  {
                     return Task.FromResult(new List<CustomIntelliSenseDTO>());
                  }
                  if (string.IsNullOrEmpty(SearchTerm))
                  {
                     // No additional filtering needed since we're already paginated from the server
                     return Task.FromResult(CustomIntelliSenseDTO);
                  }
                  else
                  {
                     var temporary = SearchTerm.ToLower().Trim();
                     var filtered = CustomIntelliSenseDTO
                         .Where(v =>
                             v.DisplayValue != null && v.DisplayValue.ToLower().Contains(temporary) ||
                             v.SendKeysValue != null && v.SendKeysValue.ToLower().Contains(temporary) ||
                             v.CommandType != null && v.CommandType.ToLower().Contains(temporary) ||
                             v.DeliveryType != null && v.DeliveryType.ToLower().Contains(temporary)
                         )
                         .ToList();
                     return Task.FromResult(filtered);
                  }
               });

               if (filteredResult != null)
               {
                  FilteredCustomIntelliSenseDTO = filteredResult;
                  // Get total count from first item if available
                  var totalCount = CustomIntelliSenseDTO?.FirstOrDefault()?.TotalCount ?? 0;
                  Title = $"({FilteredCustomIntelliSenseDTO.Count}) of {totalCount}";

                  // Start prefetching next page
                  PrefetchNextPage();
               }
            }
         }
         catch (Exception e)
         {
            Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
            _loadFailed = true;
            ExceptionMessage = e.Message;
         }
         finally
         {
            _isLoading = false;
            StateHasChanged();
         }
      }

      private async Task NextPageAsync()
      {
         // Cancel any prefetch task since we're actually moving to next page
         if (_prefetchTask != null && !_prefetchTask.IsCompleted)
         {
            // Wait for prefetch to complete since it might have our data
            try
            {
               await _prefetchTask;
            }
            catch
            {
               // Ignore prefetch errors
            }
         }

         pageNumber++;
         searchTerm = null; // Reset search when changing pages
         await LoadData();
         StateHasChanged();
      }

      private async Task PreviousPageAsync()
      {
         pageNumber--;
         if (pageNumber < 1)
         {
            pageNumber = 1;
            return;
         }

         searchTerm = null; // Reset search when changing pages
         await LoadData();
         StateHasChanged();
      }

      public void Dispose()
      {
         _searchCancellation?.Cancel();
         _searchCancellation?.Dispose();
      }
   }
}