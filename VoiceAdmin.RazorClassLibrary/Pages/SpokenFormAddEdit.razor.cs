using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace RazorClassLibrary.Pages
{
    public partial class SpokenFormAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<SpokenFormAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Parameter] public int WindowsSpeechVoiceCommandId { get; set; }
        public SpokenFormDTO SpokenFormDTO { get; set; } = new SpokenFormDTO();//{ };
        [Inject] public ISpokenFormDataService? SpokenFormDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (SpokenFormDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await SpokenFormDataService.GetSpokenFormById((int)Id);
                if (result != null)
                {
                    SpokenFormDTO = result;
                }
            }
            else
            {

                SpokenFormDTO.WindowsSpeechVoiceCommandId = WindowsSpeechVoiceCommandId;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "SpokenFormText");
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
            if ((Id == 0 || Id == null) && SpokenFormDataService != null)
            {
                SpokenFormDTO? result = await SpokenFormDataService.AddSpokenForm(SpokenFormDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Spoken Form failed to add, please investigate Error Adding New Spoken Form");
                    ToastService?.ShowError("Spoken Form failed to add, please investigate Error Adding New Spoken Form");
                    return;
                }
                ToastService?.ShowSuccess("Spoken Form added successfully");
            }
            else
            {
                if (SpokenFormDataService != null)
                {
                    await SpokenFormDataService!.UpdateSpokenForm(SpokenFormDTO, "");
                    ToastService?.ShowSuccess("The Spoken Form updated successfully");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = false;
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