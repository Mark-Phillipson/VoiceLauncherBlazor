
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public interface ITransactionTypeMappingRepository
    {
        Task<string> AddTransactionTypeMappingAsync(TransactionTypeMappingDTO transactionTypeMappingDTO);
        Task DeleteTransactionTypeMappingAsync(int Id);
        Task<IEnumerable<TransactionTypeMappingDTO>> GetAllTransactionTypeMappingsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<IEnumerable<TransactionTypeMappingDTO>> SearchTransactionTypeMappingsAsync(string serverSearchTerm);
        Task<TransactionTypeMappingDTO?> GetTransactionTypeMappingByIdAsync(int Id);
        Task<TransactionTypeMappingDTO?> UpdateTransactionTypeMappingAsync(TransactionTypeMappingDTO transactionTypeMappingDTO);
        Task<int> GetTotalCountAsync();
    }
}