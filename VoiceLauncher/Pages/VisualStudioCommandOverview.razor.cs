using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VoiceLauncher.Shared;

namespace VoiceLauncher.Pages
{
	public partial class VisualStudioCommandOverview
	{
		[Inject] public VisualStudioCommandService VisualStudioCommandService { get; set; }
		[Inject] NavigationManager NavigationManager { get; set; }
		[Inject] public ILogger<VisualStudioCommandOverview> Logger { get; set; }
		[Inject] public IToastService ToastService { get; set; }
		[CascadingParameter] public IModalService Modal { get; set; }
		[CascadingParameter] public ClaimsPrincipal User { get; set; }
		ElementReference SearchInput;
		public string Title { get; set; } = "Visual Studio Commands";
		private string searchTerm;
		public string SearchTerm { get => searchTerm; set { searchTerm = value; ApplyFilter(); } }

		public List<VisualStudioCommand> VisualStudioCommands { get; set; }
		public List<VisualStudioCommand> FilteredVisualStudioCommands { get; set; }
		//protected AddVisualStudioCommand AddVisualStudioCommand { get; set; }
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
			var query = new Uri(NavigationManager.Uri).Query;
			string caption = null;
			if (QueryHelpers.ParseQuery(query).TryGetValue("caption", out var captionQuery))
			{
				caption = captionQuery;
			}
			try
			{
				if (caption != null)
				{
					VisualStudioCommands = (await VisualStudioCommandService.GetVisualStudioCommandsAsync(caption)).ToList();
				}
				else
				{
					VisualStudioCommands = (await VisualStudioCommandService.GetVisualStudioCommandsAsync()).ToList();
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Exception occurred in on initialised async VisualStudioCommand Service", e);
				_loadFailed = true;
				ExceptionMessage = e.Message;
				ToastService.ShowError(e.Message, "Error Loading VisualStudioCommand");
			}
			FilteredVisualStudioCommands = VisualStudioCommands;

		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await SearchInput.FocusAsync();

			}
		}
		//protected async Task AddNewVisualStudioCommandAsync()
		//{
		//	var parameters = new ModalParameters();
		//	parameters.Add(nameof(User), User);
		//	parameters.Add(nameof(CustomIntelliSenseId), CustomIntelliSenseId);
		//	var formModal = Modal.Show<AddVisualStudioCommand>("Add VisualStudio Command", parameters);
		//	var result = await formModal.Result;
		//	if (!result.Cancelled)
		//	{
		//		await LoadData();
		//	}
		//}
		private void ApplyFilter()
		{
			if (string.IsNullOrEmpty(SearchTerm))
			{
				FilteredVisualStudioCommands = VisualStudioCommands.OrderBy(v => v.Caption).ToList();
				Title = $"All Visual Studio Commands ({FilteredVisualStudioCommands.Count})";
			}
			else
			{
				FilteredVisualStudioCommands = VisualStudioCommands.Where(v => v.Caption.ToLower().Contains(SearchTerm.Trim().ToLower())).ToList();
				Title = $"Filtered Visual Studio Commands ({FilteredVisualStudioCommands.Count})";
			}
		}
		protected void SortVisualStudioCommands(string sortColumn)
		{
			if (sortColumn == "Caption")
			{
				FilteredVisualStudioCommands = FilteredVisualStudioCommands.OrderBy(o => o.Caption).ToList();
			}
			else if (sortColumn == "Command")
			{
				FilteredVisualStudioCommands = FilteredVisualStudioCommands.OrderBy(o => o.Command).ToList();
			}
		}
		async Task DeleteVisualStudioCommandAsync(int additionalCommandId)
		{
			var parameters = new ModalParameters();
			parameters.Add("Title", "Please Confirm");
			parameters.Add("Message", "Do you really wish to delete this VisualStudioCommand?");
			parameters.Add("ButtonColour", "danger");
			var additionalCommand = await VisualStudioCommandService.GetVisualStudioCommandAsync(additionalCommandId);
			var formModal = Modal.Show<BlazoredModalConfirmDialog>($"Delete VisualStudio Command: {additionalCommand?.Caption}?", parameters);
			var result = await formModal.Result;
			if (!result.Cancelled)
			{
				try
				{
					await VisualStudioCommandService.DeleteVisualStudioCommand(additionalCommandId);
				}
				catch (Exception)
				{
					throw;
				}
				ToastService.ShowSuccess($"{additionalCommand.Caption} Successfully deleted.", "Delete VisualStudioCommand");
				await LoadData();
			}
		}
		//async Task EditVisualStudioCommandAsync(int additionalCommandId)
		//{
		//	var parameters = new ModalParameters();
		//	parameters.Add(nameof(User), User);
		//	parameters.Add(nameof(CustomIntelliSenseId), CustomIntelliSenseId);
		//	parameters.Add(nameof(additionalCommandId), additionalCommandId);
		//	var formModal = Modal.Show<AddVisualStudioCommand>("Edit VisualStudio Command", parameters);
		//	var result = await formModal.Result;
		//	if (!result.Cancelled)
		//	{
		//		await LoadData();
		//	}

		//}
	}
}
