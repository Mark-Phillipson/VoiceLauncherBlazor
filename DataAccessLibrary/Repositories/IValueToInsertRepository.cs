using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;


namespace DataAccessLibrary.Repositories
{
    public interface IValueToInsertRepository
    {
        Task<ValueToInsertDTO?> AddValueToInsertAsync(ValueToInsertDTO valueToInsertDTO);
        Task DeleteValueToInsertAsync(int Id);
        Task<IEnumerable<ValueToInsertDTO>> GetAllValuesToInsertAsync(int maxRows);
        Task<IEnumerable<ValueToInsertDTO>> SearchValuesToInsertAsync(string serverSearchTerm);
        Task<ValueToInsertDTO?> GetValueToInsertByIdAsync(int Id);
        Task<ValueToInsertDTO?> UpdateValueToInsertAsync(ValueToInsertDTO valueToInsertDTO);
    }
}