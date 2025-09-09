using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataAccessLibrary.Services
{
    public class TransactionTypeMappingDataService : ITransactionTypeMappingDataService
    {
        private readonly ITransactionTypeMappingRepository _transactionTypeMappingRepository;

        public TransactionTypeMappingDataService(ITransactionTypeMappingRepository transactionTypeMappingRepository)
        {
            this._transactionTypeMappingRepository = transactionTypeMappingRepository;
        }
        public async Task<List<TransactionTypeMappingDTO>> GetAllTransactionTypeMappingsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            var TransactionTypeMappings = await _transactionTypeMappingRepository.GetAllTransactionTypeMappingsAsync(pageNumber, pageSize, serverSearchTerm);
            return TransactionTypeMappings.ToList();
        }
        public async Task<List<TransactionTypeMappingDTO>> SearchTransactionTypeMappingsAsync(string serverSearchTerm)
        {
            var TransactionTypeMappings = await _transactionTypeMappingRepository.SearchTransactionTypeMappingsAsync(serverSearchTerm);
            return TransactionTypeMappings.ToList();
        }

        public async Task<TransactionTypeMappingDTO?> GetTransactionTypeMappingById(int Id)
        {
            var transactionTypeMapping = await _transactionTypeMappingRepository.GetTransactionTypeMappingByIdAsync(Id);
            return transactionTypeMapping;
        }
        public async Task<string> AddTransactionTypeMapping(TransactionTypeMappingDTO transactionTypeMappingDTO)
        {
            Guard.Against.Null(transactionTypeMappingDTO);
            var result = await _transactionTypeMappingRepository.AddTransactionTypeMappingAsync(transactionTypeMappingDTO);
            if (result == null)
            {
                throw new Exception($"Add of transactionTypeMapping failed ID: {transactionTypeMappingDTO.Id}");
            }
            return result;
        }
        public async Task<TransactionTypeMappingDTO?> UpdateTransactionTypeMapping(TransactionTypeMappingDTO transactionTypeMappingDTO, string? username)
        {
            Guard.Against.Null(transactionTypeMappingDTO);
            Guard.Against.Null(username);
            var result = await _transactionTypeMappingRepository.UpdateTransactionTypeMappingAsync(transactionTypeMappingDTO);
            if (result == null)
            {
                throw new Exception($"Update of transactionTypeMapping failed ID: {transactionTypeMappingDTO.Id}");
            }
            return result;
        }

        public async Task DeleteTransactionTypeMapping(int Id)
        {
            await _transactionTypeMappingRepository.DeleteTransactionTypeMappingAsync(Id);
        }
        public async Task<int> GetTotalCount()
        {
            int result = await _transactionTypeMappingRepository.GetTotalCountAsync();
            return result;
        }
    }
}