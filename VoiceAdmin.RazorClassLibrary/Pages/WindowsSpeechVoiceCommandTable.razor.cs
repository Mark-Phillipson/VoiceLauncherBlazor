using Ardalis.GuardClauses;

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;

using DataAccessLibrary;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using RazorClassLibrary.Shared;

namespace RazorClassLibrary.Pages
{
    public partial class WindowsSpeechVoiceCommandTable : ComponentBase
    {
        [Inject] public CreateCommands? CreateCommands { get; set; }
        [Inject] public  required IWindowsSpeechVoiceCommandDataService WindowsSpeechVoiceCommandDataService { get; set; }
        [Inject] public required  ICustomWindowsSpeechCommandDataService CustomWindowsSpeechVoiceCommandDataService { get; set; }
        [Inject] public  required ISpokenFormDataService SpokenFormDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<WindowsSpeechVoiceCommandTable>? Logger { get; set; }
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "WindowsSpeechVoiceCommand Items (WindowsSpeechVoiceCommands)";
        public List<WindowsSpeechVoiceCommandDTO>? WindowsSpeechVoiceCommandDTO { get; set; }
        public List<WindowsSpeechVoiceCommandDTO>? FilteredWindowsSpeechVoiceCommandDTO { get; set; }
        protected WindowsSpeechVoiceCommandAddEdit? WindowsSpeechVoiceCommandAddEdit { get; set; }
        public List<SpokenForm>? SpokenForms { get; set; }
        public DateTime Updated { get; set; } = DateTime.Now;
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }
        [Parameter] public string? ServerSearchTerm { get; set; }
        [Parameter] public int Id { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
        public List<string>? PropertyInfo { get; set; }
        private bool _hideActions { get; set; } = true;
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        private bool _showAutoCreated { get; set; } = false;
        private List<CommandsBreakdown>? _commandsBreakdown;
        int commandCount = 0;
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (WindowsSpeechVoiceCommandDataService != null)
                {
                    List<WindowsSpeechVoiceCommandDTO> result = new List<WindowsSpeechVoiceCommandDTO>();
                    if (ServerSearchTerm != null)
                    {

                        _hideActions = false;
                        result = WindowsSpeechVoiceCommandDataService.SearchWindowsSpeechVoiceCommandsAsync(ServerSearchTerm);
                    }
                    else if (Id > 0)
                    {
                        var command = await WindowsSpeechVoiceCommandDataService.GetWindowsSpeechVoiceCommandById(Id);
                        if (command != null)
                        {
                            result.Add(command);
                        }
                    }
                    else
                    {
                        result = await WindowsSpeechVoiceCommandDataService!.GetAllWindowsSpeechVoiceCommandsAsync(_showAutoCreated, 16);

                        Updated = DateTime.Now;
                    }
                    if (result != null)
                    {
                        SpokenForms = await WindowsSpeechVoiceCommandDataService.GetSpokenFormsAsync(result);
                        WindowsSpeechVoiceCommandDTO = result
                            .OrderByDescending(o => o.Id).ToList();
                    }
                    _commandsBreakdown = await WindowsSpeechVoiceCommandDataService.GetCommandsBreakdown();
                    commandCount = _commandsBreakdown.Sum(x => x.Number);
                }

            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredWindowsSpeechVoiceCommandDTO = WindowsSpeechVoiceCommandDTO;
            Title = $"Windows Voice Commands ({FilteredWindowsSpeechVoiceCommandDTO?.Count}) of {commandCount}";

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
        protected async Task AddNewWindowsSpeechVoiceCommandAsync()
        {
            if (WindowsSpeechVoiceCommandDataService == null) return;
            if (CustomWindowsSpeechVoiceCommandDataService == null) return;

            var parameters = new ModalParameters();
            var formModal = Modal?.Show<WindowsSpeechVoiceCommandAddEdit>("Add Voice Command", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    var parentCommand = await WindowsSpeechVoiceCommandDataService.GetLatestAdded();
                    var customWindowsSpeechCommands = await CustomWindowsSpeechVoiceCommandDataService.GetAllCustomWindowsSpeechCommandsAsync(parentCommand.Id);
                    if (customWindowsSpeechCommands.Count() > 0)
                    {
                        await LoadData();
                        return;
                    }
                    parameters = new ModalParameters();
                    parameters.Add("WindowsSpeechVoiceCommandId", parentCommand.Id);
                    formModal = Modal?.Show<CustomWindowsSpeechCommandAddEdit>($"Add Action for {parentCommand.SpokenCommand}", parameters);
                    if (formModal != null)
                    {
                        result = await formModal.Result;
                        if (!result.Cancelled)
                        {
                            await LoadData();
                        }
                    }
                }
            }
        }

