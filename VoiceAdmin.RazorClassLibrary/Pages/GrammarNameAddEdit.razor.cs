using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class GrammarNameAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public GrammarNameDTO GrammarNameDTO { get; set; } = new GrammarNameDTO();//{ };
        [Inject] public IGrammarNameDataService? GrammarNameDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (GrammarNameDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await GrammarNameDataService.GetGrammarNameById((int)Id);
                if (result != null)
                {
                    GrammarNameDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "NameOfGrammar");
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
            if ((Id == 0 || Id == null) && GrammarNameDataService != null)
            {
                GrammarNameDTO? result = await GrammarNameDataService.AddGrammarName(GrammarNameDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Grammar Name failed to add, please investigate", "Error Adding New Grammar Name");
                    return;
                }
                ToastService?.ShowSuccess("Grammar Name added successfully", "SUCCESS");
            }
            else
            {
                if (GrammarNameDataService != null)
                {
                    await GrammarNameDataService!.UpdateGrammarName(GrammarNameDTO, "");
                    ToastService?.ShowSuccess("The Grammar Name updated successfully", "SUCCESS");
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