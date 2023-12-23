using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics;
namespace RazorClassLibrary.Pages
{
	public partial class CommandSetOverview : ComponentBase
	{
		public CommandSetService? CommandSetService { get; set; }
		private bool _showInfo { get; set; } = false;
		[Inject] NavigationManager? NavigationManager { get; set; }
		[Inject] public ILogger<CommandSetOverview>? Logger { get; set; }
		[Inject] Microsoft.AspNetCore.Hosting.IWebHostEnvironment? WebHostEnvironment { get; set; }
		[Inject] public IToastService? ToastService { get; set; }
		[CascadingParameter] public IModalService? Modal { get; set; }
		[Inject] public IJSRuntime? JavaScriptRuntime { get; set; }
		private string? _result { get; set; }
		public bool ViewNew { get; set; } = false;
		public bool ShowCommands { get; set; } = true;
		public bool ShowLists { get; set; } = false;
		public string Title { get; set; } = "Command Set";
		private string searchTerm = "";
		public string SearchTerm
		{
			get => searchTerm;
			set
			{
				searchTerm = value;
				if (!string.IsNullOrWhiteSpace(searchTerm) || !string.IsNullOrWhiteSpace(SearchTermApplication))
				{
					ApplyFilter();
				}
				else
				{
					FilteredTargetApplications = null;
				}
			}
		}
		private string searchTermApplication = "";
		public string SearchTermApplication
		{
			get => searchTermApplication;
			set
			{
				searchTermApplication = value;
				if (!string.IsNullOrWhiteSpace(searchTermApplication) || !string.IsNullOrWhiteSpace(SearchTerm))
				{
					ApplyFilter();
				}
				else
				{
					FilteredTargetApplications = null;
				}
			}
		}
		public bool IsKnowbrainer { get; set; } = true;
		public CommandSet? CommandSet { get; set; }
		public List<TargetApplication>? TargetApplications { get; set; }
		public List<TargetApplication>? FilteredTargetApplications { get; set; }
		public List<VoiceCommand>? VoiceCommands { get; set; }
		public List<VoiceCommand>? FilteredVoiceCommands { get; set; }
#pragma warning disable 414, 649
		private bool _loadFailed = false;
#pragma warning restore 414, 649
		ElementReference SearchInput;
		int recordsReturned = 0;
		public string ExceptionMessage { get; set; } = string.Empty;
		public bool ShowCode { get; set; } = false;
		protected override void OnInitialized()
		{
			if (Environment.MachineName == "J40L4V3")
			{
				ViewNew = false;
			}
			LoadData();
		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await SearchInput.FocusAsync();
			}
		}
		void LoadData(string? knowbrainerScriptFileName = null, string? dragonScriptFileName = null)
		{
			var query = new Uri(NavigationManager!.Uri).Query;
			if (QueryHelpers.ParseQuery(query).TryGetValue("viewnew", out var viewNew))
			{
				ViewNew = viewNew.ToString().ToLower() == "true";
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("showcommands", out var showCommands))
			{
				ShowCommands = showCommands.ToString().ToLower() == "true" ? true : false;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("showlists", out var showLists))
			{
				ShowLists = showLists.ToString().ToLower() == "true" ? true : false;
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("showcode", out var showCode))
			{
				ShowCode = showCode.ToString().ToLower() == "true" ? true : false;
			}
			try
			{
				CommandSetService = new CommandSetService(knowbrainerScriptFileName, dragonScriptFileName, ViewNew);
				CommandSet = CommandSetService.GetCommandSet();
			}
			catch (Exception e)
			{
				Logger!.LogError(e,"Exception occurred in on initialised  CommandSet Data Service");
				_loadFailed = true;
				ExceptionMessage = e.Message;
				ToastService!.ShowError(e.Message + " Error Loading Command Set");
			}
			TargetApplications = CommandSet!.TargetApplications;
			FilteredTargetApplications = TargetApplications;
			if (QueryHelpers.ParseQuery(query).TryGetValue("name", out var name))
			{
				SearchTerm = $"{name}";
			}
			if (QueryHelpers.ParseQuery(query).TryGetValue("application", out var application))
			{

				SearchTermApplication = $"{application}";

			}
			if (!string.IsNullOrWhiteSpace(SearchTerm) || !string.IsNullOrWhiteSpace(SearchTermApplication))
			{
				ApplyFilter();
			}
			else
			{
				SearchTerm = "Window";
				SearchTermApplication = "Global";
				ApplyFilter();
			}
		}
		private void ApplyFilter()
		{
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredTargetApplications = TargetApplications!.OrderBy(v => v.Module).ToList();
				Title = $"All Applications ({FilteredTargetApplications.Count})";
			}
			else
			{
				var searchTermRevised = SearchTerm.Trim().ToLower();
				FilteredTargetApplications = TargetApplications!
				  .Where(v => v.VoiceCommands.Any(c => c.Name.ToLower().Contains(searchTermRevised)))
				  .ToList();

			}
			if (!string.IsNullOrEmpty(SearchTermApplication))
			{
				var searchTermRevised = SearchTermApplication.Trim().ToLower();
				FilteredTargetApplications = FilteredTargetApplications
				  .Where(v => v.Module != null && v.Module.ToLower().Contains(searchTermRevised)
					 || v.ModuleDescription != null && v.ModuleDescription.ToLower().Contains(searchTermRevised)
					 || v.Company != null && v.Company.ToLower().Contains(searchTermRevised)
					 || v.WindowTitle != null && v.WindowTitle.ToLower().Contains(searchTermRevised))
				  .ToList();
			}
			recordsReturned = FilteredTargetApplications.Count;
			Title = $"Filtered Applications ({recordsReturned})";
			ShowCommands = recordsReturned < 8;
			ShowLists = false;
			ShowCode = recordsReturned<8;
		}
		protected void SortTargetApplications(string sortColumn)
		{
			if (sortColumn == "Company")
			{
				FilteredTargetApplications = FilteredTargetApplications!.OrderBy(o => o.Company).ToList();
			}
		}
		public async Task CommandDrillDownAsync(string name)
		{
			SearchTerm = name;
			await JavaScriptRuntime!.InvokeVoidAsync(
	 "clipboardCopy.copyText", name);
			_result = $"Copied Spoken Text/Command Name Successfully at {DateTime.Now:hh:mm}";
			ShowLists = true;
			ShowCode = true;
			ToastService!.ShowSuccess(_result);
		}
		async Task ImportFileAsync(InputFileChangeEventArgs arguments)
		{
			long maxFileSize = 1024 * 2048;
			var file = arguments.File;
			var reader = file.OpenReadStream(maxFileSize);
			var path = $"{WebHostEnvironment!.WebRootPath}\\{file.Name}";
			FileStream fileStream = File.Create(path);
			await reader.CopyToAsync(fileStream);
			reader.Close();
			fileStream.Close();
			if (IsKnowbrainer)
			{
				LoadData(path);
			}
			else
			{
				LoadData(null, path);
			}
		}
		void SetImportFlag(bool isKnowbrainer)
		{
			IsKnowbrainer = isKnowbrainer;
		}
		private void BuildSearchUrl()
		{
			var commandName = SearchTerm;
			commandName = commandName.Replace(" ", "%20");
			commandName = commandName.Replace("<", "%3C");
			commandName = commandName.Replace(">", "%3E");

			var url = $"https://voicelauncherblazor.azurewebsites.net/commandsetoverview?name={commandName}&application={SearchTermApplication}&viewnew={ViewNew}&showcommands={ShowCommands}&showlists={ShowLists}&showcode={ShowCode}";
			NavigationManager!.NavigateTo(url);
		}
		void ApplyApplicationFilter(string module)
		{
			SearchTermApplication = module;
		}

		string Message = "";
		private void OpenFileKb()
		{
			var psi = new ProcessStartInfo();
			psi.UseShellExecute = true;
			psi.FileName = "code";
			psi.WorkingDirectory = @"C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands";
			psi.Arguments = @"-g C:\Users\MPhil\AppData\Roaming\KnowBrainer\KnowBrainerCommands\MyKBCommands.xml:1";
			psi.UseShellExecute = true;
			try
			{
				Process.Start(psi);
			}
			catch (Exception exception)
			{
				Message = exception.Message;
			}

		}
		private void OpenFileDragon()
		{
			var psi = new ProcessStartInfo();
			psi.UseShellExecute = true;
			psi.FileName = "code";
			psi.WorkingDirectory = @"C:\Users\MPhil\OneDrive\Documents";
			psi.Arguments = @"-g C:\Users\MPhil\OneDrive\Documents\MyCommands.xml:2";
			psi.UseShellExecute = true;
			try
			{
				Process.Start(psi);
			}
			catch (Exception exception)
			{
				Message = exception.Message;
			}
		}
		private void SetApplicationFilter(string application)
		{
			SearchTermApplication = application;
		}
	}
}