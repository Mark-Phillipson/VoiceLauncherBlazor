
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using System.Security.Claims;
using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class CategoryAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public CategoryDTO CategoryDTO { get; set; } = new CategoryDTO();//{ };
        [Inject] public ICategoryDataService? CategoryDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (CategoryDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await CategoryDataService.GetCategoryById((int)Id);
                if (result != null)
                {
                    CategoryDTO = result;
                }
            }
            else
            {
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "Category");
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
            if ((Id == 0 || Id == null) && CategoryDataService != null)
            {
                CategoryDTO? result = await CategoryDataService.AddCategory(CategoryDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Category failed to add, please investigate Error Adding New Category");
                    return;
                }
                ToastService?.ShowSuccess("Category added successfully");
            }
            else
            {
                if (CategoryDataService != null)
                {
                    await CategoryDataService!.UpdateCategory(CategoryDTO, "");
                    ToastService?.ShowSuccess("The Category updated successfully");
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