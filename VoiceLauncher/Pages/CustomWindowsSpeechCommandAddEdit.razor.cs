
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
using VoiceLauncher.Services;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Pages
{
    public partial class CustomWindowsSpeechCommandAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public CustomWindowsSpeechCommandDTO CustomWindowsSpeechCommandDTO { get; set; } = new CustomWindowsSpeechCommandDTO();//{ };
        [Inject] public ICustomWindowsSpeechCommandDataService? CustomWindowsSpeechCommandDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        private List<string> MouseMethods = new List<string>() { "LeftButtonDown", "RightButtonDown", "LeftButtonDoubleClick", "RightButtonDoubleClick", "LeftButtonUp", "RightButtonUp", "MiddleButtonClick", "MiddleButtonDoubleClick", "HorizontalScroll", "VerticalScroll", "MoveMouseBy", "MoveMouseTo" };
        protected override async Task OnInitializedAsync()
        {
            if (CustomWindowsSpeechCommandDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await CustomWindowsSpeechCommandDataService.GetCustomWindowsSpeechCommandById((int)Id);
                if (result != null)
                {
                    CustomWindowsSpeechCommandDTO = result;
                }
            }
            else
            {
            }
        }
        private async Task CallChangeAsync(string elementId)
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("CallChange", elementId);
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "SpokenCommand");
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
            if ((Id == 0 || Id == null) && CustomWindowsSpeechCommandDataService != null)
            {
                CustomWindowsSpeechCommandDTO? result = await CustomWindowsSpeechCommandDataService.AddCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Custom Windows Speech Command failed to add, please investigate", "Error Adding New Custom Windows Speech Command");
                    return;
                }
                ToastService?.ShowSuccess("Custom Windows Speech Command added successfully", "SUCCESS");
            }
            else
            {
                if (CustomWindowsSpeechCommandDataService != null)
                {
                    await CustomWindowsSpeechCommandDataService!.UpdateCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO, "TBC");
                    ToastService?.ShowSuccess("The Custom Windows Speech Command updated successfully", "SUCCESS");
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