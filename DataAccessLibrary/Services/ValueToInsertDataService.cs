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
    public class ValueToInsertDataService : IValueToInsertDataService
    {
        private readonly IValueToInsertRepository _valueToInsertRepository;

        public ValueToInsertDataService(IValueToInsertRepository valueToInsertRepository)
        {
            _valueToInsertRepository = valueToInsertRepository;
        }
        public async Task<List<ValueToInsertDTO>> GetAllValuesToInsertAsync()
        {
            var ValuesToInsert = await _valueToInsertRepository.GetAllValuesToInsertAsync(300);
            return ValuesToInsert.ToList();
        }
        public async Task<List<ValueToInsertDTO>> SearchValuesToInsertAsync(string serverSearchTerm)
        {
            var ValuesToInsert = await _valueToInsertRepository.SearchValuesToInsertAsync(serverSearchTerm);
            return ValuesToInsert.ToList();
        }

        public async Task<ValueToInsertDTO?> GetValueToInsertById(int Id)
        {
            var valueToInsert = await _valueToInsertRepository.GetValueToInsertByIdAsync(Id);
            return valueToInsert;
        }
        public async Task<ValueToInsertDTO?> AddValueToInsert(ValueToInsertDTO valueToInsertDTO)
        {
            Guard.Against.Null(valueToInsertDTO);
            var result = await _valueToInsertRepository.AddValueToInsertAsync(valueToInsertDTO);
            if (result == null)
            {
                throw new Exception($"Add of valueToInsert failed ID: {valueToInsertDTO.Id}");
            }
            return result;
        }
        public async Task<ValueToInsertDTO?> UpdateValueToInsert(ValueToInsertDTO valueToInsertDTO, string? username)
        {
            Guard.Against.Null(valueToInsertDTO);
            Guard.Against.Null(username);
            var result = await _valueToInsertRepository.UpdateValueToInsertAsync(valueToInsertDTO);
            if (result == null)
            {
                throw new Exception($"Update of valueToInsert failed ID: {valueToInsertDTO.Id}");
            }
            return result;
        }

        public async Task DeleteValueToInsert(int Id)
        {
            await _valueToInsertRepository.DeleteValueToInsertAsync(Id);
        }
    }
}