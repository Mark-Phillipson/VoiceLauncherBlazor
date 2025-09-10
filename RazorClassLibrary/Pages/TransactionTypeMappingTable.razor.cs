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
using RazorClassLibrary.Shared;
using DataAccessLibrary.Services;
using DataAccessLibrary.DTOs;
using Microsoft.Extensions.Logging;

namespace RazorClassLibrary.Pages
{
    public partial class TransactionTypeMappingTable : ComponentBase
    {
        [Inject] public required ITransactionTypeMappingDataService TransactionTypeMappingDataService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<TransactionTypeMappingTable>? Logger { get; set; }

        [Inject] public required IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "TransactionTypeMapping Items (TransactionTypeMappings)";
        public string EditTitle { get; set; } = "Edit TransactionTypeMapping Item (TransactionTypeMappings)";
        [Parameter] public int ParentId { get; set; }
        public List<TransactionTypeMappingDTO>? TransactionTypeMappingDTO { get; set; }
        public List<TransactionTypeMappingDTO>? FilteredTransactionTypeMappingDTO { get; set; }
        protected TransactionTypeMappingAddEdit? TransactionTypeMappingAddEdit { get; set; }
        ElementReference SearchInput;
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
            if (FilteredTransactionTypeMappingDTO == null || TransactionTypeMappingDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(ClientSearchTerm))
            {
                FilteredTransactionTypeMappingDTO = TransactionTypeMappingDTO;
            }
            else
            {
                FilteredTransactionTypeMappingDTO = TransactionTypeMappingDTO.Where(v =>
                    (v.MyTransactionType != null && v.MyTransactionType.ToLower().Contains(ClientSearchTerm))
                     || (v.Type != null && v.Type.ToLower().Contains(ClientSearchTerm))

                ).ToList();
            }
            Title = $"TransactionTypeMapping ({FilteredTransactionTypeMappingDTO.Count})";
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

        private int TransactionTypeMappingId { get; set; }
        private TransactionTypeMappingDTO? currentTransactionTypeMapping { get; set; }
        private string? Message { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (TransactionTypeMappingDataService != null)
                {
                    ServerSearchTerm = SearchTerm;
                    totalRows = await TransactionTypeMappingDataService.GetTotalCount();
                    var result = await TransactionTypeMappingDataService!.GetAllTransactionTypeMappingsAsync(pageNumber, pageSize, ServerSearchTerm ?? "");
                    //var result = await TransactionTypeMappingDataService.SearchTransactionTypeMappingsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        TransactionTypeMappingDTO = result.ToList();
                        FilteredTransactionTypeMappingDTO = result.ToList();
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
            FilteredTransactionTypeMappingDTO = TransactionTypeMappingDTO;
            Title = $"Transaction Type Mapping ({FilteredTransactionTypeMappingDTO?.Count})";

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await Task.Delay(100);
                    await SearchInput.FocusAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }
        private async Task AddNewTransactionTypeMapping()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<TransactionTypeMappingAddEdit>("Add Transaction Type Mapping", parameters);
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
            TransactionTypeMappingId = 0;
        }


        private async Task ApplyFilter()
        {
            if (FilteredTransactionTypeMappingDTO == null || TransactionTypeMappingDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                await LoadData();
                Title = $"All Transaction Type Mapping ({FilteredTransactionTypeMappingDTO.Count})";
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
        protected void SortTransactionTypeMapping(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredTransactionTypeMappingDTO == null)
            {
                return;
            }
            if (sortColumn == "MyTransactionType")
            {
                FilteredTransactionTypeMappingDTO = FilteredTransactionTypeMappingDTO.OrderBy(v => v.MyTransactionType).ToList();
            }
            else if (sortColumn == "MyTransactionType Desc")
            {
                FilteredTransactionTypeMappingDTO = FilteredTransactionTypeMappingDTO.OrderByDescending(v => v.MyTransactionType).ToList();
            }
            if (sortColumn == "Type")
            {
                FilteredTransactionTypeMappingDTO = FilteredTransactionTypeMappingDTO.OrderBy(v => v.Type).ToList();
            }
            else if (sortColumn == "Type Desc")
            {
                FilteredTransactionTypeMappingDTO = FilteredTransactionTypeMappingDTO.OrderByDescending(v => v.Type).ToList();
            }
        }
        private async Task DeleteTransactionTypeMapping(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (TransactionTypeMappingDataService != null)
            {
                var transactionTypeMapping = await TransactionTypeMappingDataService.GetTransactionTypeMappingById(Id);
                parameters.Add("Title", "Please Confirm, Delete Transaction Type Mapping");
                parameters.Add("Message", $"MyTransactionType: {transactionTypeMapping?.MyTransactionType}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Transaction Type Mapping ({transactionTypeMapping?.MyTransactionType})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await TransactionTypeMappingDataService.DeleteTransactionTypeMapping(Id);
                        ToastService?.ShowSuccess("Transaction Type Mapping deleted successfully");
                        await LoadData();
                        if (searchTerm != null)
                        {
                            await ApplyFilter();
                        }
                    }
                }
            }
            TransactionTypeMappingId = Id;
        }

        private async void EditTransactionTypeMapping(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<TransactionTypeMappingAddEdit>("Edit Transaction Type Mapping", parameters);
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
            TransactionTypeMappingId = Id;
        }

        private async Task OnValueChangedPageSize(int value)
        {
            pageSize = value;
            pageNumber = 1;
            await LoadData();
        }
        private async Task PageDown(bool goBeginning)
        {
            if (goBeginning || pageNumber <= 0)
            {
                pageNumber = 1;
            }
            else
            {
                pageNumber--;
            }
            await LoadData();
        }
        private async Task PageUp(bool goEnd)
        {
            int maximumPages = (int)Math.Ceiling((decimal)totalRows / pageSize);
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