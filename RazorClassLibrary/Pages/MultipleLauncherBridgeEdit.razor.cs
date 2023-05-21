using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;

namespace VoiceLauncher.Pages
{
  public partial class MultipleLauncherBridgeEdit : ComponentBase
  {
    [Inject]
    public LauncherMultipleLauncherBridgeDataService? LauncherMultipleLauncherBridgeService { get; set; }
    [Inject]
    public LauncherService? LauncherService { get; set; }
    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    public LauncherMultipleLauncherBridge LauncherMultipleLauncherBridge { get; set; } = new LauncherMultipleLauncherBridge();
    public List<DataAccessLibrary.Models.Launcher>? Launchers { get; set; } = new List<DataAccessLibrary.Models.Launcher>();
    public List<MultipleLauncher> MultipleLaunchers { get; set; } = new List<MultipleLauncher>();
    //needed to bind to select value
    protected string LauncherId = "1";
    protected string MultipleLauncherId = "1";

    [Parameter]
    public int MultipleLauncherBridgeId { get; set; }
    public string? Message { get; set; }
    public bool ShowDialog { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
      if (LauncherService != null)
      {
        Launchers = (await LauncherService.GetLaunchersAsync()).ToList();
        MultipleLaunchers = (await LauncherService.GetMultipleLaunchersAsync());
      }
      if (MultipleLauncherBridgeId != 0 && LauncherMultipleLauncherBridgeService != null)
      {
        LauncherMultipleLauncherBridge = await LauncherMultipleLauncherBridgeService.GetLauncherMultipleLauncherBridgeAsync(MultipleLauncherBridgeId);
      }
      else
      {
        LauncherMultipleLauncherBridge = new LauncherMultipleLauncherBridge() { };
      }
    }

    protected async Task HandleValidSubmit()
    {

      // We can handle certain requests automatically
      if (LauncherMultipleLauncherBridge.Id == 0 && LauncherMultipleLauncherBridgeService != null) // New 
      {
        await LauncherMultipleLauncherBridgeService.SaveLauncherMultipleLauncherBridge(LauncherMultipleLauncherBridge);
        NavigationManager?.NavigateTo("/multiplelaunchers");
      }
      else
      {
        if (LauncherMultipleLauncherBridgeService != null)
        {
          await LauncherMultipleLauncherBridgeService.SaveLauncherMultipleLauncherBridge(LauncherMultipleLauncherBridge);
        }
        NavigationManager?.NavigateTo("/multiplelaunchers");
      }
    }

    protected void NavigateToOverview()
    {
      NavigationManager?.NavigateTo("/multiplelaunchers");
    }
    protected async void DeleteLauncherMultipleLauncherBridge()
    {
      if (LauncherMultipleLauncherBridgeService != null)
      {
        await LauncherMultipleLauncherBridgeService.DeleteLauncherMultipleLauncherBridge(LauncherMultipleLauncherBridge.Id);

      }
      ShowDialog = false;
      NavigationManager?.NavigateTo("/expensesoverview");
    }
    protected void ShowDeleteConfirmation()
    {
      ShowDialog = true;
    }
    protected void CancelDelete()
    {
      ShowDialog = false;
    }

  }
}