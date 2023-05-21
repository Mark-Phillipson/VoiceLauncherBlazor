using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class MicrophoneAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<MicrophoneAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public MicrophoneDTO MicrophoneDTO { get; set; } = new MicrophoneDTO();//{ };
        [Inject] public IMicrophoneDataService? MicrophoneDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (MicrophoneDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await MicrophoneDataService.GetMicrophoneById((int)Id);
                if (result != null)
                {
                    MicrophoneDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "MicrophoneName");
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
            if ((Id == 0 || Id == null) && MicrophoneDataService != null)
            {
                MicrophoneDTO? result = await MicrophoneDataService.AddMicrophone(MicrophoneDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Microphone failed to add, please investigate Error Adding New Microphone");
                    ToastService?.ShowError("Microphone failed to add, please investigate Error Adding New Microphone");
                    return;
                }
                ToastService?.ShowSuccess("Microphone added successfully", "SUCCESS");
            }
            else
            {
                if (MicrophoneDataService != null)
                {
                    await MicrophoneDataService!.UpdateMicrophone(MicrophoneDTO, "");
                    ToastService?.ShowSuccess("The Microphone updated successfully", "SUCCESS");
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