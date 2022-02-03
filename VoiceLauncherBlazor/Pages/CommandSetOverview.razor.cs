using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.Models.KnowbrainerCommands;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Pages
{
    public partial class CommandSetOverview
    {
        public CommandSetService CommandSetService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<CommandSetOverview> Logger { get; set; }
        [Inject] IWebHostEnvironment WebHostEnvironment { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService Modal { get; set; }
        public bool ViewNew { get; set; } = true;
        public bool ShowCommands { get; set; } = true;
        public bool ShowLists { get; set; } = false;
        public string Title { get; set; } = "Command Set";
        private string searchTerm = null;
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
        private string searchTermApplication;
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
            if (Environment.MachineName == "DESKTOP-UROO8T1")
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
        void LoadData(string knowbrainerScriptFileName = null, string dragonScriptFileName = null)
        {
            var query = new Uri(NavigationManager.Uri).Query;
            if (QueryHelpers.ParseQuery(query).TryGetValue("viewnew", out var viewNew))
            {
                ViewNew = viewNew.ToString().ToLower() == "true" ? true : false;
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
                Logger.LogError("Exception occurred in on initialised  CommandSet Data Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
                ToastService.ShowError(e.Message, "Error Loading Command Set");
            }
            TargetApplications = CommandSet.TargetApplications;
            FilteredTargetApplications = TargetApplications;
            if (QueryHelpers.ParseQuery(query).TryGetValue("name", out var name))
            {
                SearchTerm = name;
            }
            if (QueryHelpers.ParseQuery(query).TryGetValue("application", out var application))
            {
                SearchTermApplication = application;
            }
            if (!string.IsNullOrWhiteSpace(SearchTerm) || !string.IsNullOrWhiteSpace(SearchTermApplication))
            {
                ApplyFilter();
            }
            else
            {
                FilteredTargetApplications = null;
            }
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
            var recordsReturned = FilteredTargetApplications.Count;
            Title = $"Filtered Applications ({recordsReturned})";
            ShowCommands = recordsReturned < 55;
            ShowCode = recordsReturned < 11;
            ShowLists = false;
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
        async Task ImportFileAsync(InputFileChangeEventArgs arguments)
        {
            long maxFileSize = 1024 * 2048;
            var file = arguments.File;
            var reader = file.OpenReadStream(maxFileSize);
            var path = $"{WebHostEnvironment.WebRootPath}\\{file.Name}";
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
            string url = "";
            if (Environment.MachineName == "DESKTOP-UROO8T1")
            {
                url = $"https://localhost:5667/commandsetoverview?name={commandName}&application={SearchTermApplication}&viewnew={ViewNew}&showcommands={ShowCommands}&showlists={ShowLists}&showcode={ShowCode}";
            }
            else
            {
                url = $"https://voicelauncherblazor.azurewebsites.net/commandsetoverview?name={commandName}&application={SearchTermApplication}&viewnew={ViewNew}&showcommands={ShowCommands}&showlists={ShowLists}&showcode={ShowCode}";
            }
            NavigationManager.NavigateTo(url);
        }
        void ApplyApplicationFilter(string module)
        {
            SearchTermApplication = module;
        }
    }
}
