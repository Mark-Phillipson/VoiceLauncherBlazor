
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
using DataAccessLibrary.DTO;
using System.Security.Claims;
using SampleApplication.Services;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class PromptAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<PromptAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public PromptDTO PromptDTO { get; set; } = new PromptDTO();//{ };
        [Inject] public IPromptDataService? PromptDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (PromptDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await PromptDataService.GetPromptById((int)Id);
                if (result != null)
                {
                    PromptDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "PromptText");
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
            if ((Id == 0 || Id == null) && PromptDataService != null)
            {
                PromptDTO? result = await PromptDataService.AddPrompt(PromptDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Prompt failed to add, please investigate Error Adding New Prompt");
                    ToastService?.ShowError("Prompt failed to add, please investigate Error Adding New Prompt");
                    return;
                }
                ToastService?.ShowSuccess("Prompt added successfully");
            }
            else
            {
                if (PromptDataService != null)
                {
                    await PromptDataService!.UpdatePrompt(PromptDTO, "");
                    ToastService?.ShowSuccess("The Prompt updated successfully");
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