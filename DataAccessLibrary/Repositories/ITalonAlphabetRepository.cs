using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
	public interface ITalonAlphabetRepository
	{
		Task<TalonAlphabetDTO?> AddTalonAlphabetAsync(TalonAlphabetDTO talonAlphabetDTO);
		Task DeleteTalonAlphabetAsync(int Id);
		Task<IEnumerable<TalonAlphabetDTO>> GetAllTalonAlphabetsAsync(int maxRows);
		Task<IEnumerable<TalonAlphabetDTO>> SearchTalonAlphabetsAsync(string serverSearchTerm);
		Task<TalonAlphabetDTO?> GetTalonAlphabetByIdAsync(int Id);
		Task<TalonAlphabetDTO?> UpdateTalonAlphabetAsync(TalonAlphabetDTO talonAlphabetDTO);
	}
}