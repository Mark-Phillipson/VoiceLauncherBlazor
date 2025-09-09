
using System;
using System.Collections.Generic;
using System.Linq;
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

using System.Threading.Tasks;

namespace RazorClassLibrary.Pages;

public partial class ExampleAddEdit : ComponentBase
{
        [Inject] public required IToastService ToastService { get; set; }
        [CascadingParameter] BlazoredModalInstance? ModalInstance { get; set; }
        [Parameter] public string? Title { get; set; }
        [Inject] public required ILogger<ExampleAddEdit> Logger { get; set; }
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Parameter] public int? Id { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        public ExampleDTO ExampleDTO { get; set; } = new ExampleDTO();//{ };
        [Inject] public required IExampleDataService ExampleDataService { get; set; }
        [Parameter] public int ParentId { get; set; }

    ElementReference? FirstInput;
    protected override async Task OnInitializedAsync()
    {
            if (Id != null && Id != 0)
            {
                var result = await ExampleDataService.GetExampleById((int)Id);
                if (result != null)
                {
                    ExampleDTO = result;
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
                if (FirstInput != null)
                {
                    await FirstInput.Value.FocusAsync();
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
        if ((Id == 0 || Id == null))
        {
            ExampleDTO? result = await ExampleDataService.AddExample(ExampleDTO);
            if (result == null)
            {
                Logger.LogError("Error adding Example");
            }
            else
            {
                ExampleDTO = result;
                NavigationManager.NavigateTo("/Examples");
            }
        }
        else
        {
            var updateResult = await ExampleDataService.UpdateExample(ExampleDTO,"TBC");
            if (updateResult==null)
            {
                Logger.LogError("Error updating Example");
            }
            else
            {
                NavigationManager.NavigateTo("/Examples");
            }
        }
    }
}