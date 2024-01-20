using Ardalis.GuardClauses;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;
using System.Security.Claims;
using VoiceLauncher.Services;
using WindowsInput.Native;
using WindowsInput;

namespace RazorClassLibrary.Pages
{
   public partial class CustomIntelliSenseTable : ComponentBase
   {
      [Inject] public ICustomIntelliSenseDataService? CustomIntelliSenseDataService { get; set; }
      [Inject] public NavigationManager? NavigationManager { get; set; }
      [Inject] public ILogger<CustomIntelliSenseTable>? Logger { get; set; }
      [Inject] public required ILanguageDataService LanguageDataService { get; set; }
      [Inject] public required ICategoryDataService CategoryDataService { get; set; }
      [Inject] public IToastService? ToastService { get; set; }
      [CascadingParameter] public IModalService? Modal { get; set; }
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
      public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
      public string ExceptionMessage { get; set; } = string.Empty;
      public List<string>? PropertyInfo { get; set; }
      [CascadingParameter] public ClaimsPrincipal? User { get; set; }
      [Inject] public IJSRuntime? JSRuntime { get; set; }
      public bool ShowEdit { get; set; } = false;
      private bool ShowDeleteConfirm { get; set; }
      private int CustomIntelliSenseId { get; set; }
      private int pageNumber = 1;
      private int pageSize = 10;
      private int counter = 0;
      private int shortcutValue = 0;
      protected override async Task OnInitializedAsync()
      {
         await LoadData();
      }
        protected override  async Task OnParametersSetAsync()
        {
             await LoadData();
        }
        private async Task LoadData()
      {
         try
         {
            if (CustomIntelliSenseDataService != null)
            {
               if (LanguageId == 0)
               {
                  LanguageId = 8;
               }
               if (CategoryId == 0)
               {
                  CategoryId = 34;
               }
               currentLanguage = await LanguageDataService.GetLanguageById(LanguageId);
               currentCategory = await CategoryDataService.GetCategoryById(CategoryId);
               //Not paging here, just getting all records
               List<CustomIntelliSenseDTO> result;
               if (string.IsNullOrWhiteSpace(GlobalSearchTerm))
               {
                  result = await CustomIntelliSenseDataService!.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, pageNumber, pageSize);
               }
               else
               {
                  result = await CustomIntelliSenseDataService.SearchCustomIntelliSensesAsync(GlobalSearchTerm);
                  pageSize = result.Count;
               }
               if (result != null)
               {
                  CustomIntelliSenseDTO = result.ToList();
                  //Paging Here instead
                  FilteredCustomIntelliSenseDTO = result.Skip((pageNumber - 1) * pageSize)
                  .Take(pageSize).ToList();
                  StateHasChanged();
               }
            }
         }
         catch (Exception e)
         {
            Logger?.LogError(e, "Exception occurred in LoadData Method, Getting Records from the Service");
            _loadFailed = true;
            ExceptionMessage = e.Message;
         }
         Title = $"Snippets ({FilteredCustomIntelliSenseDTO?.Count}) of {CustomIntelliSenseDTO?.Count}";

      }
      private async Task NextPageAsync()
      {
         pageNumber = pageNumber + 1;
         await LoadData();
      }
      private async Task PreviousPageAsync()
      {
         pageNumber = pageNumber - 1;
         if (pageNumber < 1)
         {
            pageNumber = 1;
         }
         await LoadData();
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
               await LoadData();
            }
         }
         CustomIntelliSenseId = 0;
      }


      private void ApplyFilter()
      {
         if (FilteredCustomIntelliSenseDTO == null || CustomIntelliSenseDTO == null)
         {
            return;
         }
         if (string.IsNullOrEmpty(SearchTerm))
         {
            FilteredCustomIntelliSenseDTO = CustomIntelliSenseDTO.OrderBy(v => v.DisplayValue)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();
            Title = $"Snippets ({FilteredCustomIntelliSenseDTO.Count}) of {CustomIntelliSenseDTO.Count}";
         }
         else
         {
            var temporary = SearchTerm.ToLower().Trim();
            FilteredCustomIntelliSenseDTO = CustomIntelliSenseDTO
                .Where(v =>
                v.DisplayValue != null && v.DisplayValue.ToLower().Contains(temporary)
                 || v.SendKeysValue != null && v.SendKeysValue.ToLower().Contains(temporary)
                 || v.CommandType != null && v.CommandType.ToLower().Contains(temporary)
                 || v.DeliveryType != null && v.DeliveryType.ToLower().Contains(temporary)
                )
                .ToList();
            Title = $"Filtered Custom Intelli Senses ({FilteredCustomIntelliSenseDTO.Count})";
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
                  await LoadData();
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
               await LoadData();
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
         }
         if (JSRuntime != null && customIntelliSenseCurrent != null)
         {
            await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", customIntelliSenseCurrent.SendKeysValue);
            var message = $"Copied Successfully: '{customIntelliSenseCurrent.SendKeysValue}'";
            InputSimulator simulator = new InputSimulator();
            simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
            simulator.Keyboard.Sleep(100);
            simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            simulator.Keyboard.Sleep(100);
            simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            ToastService!.ShowSuccess(message);
            if (RunningInBlazorHybrid)
            {
               await CloseApplication.InvokeAsync();
            }
         }
      }
   }
}