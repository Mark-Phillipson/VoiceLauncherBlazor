
using System;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTOs;

namespace DataAccessLibrary.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public TransactionRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            List<Transaction> Transactions;
            if (!string.IsNullOrWhiteSpace(serverSearchTerm))
            {
                Transactions = await context.Transactions
                                        .Where(v =>
                    (v.Description != null && v.Description.ToLower().Contains(serverSearchTerm))
                     || (v.Type != null && v.Type.ToLower().Contains(serverSearchTerm))
                     || (v.MyTransactionType != null && v.MyTransactionType.ToLower().Contains(serverSearchTerm))
                    )

                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                Transactions = await context.Transactions
                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            IEnumerable<TransactionDTO> TransactionsDTO = _mapper.Map<List<Transaction>, IEnumerable<TransactionDTO>>(Transactions);
            return TransactionsDTO;
        }
        public async Task<IEnumerable<TransactionDTO>> SearchTransactionsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var Transactions = await context.Transactions
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<TransactionDTO> TransactionsDTO = _mapper.Map<List<Transaction>, IEnumerable<TransactionDTO>>(Transactions);
            return TransactionsDTO;
        }

        public async Task<TransactionDTO?> GetTransactionByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.Transactions.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            TransactionDTO transactionDTO = _mapper.Map<Transaction, TransactionDTO>(result);
            return transactionDTO;
        }

        public async Task<TransactionDTO?> AddTransactionAsync(TransactionDTO transactionDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            Transaction transaction = _mapper.Map<TransactionDTO, Transaction>(transactionDTO);
            var addedEntity = context.Transactions.Add(transaction);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            TransactionDTO resultDTO = _mapper.Map<Transaction, TransactionDTO>(transaction);
            return resultDTO;
        }

        public async Task<TransactionDTO?> UpdateTransactionAsync(TransactionDTO transactionDTO)
        {
            Transaction transaction = _mapper.Map<TransactionDTO, Transaction>(transactionDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundTransaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == transaction.Id);

                if (foundTransaction != null)
                {
                    var mappedTransaction = _mapper.Map<Transaction>(transaction);
                    context.Transactions.Update(mappedTransaction);
                    await context.SaveChangesAsync();
                    TransactionDTO resultDTO = _mapper.Map<Transaction, TransactionDTO>(mappedTransaction);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteTransactionAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundTransaction = context.Transactions.FirstOrDefault(e => e.Id == Id);
            if (foundTransaction == null)
            {
                return;
            }
            context.Transactions.Remove(foundTransaction);
            await context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Transactions.CountAsync();
        }

        public async Task<int> AddTransactions(List<TransactionDTO> transactions)
        {
            using var context = _contextFactory.CreateDbContext();
            var mappedTransactions = _mapper.Map<List<TransactionDTO>, List<Transaction>>(transactions);
            context.Transactions.AddRange(mappedTransactions);
            try
            {
                var result = await context.SaveChangesAsync();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return 0;
            }
        }
    }
}