using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoiceLauncher.Shared;
using VoiceLauncher.Components;

namespace VoiceLauncher.Pages
{
    public partial class AdditionalCommandOverview
    {
        [Inject] public AdditionalCommandService AdditionalCommandService { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<AdditionalCommandOverview> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        [Parameter] public int CustomIntelliSenseId { get; set; }

        public string Title { get; set; } = "Additional Commands";
        private string searchTerm;
        public string SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }

        public List<AdditionalCommand> AdditionalCommands { get; set; }
        public List<AdditionalCommand> FilteredAdditionalCommands { get; set; }
        protected AddAdditionalCommand AddAdditionalCommand { get; set; }
#pragma warning disable 414, 649
        private bool _loadFailed = false;
#pragma warning restore 414, 649
        public string ExceptionMessage { get; set; } = String.Empty;
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        async Task LoadData()
        {
            try
            {
                AdditionalCommands = (await AdditionalCommandService.GetAdditionalCommandsAsync(CustomIntelliSenseId)).ToList();
            }
            catch (Exception e)
            {
                Logger.LogError("Exception occurred in on initialised async AdditionalCommand Data Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
                ToastService.ShowError(e.Message, "Error Loading AdditionalCommand");
            }
            FilteredAdditionalCommands = AdditionalCommands;

        }
        protected async Task AddNewAdditionalCommandAsync()
        {
            var parameters = new ModalParameters();

            parameters.Add(nameof(CustomIntelliSenseId), CustomIntelliSenseId);
            if (Modal != null)
            {
                var formModal = Modal.Show<AddAdditionalCommand>("Add Additional Command", parameters);
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
        }
        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredAdditionalCommands = AdditionalCommands.OrderBy(v => v.DeliveryType).ToList();
                Title = $"All AdditionalCommands ({FilteredAdditionalCommands.Count})";
            }
            else
            {
                FilteredAdditionalCommands = AdditionalCommands.Where(v => v.DeliveryType.ToLower().Contains(SearchTerm.Trim().ToLower())).ToList();
                Title = $"Filtered AdditionalCommands ({FilteredAdditionalCommands.Count})";
            }
        }
        protected void SortAdditionalCommands(string sortColumn)
        {
            if (sortColumn == "Delivery Type")
            {
                FilteredAdditionalCommands = FilteredAdditionalCommands.OrderBy(o => o.DeliveryType).ToList();
            }
        }
        async Task DeleteAdditionalCommandAsync(int additionalCommandId)
        {
            var parameters = new ModalParameters();
            parameters.Add("Title", "Please Confirm");
            parameters.Add("Message", "Do you really wish to delete this AdditionalCommand?");
            parameters.Add("ButtonColour", "danger");
            var additionalCommand = await AdditionalCommandService.GetAdditionalCommandAsync(additionalCommandId);
            var formModal = Modal.Show<BlazoredModalConfirmDialog>($"Delete Additional Command: {additionalCommand?.SendKeysValue}?", parameters);
            var result = await formModal.Result;
            if (!result.Cancelled)
            {
                try
                {
                    await AdditionalCommandService.DeleteAdditionalCommand(additionalCommandId);
                }
                catch (Exception)
                {
                    throw;
                }
                ToastService.ShowSuccess($"{additionalCommand.SendKeysValue} Successfully deleted.", "Delete AdditionalCommand");
                await LoadData();
            }
        }
        async Task EditAdditionalCommandAsync(int additionalCommandId)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(CustomIntelliSenseId), CustomIntelliSenseId);
            parameters.Add(nameof(additionalCommandId), additionalCommandId);
            var formModal = Modal.Show<AddAdditionalCommand>("Edit Additional Command", parameters);
            var result = await formModal.Result;
            if (!result.Cancelled)
            {
                await LoadData();
            }
        }
    }
}
