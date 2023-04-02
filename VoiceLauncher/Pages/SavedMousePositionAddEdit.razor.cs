
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
using VoiceLauncher.Services;
using DataAccessLibrary.DTO;

namespace VoiceLauncher.Pages
{
    public partial class SavedMousePositionAddEdit : ComponentBase
    {
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public ClaimsPrincipal? User { get; set; }
        [Parameter] public int? Id { get; set; }
        public SavedMousePositionDTO SavedMousePositionDTO { get; set; } = new SavedMousePositionDTO();//{ };
        [Inject] public ISavedMousePositionDataService? SavedMousePositionDataService { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
#pragma warning disable 414, 649
        string TaskRunning = "";
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (SavedMousePositionDataService == null)
            {
                return;
            }
            if (Id > 0)
            {
                var result = await SavedMousePositionDataService.GetSavedMousePositionById((int)Id);
                if (result != null)
                {
                    SavedMousePositionDTO = result;
                }
            }
            else
            {
            }
            if (User?.Identity?.Name != null)
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "NamedLocation");
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
            if ((Id == 0 || Id == null) && SavedMousePositionDataService != null)
            {
                SavedMousePositionDTO? result = await SavedMousePositionDataService.AddSavedMousePosition(SavedMousePositionDTO);
                if (result == null)
                {
                    ToastService?.ShowError("Saved Mouse Position failed to add, please investigate", "Error Adding New Saved Mouse Position");
                    return;
                }
                ToastService?.ShowSuccess("Saved Mouse Position added successfully", "SUCCESS");
            }
            else
            {
                if (SavedMousePositionDataService != null)
                {
                    await SavedMousePositionDataService!.UpdateSavedMousePosition(SavedMousePositionDTO, "MSP");
                    ToastService?.ShowSuccess("The Saved Mouse Position updated successfully", "SUCCESS");
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