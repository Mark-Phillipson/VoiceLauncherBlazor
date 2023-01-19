
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
            var ValuesToInsert= await context.ValuesToInsert
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<ValueToInsertDTO> ValuesToInsertDTO = _mapper.Map<List<ValueToInsert>, IEnumerable<ValueToInsertDTO>>(ValuesToInsert);
            return ValuesToInsertDTO;
        }
        public async Task<IEnumerable<ValueToInsertDTO>> SearchValuesToInsertAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var ValuesToInsert= await context.ValuesToInsert
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<ValueToInsertDTO> ValuesToInsertDTO = _mapper.Map<List<ValueToInsert>, IEnumerable<ValueToInsertDTO>>(ValuesToInsert);
            return ValuesToInsertDTO;
        }

        public async Task<ValueToInsertDTO> GetValueToInsertByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result =await context.ValuesToInsert.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            ValueToInsertDTO valueToInsertDTO =_mapper.Map<ValueToInsert, ValueToInsertDTO>(result);
            return valueToInsertDTO;
        }

        public async Task<ValueToInsertDTO> AddValueToInsertAsync(ValueToInsertDTO valueToInsertDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            ValueToInsert valueToInsert = _mapper.Map<ValueToInsertDTO, ValueToInsert>(valueToInsertDTO);
            var addedEntity = context.ValuesToInsert.Add(valueToInsert);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            ValueToInsertDTO resultDTO =_mapper.Map<ValueToInsert, ValueToInsertDTO>(valueToInsert);
            return resultDTO;
        }

        public async Task<ValueToInsertDTO?> UpdateValueToInsertAsync(ValueToInsertDTO valueToInsertDTO)
        {
            ValueToInsert valueToInsert=_mapper.Map<ValueToInsertDTO, ValueToInsert>(valueToInsertDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundValueToInsert = await context.ValuesToInsert.AsNoTracking().FirstOrDefaultAsync(e => e.Id == valueToInsert.Id);

                if (foundValueToInsert != null)
                {
                    var mappedValueToInsert = _mapper.Map<ValueToInsert>(valueToInsert);
                    context.ValuesToInsert.Update(mappedValueToInsert);
                    await context.SaveChangesAsync();
                    ValueToInsertDTO resultDTO = _mapper.Map<ValueToInsert, ValueToInsertDTO>(mappedValueToInsert);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteValueToInsertAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundValueToInsert = context.ValuesToInsert.FirstOrDefault(e => e.Id == Id);
            if (foundValueToInsert == null)
            {
                return;
            }
            context.ValuesToInsert.Remove(foundValueToInsert);
            await context.SaveChangesAsync();
        }
    }
}