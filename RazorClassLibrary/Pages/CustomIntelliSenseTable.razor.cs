
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using DataAccessLibrary.DTO;
using System.Security.Claims;
using Ardalis.GuardClauses;
using VoiceLauncher.Shared;
using VoiceLauncher.Services;
using Microsoft.Extensions.Logging;

namespace VoiceLauncher.Pages
{
    public partial class CustomIntelliSenseTable : ComponentBase
    {
        [Inject] public ICustomIntelliSenseDataService? CustomIntelliSenseDataService { get; set; }
        [Inject] public NavigationManager? NavigationManager { get; set; }
        [Inject] public ILogger<CustomIntelliSenseTable>? Logger { get; set; }
        
        [Inject] public IToastService? ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "CustomIntelliSense Items (CustomIntelliSenses)";
        public string EditTitle { get; set; } = "Edit CustomIntelliSense Item (CustomIntelliSenses)";
		
        [Parameter] public int CategoryId { get; set; }
        [Parameter] public int ParentId { get; set; }
        public List<CustomIntelliSenseDTO>? CustomIntelliSenseDTO { get; set; }
        public List<CustomIntelliSenseDTO>? FilteredCustomIntelliSenseDTO { get; set; }
        protected CustomIntelliSenseAddEdit? CustomIntelliSenseAddEdit { get; set; }
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
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int CustomIntelliSenseId  { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (CustomIntelliSenseDataService != null)
                {
                    if (CategoryId==0) {
                        CategoryId = 34;
                    }
                    var result = await CustomIntelliSenseDataService!.GetAllCustomIntelliSensesAsync(CategoryId);
                    //var result = await CustomIntelliSenseDataService.SearchCustomIntelliSensesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        CustomIntelliSenseDTO = result.ToList();
                        FilteredCustomIntelliSenseDTO = result.ToList();
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError("Exception occurred in LoadData Method, Getting Records from the Service", e);
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredCustomIntelliSenseDTO = CustomIntelliSenseDTO;
            Title = $"Custom Intelli Sense ({FilteredCustomIntelliSenseDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    if (JSRuntime!= null )
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
        private async Task AddNewCustomIntelliSense()
        {
              var parameters = new ModalParameters();
			
              parameters.Add(nameof(CategoryId), CategoryId);
              var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Add Custom Intelli Sense", parameters);
              if (formModal != null)
              {
                  var result = await formModal.Result;
                  if (!result.Cancelled)
                  {
                      await LoadData();
                  }
              }
              CustomIntelliSenseId=0;
        }


        private void ApplyFilter()
        {
            if (FilteredCustomIntelliSenseDTO == null || CustomIntelliSenseDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredCustomIntelliSenseDTO = CustomIntelliSenseDTO.OrderBy(v => v.DisplayValue).ToList();
                Title = $"All Custom Intelli Sense ({FilteredCustomIntelliSenseDTO.Count})";
            }
            else
            {
                var temporary = SearchTerm.ToLower().Trim();
                FilteredCustomIntelliSenseDTO = CustomIntelliSenseDTO
                    .Where(v => 
                    (v.DisplayValue!= null  && v.DisplayValue.ToLower().Contains(temporary))
                     || (v.SendKeysValue!= null  &&  v.SendKeysValue.ToLower().Contains(temporary))
                     || (v.CommandType!= null  &&  v.CommandType.ToLower().Contains(temporary))
                     || (v.DeliveryType!= null  &&  v.DeliveryType.ToLower().Contains(temporary))
                    )
                    .ToList();
                Title = $"Filtered Custom Intelli Senses ({FilteredCustomIntelliSenseDTO.Count})";
            }
        }
        protected void SortCustomIntelliSense(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
                        if (FilteredCustomIntelliSenseDTO == null)
            {
                return;
            }
            if (sortColumn == "DisplayValue")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.DisplayValue).ToList();
            }
            else if (sortColumn == "DisplayValue Desc")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.DisplayValue).ToList();
            }
            if (sortColumn == "SendKeysValue")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.SendKeysValue).ToList();
            }
            else if (sortColumn == "SendKeysValue Desc")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.SendKeysValue).ToList();
            }
            if (sortColumn == "CommandType")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.CommandType).ToList();
            }
            else if (sortColumn == "CommandType Desc")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.CommandType).ToList();
            }
            if (sortColumn == "DeliveryType")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderBy(v => v.DeliveryType).ToList();
            }
            else if (sortColumn == "DeliveryType Desc")
            {
                FilteredCustomIntelliSenseDTO = FilteredCustomIntelliSenseDTO.OrderByDescending(v => v.DeliveryType).ToList();
            }
        }
        private async Task DeleteCustomIntelliSense(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
              var parameters = new ModalParameters();
              if (CustomIntelliSenseDataService != null)
              {
                  var customIntelliSense = await CustomIntelliSenseDataService.GetCustomIntelliSenseById(Id);
                  parameters.Add("Title", "Please Confirm, Delete Custom Intelli Sense");
                  parameters.Add("Message", $"DisplayValue: {customIntelliSense?.DisplayValue}");
                  parameters.Add("ButtonColour", "danger");
                  parameters.Add("Icon", "fa fa-trash");
                  var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Custom Intelli Sense ({customIntelliSense?.DisplayValue})?", parameters);
                  if (formModal != null)
                  {
                      var result = await formModal.Result;
                      if (!result.Cancelled)
                      {
                          await CustomIntelliSenseDataService.DeleteCustomIntelliSense(Id);
                          ToastService?.ShowSuccess("Custom Intelli Sense deleted successfully", "SUCCESS");
                          await LoadData();
                      }
                  }
             }
             CustomIntelliSenseId = Id;
        }
                  
        private async void EditCustomIntelliSense(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<CustomIntelliSenseAddEdit>("Edit Custom Intelli Sense", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                }
            }
            CustomIntelliSenseId = Id;
        }
            
    }
}