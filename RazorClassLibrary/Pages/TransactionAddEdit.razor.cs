
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
using Microsoft.Extensions.Logging;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public partial class TransactionAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<TransactionAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public TransactionDTO TransactionDTO { get; set; } = new TransactionDTO();//{ };
        [Inject] public ITransactionDataService? TransactionDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
        ElementReference FirstInput;
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (TransactionDataService == null)
            {
                return;
            }
            if (Id != null && Id != 0)
            {
                var result = await TransactionDataService.GetTransactionById((int)Id);
                if (result != null)
                {
                    TransactionDTO = result;
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
                    await Task.Delay(100);
                    await FirstInput.FocusAsync();
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
            if ((Id == 0 || Id == null) && TransactionDataService != null)
            {
                TransactionDTO? result = await TransactionDataService.AddTransaction(TransactionDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Transaction failed to add, please investigate Error Adding New Transaction");
                    ToastService?.ShowError("Transaction failed to add, please investigate Error Adding New Transaction");
                    return;
                }
                ToastService?.ShowSuccess("Transaction added successfully");
            }
            else
            {
                if (TransactionDataService != null)
                {
                    await TransactionDataService!.UpdateTransaction(TransactionDTO, "");
                    ToastService?.ShowSuccess("The Transaction updated successfully");
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