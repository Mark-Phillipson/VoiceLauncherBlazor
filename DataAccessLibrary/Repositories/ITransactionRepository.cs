
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface ITransactionRepository
    {
        Task<TransactionDTO> AddTransactionAsync(TransactionDTO transactionDTO);
        Task DeleteTransactionAsync(int Id);
        Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<IEnumerable<TransactionDTO>> SearchTransactionsAsync(string serverSearchTerm);
        Task<TransactionDTO> GetTransactionByIdAsync(int Id);
        Task<TransactionDTO> UpdateTransactionAsync(TransactionDTO transactionDTO);
        Task<int> GetTotalCountAsync();
        Task<int> AddTransactions(List<TransactionDTO> transactions);
    }
}