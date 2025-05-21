using Ardalis.GuardClauses;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;
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
         // Load data only once during initialization
         await LoadData();
      }
      private string GetCacheKey(string suffix = "", int? pageNum = null) => 
          $"CustomIntelliSenseTable_{LanguageId}_{CategoryId}_{pageNum ?? pageNumber}_{pageSize}_{GlobalSearchTerm}{suffix}";

      private string GetFilterCacheKey() =>
          $"CustomIntelliSenseTable_Filter_{LanguageId}_{CategoryId}_{pageNumber}_{pageSize}_{GlobalSearchTerm}_{SearchTerm}";

      private async Task InvalidateCache()
      {
         Cache.InvalidateCache($"CustomIntelliSenseTable_{LanguageId}_{CategoryId}");
         await LoadData();
      }

      private async Task PrefetchNextPage()
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
                  await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
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
      }      private async Task ApplyFilter()
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

             var filterCacheKey = GetFilterCacheKey();
             FilteredCustomIntelliSenseDTO = await Cache.GetOrSetAsync(filterCacheKey, () =>
             {
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

             StateHasChanged();
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
            var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Custom Intelli Sense ({customIntelliSense?.DisplayValue})?", parameters);
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

      private async Task LoadData()
      {
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

               // First try to get filtered results from cache
               var filteredResult = await Cache.GetOrSetAsync(filterCacheKey, async () =>
               {
                  try 
                  {
                     // Get the main data
                     var cachedState = await Cache.GetOrSetAsync(cacheKey, async () =>
                     {
                        // Load language and category data in parallel
                        var languageTask = LanguageDataService.GetLanguageById(LanguageId);
                        var categoryTask = CategoryDataService.GetCategoryById(CategoryId);
                        
                        // Load the data page
                        var dataTask = string.IsNullOrWhiteSpace(GlobalSearchTerm)
                            ? CustomIntelliSenseDataService.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, pageNumber, pageSize)
                            : CustomIntelliSenseDataService.SearchCustomIntelliSensesAsync(GlobalSearchTerm);

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
                     CustomIntelliSenseDTO = cachedState.Data;

                     // Apply filter and cache the result
                     ApplyFilter();
                     return FilteredCustomIntelliSenseDTO;
                  }
                  catch
                  {
                     // If anything fails, invalidate cache and try direct fetch
                     await InvalidateCache();
                     throw;
                  }
               });

               if (filteredResult != null)
               {
                  FilteredCustomIntelliSenseDTO = filteredResult;
                  // Get total count from first item if available
                  var totalCount = CustomIntelliSenseDTO?.FirstOrDefault()?.TotalCount ?? 0;
                  Title = $"Snippets ({FilteredCustomIntelliSenseDTO.Count}) of {totalCount}";

                  // Start prefetching next page
                  _ = PrefetchNextPage();
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