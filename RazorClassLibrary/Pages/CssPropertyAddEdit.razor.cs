
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using System.Security.Claims;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Services;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class CssPropertyAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<CssPropertyAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public CssPropertyDTO CssPropertyDTO { get; set; } = new CssPropertyDTO();//{ };
        [Inject] public ICssPropertyDataService? CssPropertyDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
        ElementReference FirstInput;
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (CssPropertyDataService == null)
            {
                return;
            }
            if (Id != null && Id != 0)
            {
                var result = await CssPropertyDataService.GetCssPropertyById((int)Id);
                if (result != null)
                {
                    CssPropertyDTO = result;
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
                    await Task.Delay(100);
                    await FirstInput.FocusAsync();
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
            if ((Id == 0 || Id == null) && CssPropertyDataService != null)
            {
                CssPropertyDTO? result = await CssPropertyDataService.AddCssProperty(CssPropertyDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Css Property failed to add, please investigate Error Adding New Css Property");
                    ToastService?.ShowError("Css Property failed to add, please investigate Error Adding New Css Property");
                    return;
                }
                ToastService?.ShowSuccess("Css Property added successfully");
            }
            else
            {
                if (CssPropertyDataService != null)
                {
                    await CssPropertyDataService!.UpdateCssProperty(CssPropertyDTO, "");
                    ToastService?.ShowSuccess("The Css Property updated successfully");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = false;
        }
    }
}