
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
        public List<TransactionDTO>? HouseholdTransactions { get; set; }
        public List<TransactionDTO>? HouseholdTransactionsPrevious { get; set; }
        protected TransactionAddEdit? TransactionAddEdit { get; set; }
        private bool showingBreakdown = false;
        [Inject] public required NavigationManager navigationManager { get; set; }
        ElementReference SearchInput;
#pragma warning disable 414, 649
        private bool _loadFailed = false;
        private bool SubtotalsOnly
        {
            get => subtotalsOnly; set
            {
                subtotalsOnly = value;
                if (subtotalsOnly && FilteredTransactionDTO != null)
                {
                    FilteredTransactionDTO = FilteredTransactionDTO.Where(v => v.MyTransactionType == "SUBTOTAL").ToList();
                }
                else
                {
                    HouseholdChargesReport();
                }
            }
        }
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
        private int pageSize = 500;
        private int totalRows = 0;
        private string informationMessage = "When downloading the transactions from the bank you will have set the date filter and make them all appear on the page and then scroll to the bottom and then click download. This will download all the available transactions to a CSV file.";
        private bool subtotalsOnly = false;

        private int TransactionId { get; set; }
        private TransactionDTO? currentTransaction { get; set; }
        private string errorMessage { get; set; } = String.Empty;
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
                    var result = await TransactionDataService!.GetAllTransactionsAsync(pageNumber, pageSize, ServerSearchTerm ?? "");
                    //var result = await TransactionDataService.SearchTransactionsAsync(ServerSearchTerm);
                    if (result != null)
                    {
                        TransactionDTO = result.ToList();
                        FilteredTransactionDTO = result.ToList();
                        showingBreakdown = false;
                        FilterHouseholdTransactions();
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

        private void FilterHouseholdTransactions()
        {
            if (TransactionDTO == null)
            {
                return;
            }
            HouseholdTransactions = TransactionDTO
                .Where(v => v.MyTransactionType == "Household" && v.Date.HasValue && v.Date.Value.Year == DateTime.Now.Year)
                .GroupBy(v => new { v.Date?.Year, v.Date?.Month })
                .Select(g => new TransactionDTO
                {
                    Date = g.Key.Year.HasValue && g.Key.Month.HasValue ? new DateTime((int)g.Key.Year, (int)g.Key.Month, 1) : DateTime.MinValue,
                    MoneyOut = g.Sum(t => t.MoneyOut),
                    Description = $"{(g.Key.Month < 10 ? "0" + g.Key.Month.ToString() : g.Key.Month.ToString())}"
                })
                .OrderBy(x => x.Description)
                .ToList();
            HouseholdTransactionsPrevious = TransactionDTO
                            .Where(v => v.MyTransactionType == "Household" && v.Date.HasValue && v.Date.Value.Year == DateTime.Now.Year - 1)
                            .GroupBy(v => new { v.Date?.Year, v.Date?.Month })
                            .Select(g => new TransactionDTO
                            {
                                Date = g.Key.Year.HasValue && g.Key.Month.HasValue ? new DateTime((int)g.Key.Year, (int)g.Key.Month, 1) : DateTime.MinValue,
                                MoneyOut = g.Sum(t => t.MoneyOut),
                                Description = $"{(g.Key.Month < 10 ? "0" + g.Key.Month.ToString() : g.Key.Month.ToString())}"
                            })
                            .OrderBy(x => x.Description)
                            .ToList();
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
                var importedTransactions = await TransactionDataService.ImportTransactions(fileContent, file.Name);
                errorMessage = "";
                if (importedTransactions.Errors.Count > 0)
                {
                    foreach (var error in importedTransactions.Errors)
                    {
                        errorMessage += error + Environment.NewLine;
                    }
                    return;
                }
                var processedTransactions = await TransactionDataService.ProcessTransactions(importedTransactions.Transactions);
                if (processedTransactions != null)
                {
                    int result = await TransactionDataService.AddTransactions(processedTransactions);
                    informationMessage = $"{result} Transactions added successfully";
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
            Title = $"My Transaction Type ({FilteredTransactionDTO.Count})";
            showingBreakdown = true;
            StateHasChanged();
        }
        private void HouseholdChargesReport()
        {
            // The report should break down each individual household charge for each month giving a total for each month
            var householdCharges = FilteredTransactionDTO?.Where(v => v.MyTransactionType == "Household").ToList();
            if (householdCharges == null)
            {
                return;
            }
            int sortOrder = 0; string lastMonthYear = "";
            var uniqueMonthYearDescriptions = householdCharges
                .Select(v => new { Month = v.Date?.Month, Year = v.Date?.Year, v.Description })
                .Distinct()
                .ToList();
            var breakdown = new List<TransactionDTO>();
            object? lastMonth = null;
            decimal totalMoneyOut = 0; decimal totalMoneyIn = 0;
            decimal subtotalmoneyIn = 0; decimal subtotalmoneyOut = 0;
            foreach (var selectedMonth in uniqueMonthYearDescriptions.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Description))
            {
                if (lastMonthYear != $"{selectedMonth.Month}/{selectedMonth.Year}" && lastMonthYear != "" && lastMonth != null)
                {
                    AddSubBreakdown(householdCharges, ref sortOrder, breakdown, ref subtotalmoneyIn, ref subtotalmoneyOut, lastMonth);
                }
                sortOrder++;
                totalMoneyIn = householdCharges.Where(v => v.Date?.Month != null && v.Date?.Month == selectedMonth.Month && v.Date?.Year == selectedMonth.Year && selectedMonth.Description == v.Description).Sum(v => v.MoneyIn);
                totalMoneyOut = householdCharges.Where(v => v.Date?.Month == selectedMonth.Month && v.Date?.Year == selectedMonth.Year && selectedMonth.Description == v.Description).Sum(v => v.MoneyOut);
                var totalTransactions = householdCharges.Where(v => v.Date?.Month == selectedMonth.Month && v.Date?.Year == selectedMonth.Year).Count();
                var transaction = new TransactionDTO
                {
                    Id = sortOrder,
                    Date = selectedMonth.Year != null && selectedMonth.Month != null ? new DateTime((int)selectedMonth.Year, (int)selectedMonth.Month, 1) : null,
                    Type = householdCharges.FirstOrDefault(v => v.Date?.Month == selectedMonth.Month && v.Date?.Year == selectedMonth.Year && v.Description == selectedMonth.Description)?.Type,
                    MyTransactionType = "Household",
                    MoneyIn = totalMoneyIn,
                    MoneyOut = totalMoneyOut,
                    Description = selectedMonth.Description
                };
                breakdown.Add(transaction);
                subtotalmoneyIn += totalMoneyIn;
                subtotalmoneyOut += totalMoneyOut;
                lastMonthYear = $"{selectedMonth.Month}/{selectedMonth.Year}";
                lastMonth = selectedMonth;
            }
            if (lastMonth != null)
            {
                AddSubBreakdown(householdCharges, ref sortOrder, breakdown, ref subtotalmoneyIn, ref subtotalmoneyOut, lastMonth);
            }
            FilteredTransactionDTO = breakdown.OrderBy(x => x.Id).ToList();
            Title = $"Household Charges ({FilteredTransactionDTO.Count})";
            showingBreakdown = true;
            // StateHasChanged();
        }

        private static void AddSubBreakdown(List<TransactionDTO>? householdCharges, ref int sortOrder, List<TransactionDTO> breakdown, ref decimal subtotalmoneyIn, ref decimal subtotalmoneyOut, object month)
        {
            // Add a sub breakdown at the end of each month
            sortOrder++;
            var subBreakdown = householdCharges?.Where(v => ((dynamic)month).Month == v.Date?.Month && ((dynamic)month).Year == v.Date?.Year).ToList();
            var subtotalTransaction = new TransactionDTO
            {
                Id = sortOrder,
                Date = null,//month.Year != null && month.Month != null ? new DateTime((int)month.Year, (int)month.Month, 1) : null,
                Type = "",
                MyTransactionType = "SUBTOTAL",
                MoneyIn = subtotalmoneyIn,
                MoneyOut = subtotalmoneyOut,
                Description = $"SUBTOTAL {((dynamic)month).Month}/{((dynamic)month).Year}"
            };
            subtotalmoneyIn = 0;
            subtotalmoneyOut = 0;
            breakdown.Add(subtotalTransaction);
        }
        private void ResetBreakdown()
        {
            FilteredTransactionDTO = TransactionDTO;
            Title = $"Transaction ({FilteredTransactionDTO?.Count ?? 0})";
            showingBreakdown = false;
        }
        private void ExportToCsv()
        {
            if (FilteredTransactionDTO == null)
            {
                return;
            }
            var data = FilteredTransactionDTO;
            var fileName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var excelService = new ExcelService();
            informationMessage = excelService.ExportTransactionsToExcel(data, fileName);
        }
    }
}