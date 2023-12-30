using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class LauncherAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public ICategoryDataService? CategoryDataService { get; set; }
        private List<CategoryDTO> _categories = new List<CategoryDTO>();
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Parameter] public int CategoryID { get; set; }
        public LauncherDTO LauncherDTO { get; set; } = new LauncherDTO();//{ };
        [Inject] public ILauncherDataService? LauncherDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (LauncherDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await LauncherDataService.GetLauncherById((int)Id);
                if (result != null)
                {
                    LauncherDTO = result;
                }
            }
            else
            {
                LauncherDTO.CategoryId = CategoryID;
            }
            if (CategoryDataService != null)
            {
                _categories = await CategoryDataService.GetAllCategoriesAsync("Launch Applications", 0);
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "Name");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        public void Close()
        {
            if (ModalInstance != null)
                ModalInstance.CancelAsync();
        }

        protected async Task HandleValidSubmit()
        {
            TaskRunning = "disabled";
            if ((Id == 0 || Id == null) && LauncherDataService != null)
            {
                LauncherDTO? result = await LauncherDataService.AddLauncher(LauncherDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Launcher failed to add, please investigate Error Adding New Launcher");
                    return;
                }
                ToastService?.ShowSuccess("Launcher added successfully");
            }
            else
            {
                if (LauncherDataService != null)
                {
                    await LauncherDataService!.UpdateLauncher(LauncherDTO, "");
                    ToastService?.ShowSuccess("The Launcher updated successfully");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = "";
        }
    }
}