        private async void ApplyFilter(bool filterFromServer = true)
        {
            if (FilteredWindowsSpeechVoiceCommandDTO == null || WindowsSpeechVoiceCommandDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredWindowsSpeechVoiceCommandDTO = WindowsSpeechVoiceCommandDTO.OrderBy(v => v.SpokenCommand).ToList();
                Title = $"All Windows Speech Voice Command ({FilteredWindowsSpeechVoiceCommandDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                if (filterFromServer)
                {
                    var result = await WindowsSpeechVoiceCommandDataService!.GetAllWindowsSpeechVoiceCommandsAsync(_showAutoCreated, 1000);
                    WindowsSpeechVoiceCommandDTO = result;
                }
                FilteredWindowsSpeechVoiceCommandDTO = WindowsSpeechVoiceCommandDTO
                    .Where(v =>
                    v.SpokenForms.Any(v => v.SpokenFormText.ToLower().Contains(temporary)) ||
                    v.Description != null && v.Description.ToLower().Contains(temporary) ||
                    v.ApplicationName != null && v.ApplicationName.ToLower().Contains(temporary)
                    ).ToList();
                Title = $"Filtered Windows Speech Voice Commands ({FilteredWindowsSpeechVoiceCommandDTO.Count})";
                if (FilteredWindowsSpeechVoiceCommandDTO.Count < 4)
                {
                    _hideActions = false;
                }
                else
                {
                    _hideActions = true;
                }

                StateHasChanged();
            }
        }
        protected void SortWindowsSpeechVoiceCommand(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredWindowsSpeechVoiceCommandDTO == null)
            {
                return;
            }
            if (sortColumn == "SpokenCommand")
            {
                FilteredWindowsSpeechVoiceCommandDTO = FilteredWindowsSpeechVoiceCommandDTO.OrderBy(v => v.SpokenCommand).ToList();
            }
            else if (sortColumn == "SpokenCommand Desc")
            {
                FilteredWindowsSpeechVoiceCommandDTO = FilteredWindowsSpeechVoiceCommandDTO.OrderByDescending(v => v.SpokenCommand).ToList();
            }
        }
        async Task DeleteWindowsSpeechVoiceCommandAsync(int Id)
        {
            //Optionally remove child records here or warn about their existence
            //var ? = await ?DataService.GetAllWindowsSpeechVoiceCommand(Id);
            //if (? != null)
            //{
            //	ToastService.ShowWarning($"It is not possible to delete a windowsSpeechVoiceCommand that is linked to one or more companies! You would have to delete the companys first. {?.Count()}");
            //	return;
            //}
            var parameters = new ModalParameters();
            if (WindowsSpeechVoiceCommandDataService != null)
            {
                var windowsSpeechVoiceCommand = await WindowsSpeechVoiceCommandDataService.GetWindowsSpeechVoiceCommandById(Id);
                parameters.Add("Title", "Please Confirm, Delete Windows Speech Voice Command");
                parameters.Add("Message", $"SpokenCommand: {windowsSpeechVoiceCommand?.SpokenCommand}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete  Windows Speech Voice Command ({windowsSpeechVoiceCommand?.SpokenCommand})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await WindowsSpeechVoiceCommandDataService.DeleteWindowsSpeechVoiceCommand(Id);
                        ToastService?.ShowSuccess(" Windows Speech Voice Command deleted successfully");
                        await LoadData();
                    }
                }
            }
        }
        async Task EditWindowsSpeechVoiceCommandAsync(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<WindowsSpeechVoiceCommandAddEdit>("Edit Windows Speech Voice Command", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    ApplyFilter(true);
                }
            }
        }
        async Task CopyWindowsSpeechVoiceCommandAsync(int id)
        {
            WindowsSpeechVoiceCommandDTO? original = null;
            if (WindowsSpeechVoiceCommandDataService == null || CustomWindowsSpeechVoiceCommandDataService == null) { return; }
            original = await WindowsSpeechVoiceCommandDataService.GetWindowsSpeechVoiceCommandById(id);
            if (original == null) { return; }
            WindowsSpeechVoiceCommandDTO newCommand = new WindowsSpeechVoiceCommandDTO()
            {
                SpokenCommand = $"Copy of {original.SpokenCommand}",
                Description = original.Description,
                ApplicationName = original.ApplicationName
            };
            WindowsSpeechVoiceCommandDTO result = await WindowsSpeechVoiceCommandDataService.AddWindowsSpeechVoiceCommand(newCommand);
            List<CustomWindowsSpeechCommandDTO> originalChildren = await CustomWindowsSpeechVoiceCommandDataService.GetAllCustomWindowsSpeechCommandsAsync(original.Id);
            foreach (CustomWindowsSpeechCommandDTO item in originalChildren)
            {
                item.Id = 0;
                item.WindowsSpeechVoiceCommandId = result.Id;
                var resultChildren = await CustomWindowsSpeechVoiceCommandDataService.AddCustomWindowsSpeechCommand(item);
            }
            List<SpokenFormDTO> originalSpokenForms = await SpokenFormDataService.GetAllSpokenFormsAsync(original.Id);
            foreach (SpokenFormDTO item in originalSpokenForms)
            {
                item.Id = 0;
                item.WindowsSpeechVoiceCommandId = result.Id;
                var resultSpokenForms = await SpokenFormDataService.AddSpokenForm(item);
            }
            if (NavigationManager != null)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
            }

        }
        private void CreateCommandsDirectly()
        {
            if (CreateCommands != null)
            {
                CreateCommands.CreateCommandsFromList("1to30", "Move Right");
                //CreateCommands.CreateCommandsFromList("1to30", "Move Left");

            }
        }
        private async Task ResetServerFilterAsync()
        {
            ServerSearchTerm = null;
            _hideActions = true;
            await LoadData();
        }
    }
}