
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Services
{
    public interface ITransactionDataService
    {
        Task<List<TransactionDTO>> GetAllTransactionsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<List<TransactionDTO>> SearchTransactionsAsync(string serverSearchTerm);
        Task<TransactionDTO?> AddTransaction(TransactionDTO transactionDTO);
        Task<TransactionDTO?> GetTransactionById(int Id);
        Task<TransactionDTO?> UpdateTransaction(TransactionDTO transactionDTO, string? username);
        Task DeleteTransaction(int Id);
        Task<int> GetTotalCount();
        Task<ImportResult> ImportTransactions(string fileContent, string filename);
        Task<List<TransactionDTO>> ProcessTransactions(List<TransactionDTO> transactions);
        Task<int> AddTransactions(List<TransactionDTO> transactions);
    }
}