
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
using DataAccessLibrary.Services;
using DataAccessLibrary.DTO;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class LanguageAddEdit : ComponentBase
    {
        [Inject] IToastService? ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public ILogger<LanguageAddEdit>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        public LanguageDTO LanguageDTO { get; set; } = new LanguageDTO() { LanguageName = "Language Name", Colour = "#000000" };
        [Inject] public ILanguageDataService? LanguageDataService { get; set; }

        [Parameter] public int ParentId { get; set; }
#pragma warning disable 414, 649
        bool TaskRunning = false;
#pragma warning restore 414, 649
        protected override async Task OnInitializedAsync()
        {
            if (LanguageDataService == null)
            {
                return;
            }
            if (Id != null && Id != 0)
            {
                var result = await LanguageDataService.GetLanguageById((int)Id);
                if (result != null)
                {
                    LanguageDTO = result;
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
                        await JSRuntime.InvokeVoidAsync("window.setFocus", "Language");
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
            LanguageDTO.LanguageName = "";
            LanguageDTO.Colour = "";
            if (ModalInstance != null)
                await ModalInstance.CancelAsync();
        }
        protected async Task HandleValidSubmit()
        {
            TaskRunning = true;
            if ((Id == 0 || Id == null) && LanguageDataService != null)
            {
                LanguageDTO? result = await LanguageDataService.AddLanguage(LanguageDTO);
                if (result == null && Logger != null)
                {
                    Logger.LogError("Language failed to add, please investigate Error Adding New Language");
                    ToastService?.ShowError("Language failed to add, please investigate Error Adding New Language");
                    return;
                }
                ToastService?.ShowSuccess("Language added successfully");
            }
            else
            {
                if (LanguageDataService != null)
                {
                    await LanguageDataService!.UpdateLanguage(LanguageDTO, "");
                    ToastService?.ShowSuccess("The Language updated successfully");
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