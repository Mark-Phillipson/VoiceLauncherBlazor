using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
    public partial class WindowsSpeechVoiceCommandAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public WindowsSpeechVoiceCommandDTO WindowsSpeechVoiceCommandDTO { get; set; } = new WindowsSpeechVoiceCommandDTO() { ApplicationName = "Global" };
        [Inject] public IWindowsSpeechVoiceCommandDataService? WindowsSpeechVoiceCommandDataService { get; set; }
        [Inject] public ISpokenFormDataService? SpokenFormDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        private List<ApplicationDetail>? ApplicationDetails { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (WindowsSpeechVoiceCommandDataService == null)
            {
                return;
            }
            ApplicationDetails = await WindowsSpeechVoiceCommandDataService.GetAllApplicationDetails();
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
                WindowsSpeechVoiceCommandDTO.ApplicationName = "Global";
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
                if (SpokenFormDataService != null)
                {
                    SpokenFormDTO spokenFormDTO = new SpokenFormDTO();
                    spokenFormDTO.SpokenFormText = result.SpokenCommand;
                    spokenFormDTO.WindowsSpeechVoiceCommandId = result.Id;
                    var spokenForResult = await SpokenFormDataService.AddSpokenForm(spokenFormDTO);
                    if (spokenForResult == null) { ToastService?.ShowError("Spoken Form failed to create"); return; }
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
        private void Control()
        {
            WindowsSpeechVoiceCommandDTO.SendKeysValue = WindowsSpeechVoiceCommandDTO.SendKeysValue + "^";
        }
        private void Alternate()
        {
            WindowsSpeechVoiceCommandDTO.SendKeysValue = WindowsSpeechVoiceCommandDTO.SendKeysValue + "%";
        }
        private void Shift()
        {
            WindowsSpeechVoiceCommandDTO.SendKeysValue = WindowsSpeechVoiceCommandDTO.SendKeysValue + "+";
        }
        private void Enter()
        {
            WindowsSpeechVoiceCommandDTO.SendKeysValue = WindowsSpeechVoiceCommandDTO.SendKeysValue + "{Enter}";
        }
        private void Dictation()
        {
            WindowsSpeechVoiceCommandDTO.SpokenCommand=WindowsSpeechVoiceCommandDTO.SpokenCommand + " <dictation>";
        }
        private void Clipboard()
        {
            WindowsSpeechVoiceCommandDTO.SpokenCommand = WindowsSpeechVoiceCommandDTO.SpokenCommand + " <clipboard>";
        }
    }

}