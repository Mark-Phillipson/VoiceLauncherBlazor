using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class PhraseListGrammarAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public PhraseListGrammarDTO PhraseListGrammarDTO { get; set; } = new PhraseListGrammarDTO();//{ };
        [Inject] public IPhraseListGrammarDataService? PhraseListGrammarDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (PhraseListGrammarDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await PhraseListGrammarDataService.GetPhraseListGrammarById((int)Id);
                if (result != null)
                {
                    PhraseListGrammarDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "PhraseListGrammarValue");
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
            if ((Id == 0 || Id == null) && PhraseListGrammarDataService != null)
            {
                PhraseListGrammarDTO? result = await PhraseListGrammarDataService.AddPhraseListGrammar(PhraseListGrammarDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Phrase List Grammar failed to add, please investigate Error Adding New Phrase List Grammar");
                    return;
                }
                ToastService?.ShowSuccess("Phrase List Grammar added successfully");
            }
            else
            {
                if (PhraseListGrammarDataService != null)
                {
                    await PhraseListGrammarDataService!.UpdatePhraseListGrammar(PhraseListGrammarDTO, "");
                    ToastService?.ShowSuccess("The Phrase List Grammar updated successfully");
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