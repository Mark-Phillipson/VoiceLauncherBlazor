using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Services;

namespace RazorClassLibrary.Pages
{
    public partial class IncomeExpenseReport : ComponentBase
    {
        [Inject] public required ITransactionDataService TransactionDataService { get; set; }
        [Inject] public required NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<IncomeExpenseReport>? Logger { get; set; }
        [Inject] public IJSRuntime? JSRuntime { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public class ReportRow
        {
            public string Category { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public decimal TenPercent { get; set; }
            public decimal BusinessPortion { get; set; }
            public bool IsExpanded { get; set; }
            public List<TransactionDTO> Transactions { get; set; } = new List<TransactionDTO>();
        }

        public List<ReportRow>? ExpenseRows { get; set; }
        public List<ReportRow>? IncomeRows { get; set; }

        public decimal TotalExpenses { get; set; }
        public decimal TotalExpensesTax { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalBusinessPortion { get; set; }

        // Editable percentages (UI-bound)
        public decimal HouseholdTenPercent { get; set; } = 10m; // percent value e.g., 10 = 10%
        public decimal SkyBusinessPercent { get; set; } = 50m;
        public decimal GiffgaffBusinessPercent { get; set; } = 50m;

        protected override void OnInitialized()
        {
            // default date range: last completed UK tax year (6 April .. 5 April)
            var today = DateTime.Today;
            var currentTaxYearStart = new DateTime(today.Year, 4, 6);
            if (today < currentTaxYearStart)
            {
                currentTaxYearStart = currentTaxYearStart.AddYears(-1);
            }
            var lastTaxYearStart = currentTaxYearStart.AddYears(-1);
            var lastTaxYearEnd = currentTaxYearStart.AddDays(-1); // 5 April of currentTaxYearStart year
            FromDate = lastTaxYearStart;
            ToDate = lastTaxYearEnd;
        }

        private async Task GenerateReport()
        {
            try
            {
                var all = await TransactionDataService.GetAllTransactionsAsync(1, 10000, "");
                var list = all ?? new List<TransactionDTO>();

                // apply date filter
                if (FromDate.HasValue && ToDate.HasValue)
                {
                    list = list.Where(t => t.Date >= FromDate && t.Date <= ToDate).ToList();
                }

                // exclude L Phillipson rent payments entirely
                list = list.Where(t => !(t.Description != null && t.Description.StartsWith("L Phillipson", StringComparison.OrdinalIgnoreCase))).ToList();

                // Expenses
                var expenses = list.Where(t => t.MoneyOut > 0).ToList();
                var expenseRows = new List<ReportRow>();

                // Business Expense
                var business = expenses.Where(t => t.MyTransactionType == "Business Expense").ToList();
                expenseRows.Add(new ReportRow
                {
                    Category = "Business Expense",
                    Total = business.Sum(t => t.MoneyOut),
                    TenPercent = 0m,
                    BusinessPortion = business.Sum(t => t.MoneyOut),
                    Transactions = business.OrderByDescending(t => t.Date).ToList()
                });

                // Household
                var household = expenses.Where(t => t.MyTransactionType == "Household").ToList();
                expenseRows.Add(new ReportRow
                {
                    Category = "Household",
                    Total = household.Sum(t => t.MoneyOut),
                    TenPercent = household.Sum(t => t.MoneyOut) * (HouseholdTenPercent / 100m),
                    BusinessPortion = 0m,
                    Transactions = household.OrderByDescending(t => t.Date).ToList()
                });

                // Other Expenses
                var otherExpenses = expenses.Where(t => t.MyTransactionType != "Business Expense" && t.MyTransactionType != "Household").ToList();
                // compute allocations for certain other expenses (hard-coded per-report overrides)
                // Sky broadband -> 50% business
                // giffgaff mobile -> 50% business
                var otherBusinessAllocations = otherExpenses.Select(t => new
                {
                    Transaction = t,
                    Allocation = (t.Description != null && t.Description.IndexOf("sky", StringComparison.OrdinalIgnoreCase) >= 0) ? t.MoneyOut * (SkyBusinessPercent / 100m) :
                                 (t.Description != null && (t.Description.IndexOf("giffgaff", StringComparison.OrdinalIgnoreCase) >= 0 || t.Description.IndexOf("giff gaff", StringComparison.OrdinalIgnoreCase) >= 0)) ? t.MoneyOut * (GiffgaffBusinessPercent / 100m) : 0m
                }).ToList();

                var otherBusinessTotal = otherBusinessAllocations.Sum(x => x.Allocation);

                expenseRows.Add(new ReportRow
                {
                    Category = "Other Expenses",
                    Total = otherExpenses.Sum(t => t.MoneyOut),
                    TenPercent = 0m,
                    BusinessPortion = otherBusinessTotal,
                    Transactions = otherExpenses.OrderByDescending(t => t.Date).ToList()
                });

                ExpenseRows = expenseRows.Where(r => r.Total != 0 || (r.Transactions != null && r.Transactions.Count > 0)).ToList();
                TotalExpenses = ExpenseRows.Sum(r => r.Total);
                TotalExpensesTax = ExpenseRows.Sum(r => r.TenPercent);
                // total business portion to be included for tax return
                TotalBusinessPortion = ExpenseRows.Sum(r => r.BusinessPortion);

                // Income
                var incomes = list.Where(t => t.MoneyIn > 0).ToList();

                // Heuristics
                string[] refundKeywords = new[] { "REFUND", "REVERSAL", "RETURN", "REIMB", "REIMBURSE", "REVERSED" };
                Func<TransactionDTO, bool> isRefund = t => t.Description != null && refundKeywords.Any(k => t.Description.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

                // Pension detection: PGO TAX prefix OR contains ARMY/PENSION/MNTNLY/MNTLY
                Func<TransactionDTO, bool> isPension = t =>
                    t.Description != null && (
                        t.Description.StartsWith("PGO TAX", StringComparison.OrdinalIgnoreCase)
                        || t.Description.IndexOf("ARMY", StringComparison.OrdinalIgnoreCase) >= 0
                        || t.Description.IndexOf("PENSION", StringComparison.OrdinalIgnoreCase) >= 0
                        || t.Description.IndexOf("MNTNLY", StringComparison.OrdinalIgnoreCase) >= 0
                        || t.Description.IndexOf("MNTLY", StringComparison.OrdinalIgnoreCase) >= 0
                    );

                var pension = incomes.Where(isPension).ToList();
                // Other income: everything else (but keep refunds in the transactions list)
                var otherIncomeTransactions = incomes.Where(t => !isPension(t)).OrderByDescending(t => t.Date).ToList();

                var incomeRows = new List<ReportRow>();
                incomeRows.Add(new ReportRow
                {
                    Category = "Pension",
                    Total = pension.Sum(t => t.MoneyIn),
                    Transactions = pension.OrderByDescending(t => t.Date).ToList()
                });

                // For Other Income total, exclude refunds from the summed total but show them in the drill-down
                var otherIncomeTotal = otherIncomeTransactions.Where(t => !isRefund(t)).Sum(t => t.MoneyIn);

                incomeRows.Add(new ReportRow
                {
                    Category = "Other Income",
                    Total = otherIncomeTotal,
                    Transactions = otherIncomeTransactions
                });

                IncomeRows = incomeRows.Where(r => r.Total != 0 || (r.Transactions != null && r.Transactions.Count > 0)).ToList();
                TotalIncome = IncomeRows.Sum(r => r.Total);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error generating income/expense report");
            }
        }

        private void ToggleExpand(ReportRow row)
        {
            row.IsExpanded = !row.IsExpanded;
        }

        public async Task ExportCsv()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Section,Category,Total,10%,BusinessPortion");
                if (ExpenseRows != null)
                {
                        foreach (var r in ExpenseRows)
                        {
                            sb.Append('"').Append("Expenses").Append('"').Append(',');
                            sb.Append('"').Append(r.Category.Replace('"',' ')).Append('"').Append(',');
                            sb.Append(r.Total.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',');
                            sb.Append(r.TenPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(',');
                            sb.Append(r.BusinessPortion.ToString(System.Globalization.CultureInfo.InvariantCulture)).AppendLine();
                        }
                }
                sb.AppendLine();
                sb.AppendLine("Section,Source,Total");
                if (IncomeRows != null)
                {
                    foreach (var r in IncomeRows)
                    {
                        sb.Append('"').Append("Income").Append('"').Append(',');
                        sb.Append('"').Append(r.Category).Append('"').Append(',');
                        sb.Append(r.Total).AppendLine();
                    }
                }
                sb.AppendLine();
                sb.AppendLine("Details:Expenses");
                sb.AppendLine("Category,Date,Description,MoneyOut,BusinessPortion,TenPercent");
                if (ExpenseRows != null)
                {
                    foreach (var r in ExpenseRows)
                    {
                        if (r.Transactions == null) continue;
                        foreach (var t in r.Transactions)
                        {
                            var bp = 0m;
                            // compute per-transaction allocation for other expenses using the editable percentages
                            if (r.Category == "Other Expenses" && t.Description != null)
                            {
                                if (t.Description.IndexOf("sky", StringComparison.OrdinalIgnoreCase) >= 0) bp = t.MoneyOut * (SkyBusinessPercent / 100m);
                                else if (t.Description.IndexOf("giffgaff", StringComparison.OrdinalIgnoreCase) >= 0 || t.Description.IndexOf("giff gaff", StringComparison.OrdinalIgnoreCase) >= 0) bp = t.MoneyOut * (GiffgaffBusinessPercent / 100m);
                            }
                            sb.Append('"').Append(r.Category).Append('"').Append(',');
                            sb.Append('"').Append(t.Date?.ToString("yyyy-MM-dd")).Append('"').Append(',');
                            sb.Append('"').Append((t.Description ?? string.Empty).Replace('"', ' ')).Append('"').Append(',');
                            sb.Append(t.MoneyOut).Append(',');
                            sb.Append(bp).Append(',');
                            sb.Append(t == null ? 0 : (r.Category == "Household" ? t.MoneyOut * (HouseholdTenPercent / 100m) : 0m)).AppendLine();
                        }
                    }
                }
                sb.AppendLine();
                sb.AppendLine("Details:Income");
                sb.AppendLine("Category,Date,Description,MoneyIn,IsRefund,IsPension");
                if (IncomeRows != null)
                {
                    foreach (var r in IncomeRows)
                    {
                        if (r.Transactions == null) continue;
                        foreach (var t in r.Transactions)
                        {
                            // determine refund and pension flags using same heuristics
                            var desc = t.Description ?? string.Empty;
                            var isRefund = new[] { "REFUND", "REVERSAL", "RETURN", "REIMB", "REIMBURSE", "REVERSED" }
                                .Any(k => desc.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                            var isPension = desc.StartsWith("PGO TAX", StringComparison.OrdinalIgnoreCase)
                                           || desc.IndexOf("ARMY", StringComparison.OrdinalIgnoreCase) >= 0
                                           || desc.IndexOf("PENSION", StringComparison.OrdinalIgnoreCase) >= 0
                                           || desc.IndexOf("MNTNLY", StringComparison.OrdinalIgnoreCase) >= 0
                                           || desc.IndexOf("MNTLY", StringComparison.OrdinalIgnoreCase) >= 0;
                            sb.Append('"').Append(r.Category).Append('"').Append(',');
                            sb.Append('"').Append(t.Date?.ToString("yyyy-MM-dd")).Append('"').Append(',');
                            sb.Append('"').Append(desc.Replace('"', ' ')).Append('"').Append(',');
                            sb.Append(t.MoneyIn).Append(',');
                            sb.Append(isRefund ? "Y" : "").Append(',');
                            sb.Append(isPension ? "Y" : "").AppendLine();
                        }
                    }
                }

                var fileName = $"IncomeExpenseReport_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                var base64 = Convert.ToBase64String(bytes);
                if (JSRuntime != null)
                {
                    await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, base64);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error exporting CSV");
            }
        }
    }
}
