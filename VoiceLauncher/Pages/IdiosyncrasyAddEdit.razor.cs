
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
using VoiceLauncher.DTOs;
using VoiceLauncher.Services;
using System.Diagnostics.Tracing;

namespace VoiceLauncher.Pages;

public partial class IdiosyncrasyAddEdit : ComponentBase
{
    [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
    [Inject] public IJSRuntime? JSRuntime { get; set; }
    [Parameter] public int? Id { get; set; }
    public IdiosyncrasyDTO IdiosyncrasyDTO { get; set; } = new IdiosyncrasyDTO();//{ };
    [Inject] public IIdiosyncrasyDataService? IdiosyncrasyDataService { get; set; }
    [Inject] public IToastService? ToastService { get; set; }
    private List<string> _stringFormattingMethods { get; set; }= new List<string>() { "Just Replace","Starts With","Ends With","Contains"};
#pragma warning disable 414, 649
    string TaskRunning = "";
#pragma warning restore 414, 649
    protected override async Task OnInitializedAsync()
    {
        if (IdiosyncrasyDataService == null)
        {
            return;
        }
        if (Id > 0)
        {
            var result = await IdiosyncrasyDataService.GetIdiosyncrasyById((int)Id);
            if (result != null)
            {
                IdiosyncrasyDTO = result;
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
                    await JSRuntime.InvokeVoidAsync("window.setFocus", "FindString");
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
        if ((Id == 0 || Id == null) && IdiosyncrasyDataService != null)
        {
            IdiosyncrasyDTO? result = await IdiosyncrasyDataService.AddIdiosyncrasy(IdiosyncrasyDTO);
            if (result == null)
            {
                ToastService?.ShowError("Idiosyncrasy failed to add, please investigate", "Error Adding New Idiosyncrasy");
                return;
            }
            ToastService?.ShowSuccess("Idiosyncrasy added successfully", "SUCCESS");
        }
        else
        {
            if (IdiosyncrasyDataService != null)
            {
                await IdiosyncrasyDataService!.UpdateIdiosyncrasy(IdiosyncrasyDTO, "");
                ToastService?.ShowSuccess("The Idiosyncrasy updated successfully", "SUCCESS");
            }
        }
        if (ModalInstance != null)
        {
            await ModalInstance.CloseAsync(ModalResult.Ok(true));
        }
        TaskRunning = "";
    }
}