
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using DataAccessLibrary.Services;
using DataAccessLibrary.DTO;
using RazorClassLibrary.Shared;
namespace RazorClassLibrary.Pages
{
    public partial class ExampleTable : ComponentBase
    {
        [Inject] public required IExampleDataService ExampleDataService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<ExampleTable>? Logger { get; set; }

        [Inject] public required IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Example Items (Examples)";
        public string EditTitle { get; set; } = "Edit Example Item (Examples)";
        [Parameter] public int ParentId { get; set; }
        public List<ExampleDTO>? ExampleDTO { get; set; }
        public List<ExampleDTO>? FilteredExampleDTO { get; set; }
        protected ExampleAddEdit? ExampleAddEdit { get; set; }
        ElementReference? SearchInput;
        int maximumPages = 0;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; } }
        private string? clientSearchedTerm { get; set; }
        public string? ClientSearchTerm { get => clientSearchedTerm; set { clientSearchedTerm = value; ApplyLocalFilter(); } }
        private bool _serverPaging = false;
        private void ApplyLocalFilter()
        {
            if (FilteredExampleDTO == null || ExampleDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(ClientSearchTerm))
            {
                FilteredExampleDTO = ExampleDTO;
            }
            else
            {
                FilteredExampleDTO = ExampleDTO.Where(v =>
                    (v.Text != null && v.Text.ToLower().Contains(ClientSearchTerm.ToLower()))
                     || (v.LargeText != null && v.LargeText.ToLower().Contains(ClientSearchTerm.ToLower()))

                ).ToList();
            }
            Title = $"Example ({FilteredExampleDTO.Count})";
            StateHasChanged();
        }
        private void OnChangeClientSearchTerm(string? value)
        {
            ClientSearchTerm = value;
            ApplyLocalFilter();
        }
        private string? lastSearchTerm { get; set; }

        [Parameter] public string? ServerSearchTerm { get; set; }
        public string ExceptionMessage { get; set; } = String.Empty;
        public List<string>? PropertyInfo { get; set; }
        [CascadingParameter] public ClaimsPrincipal? User { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }
        public bool ShowEdit { get; set; } = false;
        private bool ShowDeleteConfirm { get; set; }
        private int pageNumber = 1;
        private int pageSize = 1000;
        private int totalRows = 0;

        private int ExampleId { get; set; }
        private ExampleDTO? currentExample { get; set; }
        private string message { get; set; } = "";
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (ExampleDataService != null)
                {
                    ServerSearchTerm = SearchTerm;
                    totalRows = await ExampleDataService.GetTotalCount();
                    maximumPages = (int)Math.Ceiling((decimal)totalRows / pageSize);
                    var result = await ExampleDataService!.GetAllExamplesAsync

                    (pageNumber, pageSize, ServerSearchTerm);
                    //var result = await ExampleDataService.SearchExamplesAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        ExampleDTO = result.ToList();
                        FilteredExampleDTO = result.ToList();
                        StateHasChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.LogError("e, Exception occurred in LoadData Method, Getting Records from the Service");
                _loadFailed = true;
                ExceptionMessage = e.Message;
            }
            FilteredExampleDTO = ExampleDTO;
            Title = $"Example ({FilteredExampleDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await Task.Delay(100);
                    if (SearchInput != null)
                    {
                        await SearchInput.Value.FocusAsync();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        private async Task AddNewExample()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<ExampleAddEdit>("Add Example", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    if (searchTerm != null)
                    {
                        await ApplyFilter();
                    }
                }
            }
            ExampleId = 0;
        }


        private async Task ApplyFilter()
        {
            if (FilteredExampleDTO == null || ExampleDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                await LoadData();
                Title = $"All Example ({FilteredExampleDTO.Count})";
            }
            else
            {
                if (lastSearchTerm != SearchTerm)
                {
                    await LoadData();
                }

            }
            lastSearchTerm = SearchTerm;
        }
        protected void SortExample(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredExampleDTO == null)
            {
                return;
            }
            if (sortColumn == "DateValue")
            {
                FilteredExampleDTO = FilteredExampleDTO.OrderBy(v => v.DateValue).ToList();
            }
            else if (sortColumn == "DateValue Desc")
            {
                FilteredExampleDTO = FilteredExampleDTO.OrderByDescending(v => v.DateValue).ToList();
            }
        }
        private async Task DeleteExample(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (ExampleDataService != null)
            {
                var example = await ExampleDataService.GetExampleById(Id);
                parameters.Add("Title", "Please Confirm, Delete Example");
                parameters.Add("Message", $"DateValue: {example?.DateValue}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Example ({example?.DateValue})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await ExampleDataService.DeleteExample(Id);
                        ToastService?.ShowSuccess("Example deleted successfully");
                        await LoadData();
                        if (searchTerm != null)
                        {
                            await ApplyFilter();
                        }
                    }
                }
            }
            ExampleId = Id;
        }

        private async void EditExample(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<ExampleAddEdit>("Edit Example", parameters);
            if (formModal != null)
            {
                var result = await formModal.Result;
                if (!result.Cancelled)
                {
                    await LoadData();
                    if (searchTerm != null)
                    {
                        await ApplyFilter();
                    }
                }
            }
            ExampleId = Id;
        }

        private async Task OnValueChangedPageSize(int value)
        {
            pageSize = value;
            pageNumber = 1;
            await LoadData();
        }
        private async Task PageUp(bool goBeginning)
        {
            if (goBeginning || pageNumber <= 1)
            {
                pageNumber = 1;
            }
            else
            {
                pageNumber--;
            }
            await LoadData();
        }
        private async Task PageDown(bool goEnd)
        {
            if (goEnd || pageNumber >= maximumPages)
            {
                pageNumber = maximumPages;
            }
            else
            {
                pageNumber++;
            }
            await LoadData();
        }

    }
}