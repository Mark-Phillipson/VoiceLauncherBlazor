using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccessLibrary.Services
{
    public interface IValueToInsertDataService
    {
        Task<List<ValueToInsertDTO>> GetAllValuesToInsertAsync();
        Task<List<ValueToInsertDTO>> SearchValuesToInsertAsync(string serverSearchTerm);
        Task<ValueToInsertDTO?> AddValueToInsert(ValueToInsertDTO valueToInsertDTO);
        Task<ValueToInsertDTO?> GetValueToInsertById(int Id);
        Task<ValueToInsertDTO?> UpdateValueToInsert(ValueToInsertDTO valueToInsertDTO, string? username);
        Task DeleteValueToInsert(int Id);
    }
}
