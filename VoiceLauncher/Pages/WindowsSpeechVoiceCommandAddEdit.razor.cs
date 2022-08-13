
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
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

namespace VoiceLauncher.Pages
{
    public partial class WindowsSpeechVoiceCommandAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public WindowsSpeechVoiceCommandDTO WindowsSpeechVoiceCommandDTO { get; set; } = new WindowsSpeechVoiceCommandDTO();//{ };
        [Inject] public IWindowsSpeechVoiceCommandDataService? WindowsSpeechVoiceCommandDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (WindowsSpeechVoiceCommandDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await WindowsSpeechVoiceCommandDataService.GetWindowsSpeechVoiceCommandById((int)Id);
                if (result != null)
                {
                    WindowsSpeechVoiceCommandDTO = result;
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
            if ((Id == 0 || Id == null) && WindowsSpeechVoiceCommandDataService != null)
            {
                WindowsSpeechVoiceCommandDTO? result = await WindowsSpeechVoiceCommandDataService.AddWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Windows Speech Voice Command failed to add, please investigate", "Error Adding New Windows Speech Voice Command");
                    return;
                }
                ToastService?.ShowSuccess("Windows Speech Voice Command added successfully", "SUCCESS");
            }
            else
            {
                if (WindowsSpeechVoiceCommandDataService != null)
                {
                    await WindowsSpeechVoiceCommandDataService!.UpdateWindowsSpeechVoiceCommand(WindowsSpeechVoiceCommandDTO, "TBC");
                    ToastService?.ShowSuccess("The Windows Speech Voice Command updated successfully", "SUCCESS");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = "";
        }
        private async Task CallChangeAsync(string elementId)
        {
            if (JSRuntime != null)
            {
                await JSRuntime.InvokeVoidAsync("CallChange", elementId);
            }
        }
    }
}