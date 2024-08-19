
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
using DataAccessLibrary.Services;
using DataAccessLibrary.DTOs;
using Microsoft.Extensions.Logging;
using RazorClassLibrary.Shared;
namespace RazorClassLibrary.Pages
{
    public partial class TransactionTable : ComponentBase
    {
        [Inject] public required ITransactionDataService TransactionDataService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<TransactionTable>? Logger { get; set; }
        [Inject] public required IToastService ToastService { get; set; }
        [CascadingParameter] public IModalService? Modal { get; set; }
        public string Title { get; set; } = "Transaction Items (Transactions)";
        public string EditTitle { get; set; } = "Edit Transaction Item (Transactions)";
        [Parameter] public int ParentId { get; set; }
        public List<TransactionDTO>? TransactionDTO { get; set; }
        public List<TransactionDTO>? FilteredTransactionDTO { get; set; }
        protected TransactionAddEdit? TransactionAddEdit { get; set; }
        [Inject] public required NavigationManager navigationManager { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private string? searchTerm = null;
#pragma warning restore 414, 649
        public string? SearchTerm { get => searchTerm; set { searchTerm = value; } }
        private string? clientSearchedTerm { get; set; }
        public string? ClientSearchTerm { get => clientSearchedTerm; set { clientSearchedTerm = value; ApplyLocalFilter(); } }
        private bool _serverPaging = true;
        private DateTime? FromDate { get; set; } = DateTime.Now.AddMonths(-12);
        private DateTime? ToDate { get; set; } = DateTime.Now;
        private void ApplyLocalFilter()
        {
            if (FilteredTransactionDTO == null || TransactionDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(ClientSearchTerm))
            {
                FilteredTransactionDTO = TransactionDTO;
            }
            if (FromDate.HasValue && ToDate.HasValue)
            {
                FilteredTransactionDTO = FilteredTransactionDTO.Where(v => v.Date >= FromDate && v.Date <= ToDate).ToList();
                if (!string.IsNullOrEmpty(ClientSearchTerm))
                {
                    FilteredTransactionDTO = FilteredTransactionDTO.Where(v =>
                        (v.Description != null && v.Description.ToLower().Contains(ClientSearchTerm))
                         || (v.Type != null && v.Type.ToLower().Contains(ClientSearchTerm))
                         || (v.MyTransactionType != null && v.MyTransactionType.ToLower().Contains(ClientSearchTerm))
                    ).ToList();
                }
            }
            if (!string.IsNullOrEmpty(ClientSearchTerm))
            {
                FilteredTransactionDTO = TransactionDTO.Where(v =>
                    (v.Description != null && v.Description.ToLower().Contains(ClientSearchTerm))
                     || (v.Type != null && v.Type.ToLower().Contains(ClientSearchTerm))
                     || (v.MyTransactionType != null && v.MyTransactionType.ToLower().Contains(ClientSearchTerm))

                ).ToList();
            }
            Title = $"Transaction ({FilteredTransactionDTO.Count})";
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
        private int pageSize = 200;
        private int totalRows = 0;

        private int TransactionId { get; set; }
        private TransactionDTO? currentTransaction { get; set; }
        private string? Message { get; set; }
        protected override async Task OnInitializedAsync()
        {
            if (Environment.MachineName != "J40L4V3")
            {
                navigationManager.NavigateTo(navigationManager.BaseUri);
            }
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                if (TransactionDataService != null)
                {
                    ServerSearchTerm = SearchTerm;
                    totalRows = await TransactionDataService.GetTotalCount();
                    var result = await TransactionDataService!.GetAllTransactionsAsync(pageNumber, pageSize, ServerSearchTerm);
                    //var result = await TransactionDataService.SearchTransactionsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        TransactionDTO = result.ToList();
                        FilteredTransactionDTO = result.ToList();
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
            FilteredTransactionDTO = TransactionDTO;
            Title = $"Transaction ({FilteredTransactionDTO?.Count})";

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
        private async Task AddNewTransaction()
        {
            var parameters = new ModalParameters();
            var formModal = Modal?.Show<TransactionAddEdit>("Add Transaction", parameters);
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
            TransactionId = 0;
        }


        private async Task ApplyFilter()
        {
            if (FilteredTransactionDTO == null || TransactionDTO == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(SearchTerm))
            {
                await LoadData();
                Title = $"All Transaction ({FilteredTransactionDTO.Count})";
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
        protected void SortTransaction(string sortColumn)
        {
            Guard.Against.Null(sortColumn, nameof(sortColumn));
            if (FilteredTransactionDTO == null)
            {
                return;
            }
            if (sortColumn == "Date")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.Date).ToList();
            }
            else if (sortColumn == "Date Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.Date).ToList();
            }
            if (sortColumn == "Description")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.Description).ToList();
            }
            else if (sortColumn == "Description Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.Description).ToList();
            }
            if (sortColumn == "Type")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.Type).ToList();
            }
            else if (sortColumn == "Type Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.Type).ToList();
            }
            if (sortColumn == "MoneyIn")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.MoneyIn).ToList();
            }
            else if (sortColumn == "MoneyIn Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.MoneyIn).ToList();
            }
            if (sortColumn == "MoneyOut")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.MoneyOut).ToList();
            }
            else if (sortColumn == "MoneyOut Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.MoneyOut).ToList();
            }
            if (sortColumn == "MyTransactionType")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderBy(v => v.MyTransactionType).ToList();
            }
            else if (sortColumn == "MyTransactionType Desc")
            {
                FilteredTransactionDTO = FilteredTransactionDTO.OrderByDescending(v => v.MyTransactionType).ToList();
            }
        }
        private async Task DeleteTransaction(int Id)
        {
            //TODO Optionally remove child records here or warn about their existence
            var parameters = new ModalParameters();
            if (TransactionDataService != null)
            {
                var transaction = await TransactionDataService.GetTransactionById(Id);
                parameters.Add("Title", "Please Confirm, Delete Transaction");
                parameters.Add("Message", $"Date: {transaction?.Date}");
                parameters.Add("ButtonColour", "danger");
                parameters.Add("Icon", "fa fa-trash");
                var formModal = Modal?.Show<BlazoredModalConfirmDialog>($"Delete Transaction ({transaction?.Date})?", parameters);
                if (formModal != null)
                {
                    var result = await formModal.Result;
                    if (!result.Cancelled)
                    {
                        await TransactionDataService.DeleteTransaction(Id);
                        ToastService?.ShowSuccess("Transaction deleted successfully");
                        await LoadData();
                        if (searchTerm != null)
                        {
                            await ApplyFilter();
                        }
                    }
                }
            }
            TransactionId = Id;
        }

        private async void EditTransaction(int Id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", Id);
            var formModal = Modal?.Show<TransactionAddEdit>("Edit Transaction", parameters);
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
            TransactionId = Id;
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
        private async Task HandleFileSelection(InputFileChangeEventArgs e)
        {

            var file = e.File;
            if (file != null)
            {
                var reader = new StreamReader(file.OpenReadStream());
                var fileContent = await reader.ReadToEndAsync();
                var transactions = await TransactionDataService.ImportTransactions(fileContent, file.Name);
                transactions = await TransactionDataService.ProcessTransactions(transactions);
                if (transactions != null)
                {
                    int result = await TransactionDataService.AddTransactions(transactions);
                    await LoadData();

                }
            }
        }
        private void BreakdownByMyTransactionType()
        {
            if (FilteredTransactionDTO == null)
            {
                return;
            }
            var myTransactionTypes = FilteredTransactionDTO.Select(v => v.MyTransactionType).Distinct().ToList();
            var breakdown = new List<TransactionDTO>();
            foreach (var myTransactionType in myTransactionTypes)
            {
                var totalMoneyIn = FilteredTransactionDTO.Where(v => v.MyTransactionType == myTransactionType).Sum(v => v.MoneyIn);
                var totalMoneyOut = FilteredTransactionDTO.Where(v => v.MyTransactionType == myTransactionType).Sum(v => v.MoneyOut);
                var totalTransactions = FilteredTransactionDTO.Where(v => v.MyTransactionType == myTransactionType).Count();
                var transaction = new TransactionDTO
                {
                    Date = null,
                    Type = "--------",
                    MyTransactionType = myTransactionType,
                    MoneyIn = totalMoneyIn,
                    MoneyOut = totalMoneyOut,
                    Description = $"Total Transactions: {totalTransactions}"
                };
                breakdown.Add(transaction);
            }
            FilteredTransactionDTO = breakdown;
            Title = $"Transaction Breakdown by MyTransactionType ({FilteredTransactionDTO.Count})";
            StateHasChanged();
        }
    }
}