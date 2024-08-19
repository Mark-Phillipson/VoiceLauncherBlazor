
using System;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Repositories;

namespace DataAccessLibrary.Repositories
{
    public class TransactionTypeMappingRepository : ITransactionTypeMappingRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public TransactionTypeMappingRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<TransactionTypeMappingDTO>> GetAllTransactionTypeMappingsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            List<TransactionTypeMapping> TransactionTypeMappings;
            if (!string.IsNullOrWhiteSpace(serverSearchTerm))
            {
                TransactionTypeMappings = await context.TransactionTypeMappings
                                        .Where(v =>
                    (v.MyTransactionType != null && v.MyTransactionType.ToLower().Contains(serverSearchTerm))
                     || (v.Type != null && v.Type.ToLower().Contains(serverSearchTerm))
                    )

                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                TransactionTypeMappings = await context.TransactionTypeMappings
                    //.OrderBy(v => v.?)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            IEnumerable<TransactionTypeMappingDTO> TransactionTypeMappingsDTO = _mapper.Map<List<TransactionTypeMapping>, IEnumerable<TransactionTypeMappingDTO>>(TransactionTypeMappings);
            return TransactionTypeMappingsDTO;
        }
        public async Task<IEnumerable<TransactionTypeMappingDTO>> SearchTransactionTypeMappingsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var TransactionTypeMappings = await context.TransactionTypeMappings
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<TransactionTypeMappingDTO> TransactionTypeMappingsDTO = _mapper.Map<List<TransactionTypeMapping>, IEnumerable<TransactionTypeMappingDTO>>(TransactionTypeMappings);
            return TransactionTypeMappingsDTO;
        }

        public async Task<TransactionTypeMappingDTO> GetTransactionTypeMappingByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.TransactionTypeMappings.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            TransactionTypeMappingDTO transactionTypeMappingDTO = _mapper.Map<TransactionTypeMapping, TransactionTypeMappingDTO>(result);
            return transactionTypeMappingDTO;
        }

        public async Task<string> AddTransactionTypeMappingAsync(TransactionTypeMappingDTO transactionTypeMappingDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            TransactionTypeMapping transactionTypeMapping = _mapper.Map<TransactionTypeMappingDTO, TransactionTypeMapping>(transactionTypeMappingDTO);
            var addedEntity = context.TransactionTypeMappings.Add(transactionTypeMapping);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return exception.InnerException != null ? exception.InnerException.Message : exception.Message;
            }
            TransactionTypeMappingDTO resultDTO = _mapper.Map<TransactionTypeMapping, TransactionTypeMappingDTO>(transactionTypeMapping);
            return "success";
        }

        public async Task<TransactionTypeMappingDTO> UpdateTransactionTypeMappingAsync(TransactionTypeMappingDTO transactionTypeMappingDTO)
        {
            TransactionTypeMapping transactionTypeMapping = _mapper.Map<TransactionTypeMappingDTO, TransactionTypeMapping>(transactionTypeMappingDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundTransactionTypeMapping = await context.TransactionTypeMappings.AsNoTracking().FirstOrDefaultAsync(e => e.Id == transactionTypeMapping.Id);

                if (foundTransactionTypeMapping != null)
                {
                    var mappedTransactionTypeMapping = _mapper.Map<TransactionTypeMapping>(transactionTypeMapping);
                    context.TransactionTypeMappings.Update(mappedTransactionTypeMapping);
                    await context.SaveChangesAsync();
                    TransactionTypeMappingDTO resultDTO = _mapper.Map<TransactionTypeMapping, TransactionTypeMappingDTO>(mappedTransactionTypeMapping);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteTransactionTypeMappingAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundTransactionTypeMapping = context.TransactionTypeMappings.FirstOrDefault(e => e.Id == Id);
            if (foundTransactionTypeMapping == null)
            {
                return;
            }
            context.TransactionTypeMappings.Remove(foundTransactionTypeMapping);
            await context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.TransactionTypeMappings.CountAsync();
        }

    }
}