
using AutoMapper;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DataAccessLibrary.Repositories
{
    public class ValueToInsertRepository : IValueToInsertRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public ValueToInsertRepository(IDbContextFactory<ApplicationDbContext> contextFactory,IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
		        public async Task<IEnumerable<ValueToInsertDTO>> GetAllValuesToInsertAsync(int maxRows= 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var ValuesToInsert= await context.ValuesToInserts
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<ValueToInsertDTO> ValuesToInsertDTO = _mapper.Map<List<ValuesToInsert>, IEnumerable<ValueToInsertDTO>>(ValuesToInsert);
            return ValuesToInsertDTO;
        }
        public async Task<IEnumerable<ValueToInsertDTO>> SearchValuesToInsertAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var ValuesToInsert= await context.ValuesToInserts
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<ValueToInsertDTO> ValuesToInsertDTO = _mapper.Map<List<ValuesToInsert>, IEnumerable<ValueToInsertDTO>>(ValuesToInsert);
            return ValuesToInsertDTO;
        }

        public async Task<ValueToInsertDTO> GetValueToInsertByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result =await context.ValuesToInserts.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            ValueToInsertDTO valueToInsertDTO =_mapper.Map<ValuesToInsert, ValueToInsertDTO>(result);
            return valueToInsertDTO;
        }

        public async Task<ValueToInsertDTO> AddValueToInsertAsync(ValueToInsertDTO valueToInsertDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            ValuesToInsert valueToInsert = _mapper.Map<ValueToInsertDTO, ValuesToInsert>(valueToInsertDTO);
            var addedEntity = context.ValuesToInserts.Add(valueToInsert);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            ValueToInsertDTO resultDTO =_mapper.Map<ValuesToInsert, ValueToInsertDTO>(valueToInsert);
            return resultDTO;
        }

        public async Task<ValueToInsertDTO> UpdateValueToInsertAsync(ValueToInsertDTO valueToInsertDTO)
        {
            ValuesToInsert valueToInsert=_mapper.Map<ValueToInsertDTO, ValuesToInsert>(valueToInsertDTO);
               using (var context = _contextFactory.CreateDbContext())
            {
                var foundValueToInsert = await context.ValuesToInserts.AsNoTracking().FirstOrDefaultAsync(e => e.Id == valueToInsert.Id);

                if (foundValueToInsert != null)
                {
                    var mappedValueToInsert = _mapper.Map<ValuesToInsert>(valueToInsert);
                    context.ValuesToInserts.Update(mappedValueToInsert);
                    await context.SaveChangesAsync();
                    ValueToInsertDTO resultDTO = _mapper.Map<ValuesToInsert, ValueToInsertDTO>(mappedValueToInsert);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteValueToInsertAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundValueToInsert = context.ValuesToInserts.FirstOrDefault(e => e.Id == Id);
            if (foundValueToInsert == null)
            {
                return;
            }
            context.ValuesToInserts.Remove(foundValueToInsert);
            await context.SaveChangesAsync();
        }
    }
}