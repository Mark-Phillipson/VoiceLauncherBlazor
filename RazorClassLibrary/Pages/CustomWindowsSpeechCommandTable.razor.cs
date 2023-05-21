using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using System.Security.Claims;

using VoiceLauncher.Shared;

namespace VoiceLauncher.Pages
{
  public partial class CustomWindowsSpeechCommandTable : ComponentBase
  {
    [Inject] public ICustomWindowsSpeechCommandDataService? CustomWindowsSpeechCommandDataService { get; set; }
    [Parameter] public int WindowsSpeechVoiceCommandId { get; set; }
    [Parameter] public DateTime Updated { get; set; }
    [Inject] public NavigationManager? NavigationManager { get; set; }
    [Inject] public ILogger<CustomWindowsSpeechCommandTable>? Logger { get; set; }
    [Inject] public IToastService? ToastService { get; set; }
    [CascadingParameter] public IModalService? Modal { get; set; }
    public string Title { get; set; } = "CustomWindowsSpeechCommand Items (CustomWindowsSpeechCommands)";
    public List<CustomWindowsSpeechCommandDTO>? CustomWindowsSpeechCommandDTO { get; set; }
    public List<CustomWindowsSpeechCommandDTO>? FilteredCustomWindowsSpeechCommandDTO { get; set; }
    protected CustomWindowsSpeechCommandAddEdit? CustomWindowsSpeechCommandAddEdit { get; set; }
#pragma warning disable 414, 649
    private bool _loadFailed = false;
    private string? searchTerm = null;
#pragma warning restore 414, 649
    public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
    [Parameter] public string? ServerSearchTerm { get; set; }
    public string ExceptionMessage { get; set; } = String.Empty;
    public List<string>? PropertyInfo { get; set; }
    [CascadingParameter] public ClaimsPrincipal? User { get; set; }
    [Inject] public IJSRuntime? JSRuntime { get; set; }
    protected override async Task OnParametersSetAsync()
    {
      await LoadData();
    }

    protected override async Task OnInitializedAsync()
    {
      await LoadData();
    }

    private async Task LoadData()
    {
      try
      {
        if (CustomWindowsSpeechCommandDataService != null && WindowsSpeechVoiceCommandId > 0)
        {
          var result = await CustomWindowsSpeechCommandDataService!.GetAllCustomWindowsSpeechCommandsAsync(WindowsSpeechVoiceCommandId);
          if (result != null)
          {
            CustomWindowsSpeechCommandDTO = result.ToList();
          }
        }

      }
      catch (Exception e)
      {
        Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
        _loadFailed = true;
        ExceptionMessage = e.Message;
      }
      FilteredCustomWindowsSpeechCommandDTO = CustomWindowsSpeechCommandDTO;
      Title = $"Actions ({FilteredCustomWindowsSpeechCommandDTO?.Count})";

    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        try
        {
          if (JSRuntime != null)
          {
            await JSRuntime.InvokeVoidAsync("window.setFocus", "TextToEnter");
          }
        }
        catch (Exception exception)
        {
          Console.WriteLine(exception.Message);
        }
      }
    }
    protected async Task AddNewCustomWindowsSpeechCommandAsync()
    {
      var parameters = new ModalParameters();
      parameters.Add(nameof(WindowsSpeechVoiceCommandId), WindowsSpeechVoiceCommandId);
      var formModal = Modal?.Show<CustomWindowsSpeechCommandAddEdit>("Add Custom Windows Speech Command", parameters);
      if (formModal != null)
      {
        var result = await formModal.Result;
        if (!result.Cancelled)
        {
          await LoadData();
        }
      }
    }

    private void ApplyFilter()
    {
      if (FilteredCustomWindowsSpeechCommandDTO == null || CustomWindowsSpeechCommandDTO == null)
      {
        return;
      }
      if (string.IsNullOrEmpty(SearchTerm))
      {
        FilteredCustomWindowsSpeechCommandDTO = CustomWindowsSpeechCommandDTO.OrderBy(v => v.TextToEnter).ToList();
        Title = $"Actions ({FilteredCustomWindowsSpeechCommandDTO.Count})";
      }
      else
      {
        var temporary = SearchTerm.ToLower().Trim();
        FilteredCustomWindowsSpeechCommandDTO = CustomWindowsSpeechCommandDTO
            .Where(v =>
            (v.TextToEnter != null && v.TextToEnter.ToLower().Contains(temporary))
            )
            .ToList();
        Title = $"Actions ({FilteredCustomWindowsSpeechCommandDTO.Count})";
      }
    }
    protected void SortCustomWindowsSpeechCommand(string sortColumn)
    {
      Guard.Against.Null(sortColumn, nameof(sortColumn));
      if (FilteredCustomWindowsSpeechCommandDTO == null)
      {
        return;
      }
      if (sortColumn == "TextToEnter")
      {
        FilteredCustomWindowsSpeechCommandDTO = FilteredCustomWindowsSpeechCommandDTO.OrderBy(v => v.TextToEnter).ToList();
      }
      else if (sortColumn == "TextToEnter Desc")
      {
        FilteredCustomWindowsSpeechCommandDTO = FilteredCustomWindowsSpeechCommandDTO.OrderByDescending(v => v.TextToEnter).ToList();
      }
    }
    async Task DeleteCustomWindowsSpeechCommandAsync(int Id)
    {
      //Optionally remove child records here or warn about their existence
      //var ? = await ?DataService.GetAllCustomWindowsSpeechCommand(Id);
      //if (? != null)
      //{
      //	ToastService.ShowWarning($"It is not possible to delete a customWindowsSpeechCommand that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
      //	return;
      //}
      var parameters = new ModalParameters();
      if (CustomWindowsSpeechCommandDataService != null)
      {
        var customWindowsSpeechCommand = await CustomWindowsSpeechCommandDataService.GetCustomWindowsSpeechCommandById(Id);
        parameters.Add("Title", "Please Confirm, Delete Custom Windows Speech Command");
        parameters.Add("Message", $"TextToEnter: {customWindowsSpeechCommand?.TextToEnter}");
        parameters.Add("ButtonColour", "danger");
        parameters.Add("Icon", "fa fa-trash");
        var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Custom Windows Speech Command ({customWindowsSpeechCommand?.TextToEnter})?", parameters);
        if (formModal != null)
        {
          var result = await formModal.Result;
          if (!result.Cancelled)
          {
            await CustomWindowsSpeechCommandDataService.DeleteCustomWindowsSpeechCommand(Id);
            ToastService?.ShowSuccess(" Custom Windows Speech Command deleted successfully", "SUCCESS");
            await LoadData();
          }
        }
      }
    }
    async Task EditCustomWindowsSpeechCommandAsync(int Id)
    {
      var parameters = new ModalParameters();
      parameters.Add("Id", Id);
      var formModal = Modal?.Show<CustomWindowsSpeechCommandAddEdit>("Edit Custom Windows Speech Command", parameters);
      if (formModal != null)
      {
        var result = await formModal.Result;
        if (!result.Cancelled)
        {
          await LoadData();
        }
      }
    }
  }
}