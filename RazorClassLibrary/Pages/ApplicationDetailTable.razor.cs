using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.DTO;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using RazorClassLibrary.Pages;

using System.Security.Claims;

using VoiceLauncher.Services;
using VoiceLauncher.Shared;

namespace VoiceLauncher.Pages
{
  public partial class ApplicationDetailTable : ComponentBase
  {
    [Inject] public IApplicationDetailDataService? ApplicationDetailDataService { get; set; }
    [Inject] public NavigationManager? NavigationManager { get; set; }
    [Inject] public ILogger<ApplicationDetailTable>? Logger { get; set; }
    [Inject] public IToastService? ToastService { get; set; }
    [CascadingParameter] public IModalService? Modal { get; set; }
    public string Title { get; set; } = "ApplicationDetail Items (ApplicationDetails)";
    public List<ApplicationDetailDTO>? ApplicationDetailDTO { get; set; }
    public List<ApplicationDetailDTO>? FilteredApplicationDetailDTO { get; set; }
    protected ApplicationDetailAddEdit? ApplicationDetailAddEdit { get; set; }
    ElementReference SearchInput;
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
    protected override async Task OnInitializedAsync()
    {
      await LoadData();
    }

    private async Task LoadData()
    {
      try
      {
        if (ApplicationDetailDataService != null)
        {
          var result = await ApplicationDetailDataService!.GetAllApplicationDetailsAsync();
          //var result = await ApplicationDetailDataService.SearchApplicationDetailsAsync(ServerSearchTerm);
          if (result != null)
          {
            ApplicationDetailDTO = result.ToList();
          }
        }

      }
      catch (Exception e)
      {
        Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
        _loadFailed = true;
        ExceptionMessage = e.Message;
      }
      FilteredApplicationDetailDTO = ApplicationDetailDTO;
      Title = $"Application Detail ({FilteredApplicationDetailDTO?.Count})";

    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        try
        {
          if (JSRuntime != null)
          {
            await JSRuntime.InvokeVoidAsync("window.setFocus", "SearchInput");
          }
        }
        catch (Exception exception)
        {
          Console.WriteLine(exception.Message);
        }
      }
    }
    protected async Task AddNewApplicationDetailAsync()
    {
      var parameters = new ModalParameters();
      var formModal = Modal?.Show<ApplicationDetailAddEdit>("Add Application Detail", parameters);
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
      if (FilteredApplicationDetailDTO == null || ApplicationDetailDTO == null)
      {
        return;
      }
      if (string.IsNullOrEmpty(SearchTerm))
      {
        FilteredApplicationDetailDTO = ApplicationDetailDTO.OrderBy(v => v.ProcessName).ToList();
        Title = $"All Application Detail ({FilteredApplicationDetailDTO.Count})";
      }
      else
      {
        var temporary = SearchTerm.ToLower().Trim();
        FilteredApplicationDetailDTO = ApplicationDetailDTO
            .Where(v =>
            (v.ProcessName != null && v.ProcessName.ToLower().Contains(temporary))
             || (v.ApplicationTitle != null && v.ApplicationTitle.ToLower().Contains(temporary))
            )
            .ToList();
        Title = $"Filtered Application Details ({FilteredApplicationDetailDTO.Count})";
      }
    }
    protected void SortApplicationDetail(string sortColumn)
    {
      Guard.Against.Null(sortColumn, nameof(sortColumn));
      if (FilteredApplicationDetailDTO == null)
      {
        return;
      }
      if (sortColumn == "ProcessName")
      {
        FilteredApplicationDetailDTO = FilteredApplicationDetailDTO.OrderBy(v => v.ProcessName).ToList();
      }
      else if (sortColumn == "ProcessName Desc")
      {
        FilteredApplicationDetailDTO = FilteredApplicationDetailDTO.OrderByDescending(v => v.ProcessName).ToList();
      }
      if (sortColumn == "ApplicationTitle")
      {
        FilteredApplicationDetailDTO = FilteredApplicationDetailDTO.OrderBy(v => v.ApplicationTitle).ToList();
      }
      else if (sortColumn == "ApplicationTitle Desc")
      {
        FilteredApplicationDetailDTO = FilteredApplicationDetailDTO.OrderByDescending(v => v.ApplicationTitle).ToList();
      }
    }
    async Task DeleteApplicationDetailAsync(int Id)
    {
      //Optionally remove child records here or warn about their existence
      //var ? = await ?DataService.GetAllApplicationDetail(Id);
      //if (? != null)
      //{
      //	ToastService.ShowWarning($"It is not possible to delete a applicationDetail that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
      //	return;
      //}
      var parameters = new ModalParameters();
      if (ApplicationDetailDataService != null)
      {
        var applicationDetail = await ApplicationDetailDataService.GetApplicationDetailById(Id);
        parameters.Add("Title", "Please Confirm, Delete Application Detail");
        parameters.Add("Message", $"ProcessName: {applicationDetail?.ProcessName}");
        parameters.Add("ButtonColour", "danger");
        parameters.Add("Icon", "fa fa-trash");
        var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Application Detail ({applicationDetail?.ProcessName})?", parameters);
        if (formModal != null)
        {
          var result = await formModal.Result;
          if (!result.Cancelled)
          {
            await ApplicationDetailDataService.DeleteApplicationDetail(Id);
            ToastService?.ShowSuccess(" Application Detail deleted successfully", "SUCCESS");
            await LoadData();
          }
        }
      }
    }
    async Task EditApplicationDetailAsync(int Id)
    {
      var parameters = new ModalParameters();
      parameters.Add("Id", Id);
      var formModal = Modal?.Show<ApplicationDetailAddEdit>("Edit Application Detail", parameters);
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