using Blazored.Modal;
using Blazored.Modal.Services;

using Microsoft.AspNetCore.Components;

namespace RazorClassLibrary.Shared;

 public partial class BlazoredModalConfirmDialog : ComponentBase
 {
     [Parameter]
     public string Title { get; set; } = "Please Confirm";
     [Parameter]
     public string? Message { get; set; }

     [Parameter]
     public string ButtonColour { get; set; } = "danger";
     [Parameter]
     public string? Icon { get; set; } = "fas fa-question";
     [CascadingParameter]
     BlazoredModalInstance? ModalInstance { get; set; }

     ElementReference CancelButton;
     void OnCancel()
     {
         ModalInstance?.CancelAsync();
     }

     void OnConfirm()
     {
         ModalInstance?.CloseAsync(ModalResult.Ok(true));
     }

     protected override async Task OnAfterRenderAsync(bool firstRender)
     {
         if (firstRender)
         {
             await CancelButton.FocusAsync();
         }
     }
 }