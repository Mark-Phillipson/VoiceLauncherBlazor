using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
	public partial class CommandSetOverview
	{
		 public CommandSetService CommandSetService { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }
		[Inject] public ILogger<CommandSetOverview> Logger { get; set; }
		[Inject] public IToastService ToastService { get; set; }
		[CascadingParameter] public IModalService Modal { get; set; }
		public bool ViewNew { get; set; } = false;
		public bool ShowCommands { get; set; } = true;
		public bool ShowLists { get; set; } = false;
		public string Title { get; set; } = "Command Set";
		private string searchTerm="Add Method";
		public string SearchTerm 
		{ get => searchTerm; 
			set {
				if (!string.IsNullOrWhiteSpace(value) )
				{
					searchTerm = value; 
				}
				else
				{
					searchTerm = "Add Method"; 
				}
				ApplyFilter(); 
			} 
		}
		private string searchTermApplication;
		public string SearchTermApplication { get => searchTermApplication; set { searchTermApplication = value; ApplyFilter(); } }

		public CommandSet CommandSet { get; set; }
		public List<TargetApplication> TargetApplications { get; set; }
		public List<TargetApplication> FilteredTargetApplications { get; set; }
		public List<VoiceCommand> VoiceCommands { get; set; }
		public List<VoiceCommand> FilteredVoiceCommands { get; set; }
#pragma warning disable 414, 649
		private bool _loadFailed = false;
#pragma warning restore 414, 649
		ElementReference SearchInput;
		public string ExceptionMessage { get; set; } = String.Empty;
		public bool ShowCode { get; set; } = false;
		protected override void OnInitialized()
		{
			LoadData();
		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await SearchInput.FocusAsync();
			}
		}
		void LoadData()
		{
			try
			{
				CommandSetService = new CommandSetService(null, ViewNew);
				CommandSet = CommandSetService.GetCommandSet();
			}
			catch (Exception e)
			{
				Logger.LogError("Exception occurred in on initialised  CommandSet Data Service", e);
				_loadFailed = true;
				ExceptionMessage = e.Message;
				ToastService.ShowError(e.Message, "Error Loading Command Set");
			}
			TargetApplications = CommandSet.TargetApplications;
			FilteredTargetApplications = TargetApplications;
			if (string.IsNullOrWhiteSpace(SearchTerm))
			{
				SearchTerm = "Add Method";
			}
			ApplyFilter();
		}
		private void ApplyFilter()
		{
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredTargetApplications = TargetApplications.OrderBy(v => v.Module).ToList();
				Title = $"All Applications ({FilteredTargetApplications.Count})";
			}
			else
			{
				var searchTermRevised = SearchTerm.Trim().ToLower();
					FilteredTargetApplications = TargetApplications
						.Where(v => (v.VoiceCommands.Any(c => c.Name.ToLower().Contains(searchTermRevised))))
						.ToList();

			}
			if (!string.IsNullOrEmpty(searchTermApplication))
			{
				var searchTermRevised = SearchTermApplication.Trim().ToLower();
				FilteredTargetApplications = FilteredTargetApplications
					.Where(v => (v.Module != null && v.Module.ToLower().Contains(searchTermRevised))
					  || (v.ModuleDescription != null && v.ModuleDescription.ToLower().Contains(searchTermRevised))
					  || (v.Company != null && v.Company.ToLower().Contains(searchTermRevised))
					  || (v.WindowTitle != null && v.WindowTitle.ToLower().Contains(searchTermRevised)))
					.ToList();
			}
			Title = $"Filtered Applications ({FilteredTargetApplications.Count})";
		}
		protected void SortTargetApplications(string sortColumn)
		{
			if (sortColumn == "Company")
			{
				FilteredTargetApplications = FilteredTargetApplications.OrderBy(o => o.Company).ToList();
			}
		}
		public void CommandDrillDown(string name)
		{
			SearchTerm = name;
		}
		public void ToggleScriptFile()
		{
			ViewNew = !ViewNew;
			CommandSet = null;
			LoadData();
		}
	}
}
