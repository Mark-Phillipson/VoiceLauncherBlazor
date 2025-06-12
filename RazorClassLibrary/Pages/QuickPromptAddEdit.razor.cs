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
using DataAccessLibrary.Services;
using DataAccessLibrary.DTO;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class QuickPromptAddEdit : ComponentBase
    {
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<QuickPromptAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public QuickPromptDTO QuickPromptDTO { get; set; } = new QuickPromptDTO();
        [Inject] public IQuickPromptDataService? QuickPromptDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
        
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649

        protected override async Task OnInitializedAsync()
        {
            if (QuickPromptDataService == null)
            {
                return;
            }
            
            if (Id > 0)
            {
                var result = await QuickPromptDataService.GetQuickPromptById((int)Id);
                if (result != null)
                {
                    QuickPromptDTO = result;
                }
            }
            else
            {
                // Set defaults for new prompt
                QuickPromptDTO.IsActive = true;
                QuickPromptDTO.CreatedDate = DateTime.UtcNow;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "Type");
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
            
            if ((Id == 0 || Id == null) && QuickPromptDataService != null)
            {
                QuickPromptDTO? result = await QuickPromptDataService.AddQuickPrompt(QuickPromptDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Quick Prompt failed to add, please investigate Error Adding New Quick Prompt");
                    TaskRunning = "";
                    return;
                }
                ToastService?.ShowSuccess("Quick Prompt added successfully");
            }
            else
            {
                if (QuickPromptDataService != null)
                {
                    QuickPromptDTO.LastModifiedDate = DateTime.UtcNow;
                    await QuickPromptDataService.UpdateQuickPrompt(QuickPromptDTO, "");
                    ToastService?.ShowSuccess("The Quick Prompt updated successfully");
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
