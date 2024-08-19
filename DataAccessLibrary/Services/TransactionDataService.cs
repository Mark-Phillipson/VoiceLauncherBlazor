using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTOs;
using System.IO;
using System.Globalization;


namespace DataAccessLibrary.Services
{
    public class TransactionDataService : ITransactionDataService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionTypeMappingRepository _transactionTypeMappingRepository;

        public TransactionDataService(ITransactionRepository transactionRepository, ITransactionTypeMappingRepository transactionTypeMappingRepository)
        {
            this._transactionRepository = transactionRepository;
            this._transactionTypeMappingRepository = transactionTypeMappingRepository;
        }
        public async Task<List<TransactionDTO>> GetAllTransactionsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            var Transactions = await _transactionRepository.GetAllTransactionsAsync(pageNumber, pageSize, serverSearchTerm);
            return Transactions.ToList();
        }
        public async Task<List<TransactionDTO>> SearchTransactionsAsync(string serverSearchTerm)
        {
            var Transactions = await _transactionRepository.SearchTransactionsAsync(serverSearchTerm);
            return Transactions.ToList();
        }

        public async Task<TransactionDTO> GetTransactionById(int Id)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(Id);
            return transaction;
        }
        public async Task<TransactionDTO> AddTransaction(TransactionDTO transactionDTO)
        {
            Guard.Against.Null(transactionDTO);
            var result = await _transactionRepository.AddTransactionAsync(transactionDTO);
            if (result == null)
            {
                throw new Exception($"Add of transaction failed ID: {transactionDTO.Id}");
            }
            return result;
        }
        public async Task<TransactionDTO> UpdateTransaction(TransactionDTO transactionDTO, string username)
        {
            Guard.Against.Null(transactionDTO);
            Guard.Against.Null(username);
            var result = await _transactionRepository.UpdateTransactionAsync(transactionDTO);
            if (result == null)
            {
                throw new Exception($"Update of transaction failed ID: {transactionDTO.Id}");
            }
            return result;
        }

        public async Task DeleteTransaction(int Id)
        {
            await _transactionRepository.DeleteTransactionAsync(Id);
        }
        public async Task<int> GetTotalCount()
        {
            int result = await _transactionRepository.GetTotalCountAsync();
            return result;
        }

        public Task<List<TransactionDTO>> ImportTransactions(string fileContents, string filename)
        {
            var transactions = new List<TransactionDTO>();

            using (var reader = new StringReader(fileContents))
            {
                // Skip the header
                reader.ReadLine();

                while (reader.Peek() != -1)
                {
                    var line = reader.ReadLine();
                    string[] values = null;
                    if (line != null)
                    {
                        values = line.Split(',');
                    }
                    if (values == null || values.Length == 0)
                    {
                        System.Console.WriteLine("Invalid line: " + line);
                        continue;
                    }
                    DateTime date;
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].Replace("\"", string.Empty);
                    }
                    try
                    {
                        date = DateTime.ParseExact(values[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    catch (System.Exception exception)
                    {
                        System.Console.WriteLine($"Invalid date: {exception.Message}");
                        throw;
                    }
                    string description = values[1];
                    string type = values[2];
                    decimal moneyIn = 0;
                    if (values[3].Length > 0)
                    {
                        moneyIn = decimal.Parse(values[3]);
                    }
                    decimal moneyOut = 0;
                    if (values[4].Length > 0)
                    {
                        moneyOut = decimal.Parse(values[4]);
                    }
                    decimal balance = 0;
                    if (values[5].Length > 0)
                    {
                        balance = decimal.Parse(values[5]);
                    }

                    var transaction = new TransactionDTO
                    {
                        Date = date,
                        Description = description,
                        Type = type,
                        MoneyIn = moneyIn,
                        MoneyOut = moneyOut,
                        Balance = balance,
                        ImportFilename = filename,
                        ImportDate = DateTime.Now
                    };

                    transactions.Add(transaction);
                }
            }

            return Task.FromResult(transactions);
        }


        public async Task<List<TransactionDTO>> ProcessTransactions(List<TransactionDTO> transactions)
        {
            var mappings = await _transactionTypeMappingRepository.GetAllTransactionTypeMappingsAsync(1, 2000, "");
            foreach (var transaction in transactions)
            {
                foreach (var mapping in mappings)
                {
                    if (transaction.Description.ToLower().Contains(mapping.Type))
                    {
                        transaction.MyTransactionType = mapping.MyTransactionType;
                        break;
                    }
                }
            }
            return transactions;
        }

        public async Task<int> AddTransactions(List<TransactionDTO> transactions)
        {
            var result = await _transactionRepository.AddTransactions(transactions);
            return result;
        }
    }
}