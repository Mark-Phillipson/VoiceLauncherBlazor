using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.Services;
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
        [Inject]
        private ILauncherRepository LauncherRepository { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        string[] filenameList = new string[0];
        List<string> imageUlrs = new List<string>();
        // New property to track icon input mode
        public bool UseCustomIconUrl { get; set; } = false;
        private void SetIconInputMode(bool useCustom)
        {
            UseCustomIconUrl = useCustom;
            if (!useCustom && imageUlrs.Count > 0 && !imageUlrs.Contains(LauncherDTO.Icon))
            {
                LauncherDTO.Icon = imageUlrs.First();
            }
        }
        private void LoadImages()
        {
            string directoryPath = @"C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceLauncher\wwwroot\images";
            filenameList = Directory.GetFiles(directoryPath);
            foreach (string item in filenameList)
            {
                string imageUrl = item.Replace(@"C:\Users\MPhil\source\repos\VoiceLauncherBlazor\VoiceLauncher\wwwroot\images\", "");
                imageUlrs.Add(imageUrl);
            }
        }

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
            LoadImages();

            if (LauncherDTO.Id > 0)
            {
                // Load existing category associations
                var categoryIds = await LauncherRepository.GetCategoryIdsForLauncherAsync(LauncherDTO.Id);
                SelectedCategoryIds = new HashSet<int>(categoryIds);

                // Keep the primary category if it exists
                if (LauncherDTO.CategoryId > 0 && !SelectedCategoryIds.Contains(LauncherDTO.CategoryId))
                {
                    SelectedCategoryIds.Add(LauncherDTO.CategoryId);
                }
            }
            else if (LauncherDTO.CategoryId > 0)
            {
                // For new launcher with default category
                SelectedCategoryIds.Add(LauncherDTO.CategoryId);
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

        protected void CategoryCheckboxChanged(int categoryId, object checkedValue)
        {
            if (checkedValue is bool isChecked)
            {
                if (isChecked)
                {
                    SelectedCategoryIds.Add(categoryId);

                    // If no primary category is set, use this as primary
                    if (LauncherDTO.CategoryId <= 0)
                    {
                        LauncherDTO.CategoryId = categoryId;
                    }
                }
                else
                {
                    SelectedCategoryIds.Remove(categoryId);

                    // If removing the primary category, select another one as primary if available
                    if (LauncherDTO.CategoryId == categoryId && SelectedCategoryIds.Any())
                    {
                        LauncherDTO.CategoryId = SelectedCategoryIds.First();
                    }
                }
            }
        }

        protected async Task HandleValidSubmit()
        {
            // Validate that at least one category is selected
            if (!SelectedCategoryIds.Any())
            {
                // Show validation error
                return;
            }

            // Make sure primary category is in selected categories
            if (LauncherDTO.CategoryId <= 0 && SelectedCategoryIds.Any())
            {
                LauncherDTO.CategoryId = SelectedCategoryIds.First();
            }

            // Save launcher first
            LauncherDTO savedLauncher;
            if (LauncherDTO.Id > 0)
            {
                savedLauncher = await LauncherRepository.UpdateLauncherAsync(LauncherDTO);
            }
            else
            {
                savedLauncher = await LauncherRepository.AddLauncherAsync(LauncherDTO);
            }

            if (savedLauncher != null)
            {
                // Update category associations
                await LauncherRepository.UpdateLauncherCategoriesAsync(savedLauncher.Id, SelectedCategoryIds);

                ToastService?.ShowSuccess("Launcher saved successfully");
            }
            else
            {
                ToastService?.ShowError("Launcher failed to save, please investigate Error Adding New Launcher");
            }

            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = "";
        }

        // Add this property to store selected category IDs
        protected HashSet<int> SelectedCategoryIds { get; set; } = new HashSet<int>();
    }
}