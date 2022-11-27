
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
using VoiceLauncher.DTOs;
using VoiceLauncher.Services;

namespace VoiceLauncher.Pages
{
    public partial class ApplicationDetailAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public ApplicationDetailDTO ApplicationDetailDTO { get; set; } = new ApplicationDetailDTO();//{ };
        [Inject] public IApplicationDetailDataService? ApplicationDetailDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (ApplicationDetailDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await ApplicationDetailDataService.GetApplicationDetailById((int)Id);
                if (result != null)
                {
                    ApplicationDetailDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "ProcessName");
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
            if ((Id == 0 || Id == null) && ApplicationDetailDataService != null)
            {
                ApplicationDetailDTO? result = await ApplicationDetailDataService.AddApplicationDetail(ApplicationDetailDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Application Detail failed to add, please investigate", "Error Adding New Application Detail");
                    return;
                }
                ToastService?.ShowSuccess("Application Detail added successfully", "SUCCESS");
            }
            else
            {
                if (ApplicationDetailDataService != null)
                {
                    await ApplicationDetailDataService!.UpdateApplicationDetail(ApplicationDetailDTO, "");
                    ToastService?.ShowSuccess("The Application Detail updated successfully", "SUCCESS");
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