using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using VoiceLauncher.Services;

namespace VoiceLauncher.Pages
{
    public partial class CustomIntelliSenseAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<CustomIntelliSenseAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Parameter] public int CategoryID { get; set; }
        public CustomIntelliSenseDTO CustomIntelliSenseDTO { get; set; } = new CustomIntelliSenseDTO();//{ };
        [Inject] public required ICategoryDataService CategoryDataService { get; set; }
        private List<CategoryDTO> categories { get; set; } = new List<CategoryDTO>();
        [Inject] public required LanguageService LanguageService { get; set; }
        private List<DataAccessLibrary.Models.Language> languages { get; set; } = new List<DataAccessLibrary.Models.Language>();
        [Inject] public ICustomIntelliSenseDataService? CustomIntelliSenseDataService { get; set; }
        private string variable1 { get; set; } = "";
        private string variable2 { get; set; } = "";
        private string variable3 { get; set; } = "";
        private List<DataAccessLibrary.Models.GeneralLookup>? generalLookups { get; set; }
        [Inject] public required GeneralLookupService GeneralLookupDataService { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (CustomIntelliSenseDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await CustomIntelliSenseDataService.GetCustomIntelliSenseById((int)Id);
                if (result != null)
                {
                    CustomIntelliSenseDTO = result;
                }
            }
            else
            {

                if (CategoryID > 0)
                {
                    CustomIntelliSenseDTO.CategoryId = CategoryID;
                }
                CustomIntelliSenseDTO.CommandType = "SendKeys";
            }
            categories = await CategoryDataService.GetAllCategoriesAsync("IntelliSense Command", 0);
            languages = await LanguageService.GetLanguagesAsync();
            generalLookups = await GeneralLookupDataService.GetGeneralLookUpsAsync("Delivery Type");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime != null)
                    {
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "DisplayValue");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        public async Task CloseAsync()
        {
            if (ModalInstance != null)
                await ModalInstance.CancelAsync();
        }
        protected async Task HandleValidSubmit()
        {
            TaskRunning = true;
            if ((Id == 0 || Id == null) && CustomIntelliSenseDataService != null)
            {
                CustomIntelliSenseDTO? result = await CustomIntelliSenseDataService.AddCustomIntelliSense(CustomIntelliSenseDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Custom Intelli Sense failed to add, please investigate Error Adding New Custom Intelli Sense");
                    ToastService?.ShowError("Custom Intelli Sense failed to add, please investigate Error Adding New Custom Intelli Sense");
                    return;
                }
                ToastService?.ShowSuccess("Custom Intelli Sense added successfully", "SUCCESS");
            }
            else
            {
                if (CustomIntelliSenseDataService != null)
                {
                    await CustomIntelliSenseDataService!.UpdateCustomIntelliSense(CustomIntelliSenseDTO, "");
                    ToastService?.ShowSuccess("The Custom Intelli Sense updated successfully", "SUCCESS");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = false;
        }
        private async Task CopyItemAsync(string itemToCopy)
        {
            if (string.IsNullOrEmpty(itemToCopy)) { return; }
            if (itemToCopy.Contains("`Variable1`") && variable1.Length> 0) {
                itemToCopy = itemToCopy.Replace("`Variable1`", variable1);
            }
            if (itemToCopy.Contains("`Variable2`") && variable2.Length> 0)
            {
                itemToCopy = itemToCopy.Replace("`Variable2`", variable2);
            }
            if (itemToCopy.Contains("`Variable3`") && variable3.Length> 0)
            {
                itemToCopy = itemToCopy.Replace("`Variable3`", variable3);
            }
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", itemToCopy);
                var message = $"Copied Successfully: '{itemToCopy}'";
                ToastService!.ShowSuccess(message, "Copy Item");
            }
        }
    }
}