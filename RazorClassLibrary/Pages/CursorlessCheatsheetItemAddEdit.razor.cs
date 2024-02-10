
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
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public partial class CursorlessCheatsheetItemAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<CursorlessCheatsheetItemAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? id { get; set; }
        public CursorlessCheatsheetItemDTO CursorlessCheatsheetItemDTO { get; set; } = new CursorlessCheatsheetItemDTO();//{ };
        [Inject] public ICursorlessCheatsheetItemDataService? CursorlessCheatsheetItemDataService { get; set; }
        [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (CursorlessCheatsheetItemDataService == null)
            {
                return;
            }
            if (id != null && id != 0)
            {
                var result = await CursorlessCheatsheetItemDataService.GetCursorlessCheatsheetItemById((int)id);
                if (result != null)
                {
                    CursorlessCheatsheetItemDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "SpokenForm");
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
            if ((id == 0 || id == null) && CursorlessCheatsheetItemDataService != null)
            {
                CursorlessCheatsheetItemDTO? result = await CursorlessCheatsheetItemDataService.AddCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Cursorless Cheatsheet Item failed to add, please investigate Error Adding New Cursorless Cheatsheet Item");
                    ToastService?.ShowError("Cursorless Cheatsheet Item failed to add, please investigate Error Adding New Cursorless Cheatsheet Item");
                    return;
                }
                ToastService?.ShowSuccess("Cursorless Cheatsheet Item added successfully");
            }
            else
            {
                if (CursorlessCheatsheetItemDataService != null)
                {
                    await CursorlessCheatsheetItemDataService!.UpdateCursorlessCheatsheetItem(CursorlessCheatsheetItemDTO, "");
                    ToastService?.ShowSuccess("The Cursorless Cheatsheet Item updated successfully");
                }
            }
            if (ModalInstance != null)
            {
                await ModalInstance.CloseAsync(ModalResult.Ok(true));
            }
            TaskRunning = false;
        }
    }

    public class ApplicationState
    {
    }
}