using Blazored.Toast.Services;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace VoiceLauncher.Pages
{
	public partial class Launcher
	{
		[Parameter] public int LauncherId { get; set; }
		private EditContext? _editContext;
		[Inject] public IJSRuntime? JSRuntime { get; set; }
		[Inject] IToastService? ToastService { get; set; }
		public DataAccessLibrary.Models.Launcher? LauncherModel { get; set; }
		public List<DataAccessLibrary.Models.Category>? Categories { get; set; }
		public List<DataAccessLibrary.Models.Computer>? Computers { get; set; }
		public string? Message { get; set; }
		[Parameter] public EventCallback OnClose { get; set; }
#pragma warning disable 414
		private bool _loadFailed = false;
#pragma warning restore 414

		protected override async Task OnInitializedAsync()
		{
			if (LauncherId > 0)
			{
				try
				{
					LauncherModel = await LauncherService.GetLauncherAsync(LauncherId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
			else
			{
				LauncherModel = new DataAccessLibrary.Models.Launcher
				{
					CommandLine = "http://"
				};
			}
            if (LauncherModel!= null )
            {
                _editContext = new EditContext(LauncherModel); 
            }
			try
			{
				Categories = await CategoryService.GetCategoriesByTypeAsync("Launch Applications");
				Computers = await ComputerService.GetComputersAsync();
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
		}

		protected override async Task OnParametersSetAsync()
		{
			if (LauncherId > 0)
			{
				try
				{
					LauncherModel = await LauncherService.GetLauncherAsync(LauncherId);
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message);
					_loadFailed = true;
				}
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await JSRuntime!.InvokeVoidAsync("setFocus", "LauncherName");
			}
		}

		private async Task HandleValidSubmit()
		{
			if (Environment.MachineName != "J40L4V3")
			{
				ToastService!.ShowError("This demo application does not allow editing of data!", "Demo Only");
				return;
			}
			try
			{
				var result = await LauncherService.SaveLauncher(LauncherModel);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				_loadFailed = true;
			}
			Message = $"Saved Successfully {DateTime.Now.ToLongTimeString()}";
            if (LauncherModel?.Category!= null )
            {
				NavigationManager.NavigateTo($"/launchers?category={LauncherModel?.Category.CategoryName}");
                return;
            }
			NavigationManager.NavigateTo($"/launchers");
		}
		void HideMessage(MouseEventArgs args)
		{
			Message = null;
		}
		private async Task CallChangeAsync(string elementId)
		{
			await JSRuntime!.InvokeVoidAsync("CallChange", elementId);
		}

	}
}
