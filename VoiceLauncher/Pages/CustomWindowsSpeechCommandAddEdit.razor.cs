
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
using WindowsInput.Native;

namespace VoiceLauncher.Pages
{
    public partial class CustomWindowsSpeechCommandAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Parameter] public int? WindowsSpeechVoiceCommandId { get; set; }
        public CustomWindowsSpeechCommandDTO CustomWindowsSpeechCommandDTO { get; set; } = new CustomWindowsSpeechCommandDTO();
        [Inject] public ICustomWindowsSpeechCommandDataService? CustomWindowsSpeechCommandDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }

#pragma warning disable 414, 649
        string TaskRunning = "";
        private string? _filter = "";
        public string? KeyPressFilter
        {
            get => _filter; set
            {
                _filter = value;
                if (_filter != null)
                {
                    FilterKeyPressValue(_filter);
                }
            }
        }
        private string? _keyDownFilter = "";
        public string? KeyDownFilter
        {
            get => _keyDownFilter; set
            {
                _keyDownFilter = value;
                if (_keyDownFilter != null)
                {
                    FilterKeyDownValue(_keyDownFilter);
                }
            }
        }
#pragma warning disable 414, 649

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
            else if (WindowsSpeechVoiceCommandId != null)
            {
                CustomWindowsSpeechCommandDTO.WindowsSpeechVoiceCommandId = (int)WindowsSpeechVoiceCommandId;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "TextToEnter");
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
                    await CustomWindowsSpeechCommandDataService!.UpdateCustomWindowsSpeechCommand(CustomWindowsSpeechCommandDTO, "");
                    ToastService?.ShowSuccess("The Custom Windows Speech Command updated successfully", "SUCCESS");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = "";
        }
        private void FilterKeyPressValue(string filter)
        {
            if (filter.ToLower() == "a")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_A;
            }
            else if (filter.ToLower() == "b")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_B;
            }
            else if (filter.ToLower() == "c")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_C;
            }
            else if (filter.ToLower() == "d")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_D;
            }
            else if (filter.ToLower() == "e")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_E;
            }
            else if (filter.ToLower() == "f")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_F;
            }
            else if (filter.ToLower() == "g")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_C;
            }
            else if (filter.ToLower() == "h")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_H;
            }
            else if (filter.ToLower() == "i")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_I;
            }
            else if (filter.ToLower() == "j")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_J;
            }
            else if (filter.ToLower() == "k")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_K;
            }
            else if (filter.ToLower() == "l")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_L;
            }
            else if (filter.ToLower() == "m")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_M;
            }
            else if (filter.ToLower() == "n")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_N;
            }
            else if (filter.ToLower() == "o")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_O;
            }
            else if (filter.ToLower() == "p")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_P;
            }
            else if (filter.ToLower() == "q")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_Q;
            }
            else if (filter.ToLower() == "r")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_R;
            }
            else if (filter.ToLower() == "s")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_S;
            }
            else if (filter.ToLower() == "t")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_T;
            }
            else if (filter.ToLower() == "u")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_U;
            }
            else if (filter.ToLower() == "v")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_V;
            }
            else if (filter.ToLower() == "w")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_W;
            }
            else if (filter.ToLower() == "x")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_X;
            }
            else if (filter.ToLower() == "y")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_Y;
            }
            else if (filter.ToLower() == "z")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_Z;
            }
            else if (filter.ToLower() == "0")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_0;
            }
            else if (filter.ToLower() == "1")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_1;
            }
            else if (filter.ToLower() == "2")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_2;
            }
            else if (filter.ToLower() == "3")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_3;
            }
            else if (filter.ToLower() == "4")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_4;
            }
            else if (filter.ToLower() == "5")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_5;
            }
            else if (filter.ToLower() == "6")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_6;
            }
            else if (filter.ToLower() == "7")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_7;
            }
            else if (filter.ToLower() == "8")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_8;
            }
            else if (filter.ToLower() == "9")
            {
                CustomWindowsSpeechCommandDTO.KeyPressValue = VirtualKeyCode.VK_9;
            }

        }
        private void FilterKeyDownValue(string filter)
        {
            if (filter.ToLower() == "a")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_A;
            }
            else if (filter.ToLower() == "b")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_B;
            }
            else if (filter.ToLower() == "c")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_C;
            }
            else if (filter.ToLower() == "d")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_D;
            }
            else if (filter.ToLower() == "e")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_E;
            }
            else if (filter.ToLower() == "f")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_F;
            }
            else if (filter.ToLower() == "g")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_C;
            }
            else if (filter.ToLower() == "h")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_H;
            }
            else if (filter.ToLower() == "i")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_I;
            }
            else if (filter.ToLower() == "j")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_J;
            }
            else if (filter.ToLower() == "k")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_K;
            }
            else if (filter.ToLower() == "l")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_L;
            }
            else if (filter.ToLower() == "m")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_M;
            }
            else if (filter.ToLower() == "n")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_N;
            }
            else if (filter.ToLower() == "o")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_O;
            }
            else if (filter.ToLower() == "p")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_P;
            }
            else if (filter.ToLower() == "q")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_Q;
            }
            else if (filter.ToLower() == "r")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_R;
            }
            else if (filter.ToLower() == "s")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_S;
            }
            else if (filter.ToLower() == "t")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_T;
            }
            else if (filter.ToLower() == "u")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_U;
            }
            else if (filter.ToLower() == "v")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_V;
            }
            else if (filter.ToLower() == "w")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_W;
            }
            else if (filter.ToLower() == "x")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_X;
            }
            else if (filter.ToLower() == "y")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_Y;
            }
            else if (filter.ToLower() == "z")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_Z;
            }
            else if (filter.ToLower() == "0")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_0;
            }
            else if (filter.ToLower() == "1")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_1;
            }
            else if (filter.ToLower() == "2")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_2;
            }
            else if (filter.ToLower() == "3")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_3;
            }
            else if (filter.ToLower() == "4")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_4;
            }
            else if (filter.ToLower() == "5")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_5;
            }
            else if (filter.ToLower() == "6")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_6;
            }
            else if (filter.ToLower() == "7")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_7;
            }
            else if (filter.ToLower() == "8")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_8;
            }
            else if (filter.ToLower() == "9")
            {
                CustomWindowsSpeechCommandDTO.KeyDownValue = VirtualKeyCode.VK_9;
            }

        }
    }
}