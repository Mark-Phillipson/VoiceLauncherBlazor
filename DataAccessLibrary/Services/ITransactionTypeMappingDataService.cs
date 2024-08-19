

using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Services
{
    public interface ITransactionTypeMappingDataService
    {
        Task<List<TransactionTypeMappingDTO>> GetAllTransactionTypeMappingsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<List<TransactionTypeMappingDTO>> SearchTransactionTypeMappingsAsync(string serverSearchTerm);
        Task<string> AddTransactionTypeMapping(TransactionTypeMappingDTO transactionTypeMappingDTO);
        Task<TransactionTypeMappingDTO> GetTransactionTypeMappingById(int Id);
        Task<TransactionTypeMappingDTO> UpdateTransactionTypeMapping(TransactionTypeMappingDTO transactionTypeMappingDTO, string? username);
        Task DeleteTransactionTypeMapping(int Id);
        Task<int> GetTotalCount();
    }
}