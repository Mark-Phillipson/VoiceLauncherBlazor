using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using VoiceLauncher.Services;

namespace RazorClassLibrary.Pages
{
    public partial class GrammarItemAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Parameter] public int GrammarNameId { get; set; }
        public GrammarItemDTO GrammarItemDTO { get; set; } = new GrammarItemDTO();//{ };
        [Inject] public IGrammarItemDataService? GrammarItemDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (GrammarItemDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await GrammarItemDataService.GetGrammarItemById((int)Id);
                if (result != null)
                {
                    GrammarItemDTO = result;
                }
            }
            else
            {

                GrammarItemDTO.GrammarNameId = GrammarNameId;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "Value");
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
            if ((Id == 0 || Id == null) && GrammarItemDataService != null)
            {
                GrammarItemDTO? result = await GrammarItemDataService.AddGrammarItem(GrammarItemDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Grammar Item failed to add, please investigate", "Error Adding New Grammar Item");
                    return;
                }
                ToastService?.ShowSuccess("Grammar Item added successfully", "SUCCESS");
            }
            else
            {
                if (GrammarItemDataService != null)
                {
                    await GrammarItemDataService!.UpdateGrammarItem(GrammarItemDTO, "");
                    ToastService?.ShowSuccess("The Grammar Item updated successfully", "SUCCESS");
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