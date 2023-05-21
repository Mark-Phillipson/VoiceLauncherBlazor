using Blazored.Modal;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
  public partial class ValueToInsertAddEdit : ComponentBase
  {
    [Parameter] public EventCallback<bool> CloseModal { get; set; }
    [Parameter] public string? Title { get; set; }
    [Inject] public ILogger<ValueToInsertAddEdit>? Logger { get; set; }
    [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
    [Inject] public IJSRuntime? JSRuntime { get; set; }
    [Parameter] public int? Id { get; set; }
    public ValueToInsertDTO ValueToInsertDTO { get; set; } = new ValueToInsertDTO();//{ };
    [Inject] public IValueToInsertDataService? ValueToInsertDataService { get; set; }
    [Inject] public IToastService? ToastService { get; set; }
    //[Inject] public ApplicationState? ApplicationState { get; set; }
    [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
    bool TaskRunning = false;
#pragma warning restore 414, 649
    protected override async Task OnInitializedAsync()
    {
      if (ValueToInsertDataService == null)
      {
        return;
      }
      if (Id > 0)
      {
        var result = await ValueToInsertDataService.GetValueToInsertById((int)Id);
        if (result != null)
        {
          ValueToInsertDTO = result;
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
            await JSRuntime.InvokeVoidAsync("window.setFocus", "ValueToInsert");
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
      //            if (ModalInstance != null)
      //                ModalInstance.CancelAsync();
      await CloseModal.InvokeAsync(true);
    }

    protected async Task HandleValidSubmit()
    {
      TaskRunning = true;
      if ((Id == 0 || Id == null) && ValueToInsertDataService != null)
      {
        ValueToInsertDTO? result = await ValueToInsertDataService.AddValueToInsert(ValueToInsertDTO);
        if (result == null && Logger != null)
        {
          Logger.LogError("Value To Insert failed to add, please investigate Error Adding New Value To Insert");
          ToastService?.ShowError("Value To Insert failed to add, please investigate Error Adding New Value To Insert");
          return;
        }
        ToastService?.ShowSuccess("Value To Insert added successfully", "SUCCESS");

      }
      else
      {
        if (ValueToInsertDataService != null)
        {
          await ValueToInsertDataService!.UpdateValueToInsert(ValueToInsertDTO, "");
          ToastService?.ShowSuccess("The Value To Insert updated successfully", "SUCCESS");
        }
      }
      //if (ModalInstance != null)
      //{
      //    await ModalInstance.CloseAsync(ModalResult.Ok(true));
      //}
      await CloseModal.InvokeAsync(true);
      TaskRunning = false;
    }
  }
}