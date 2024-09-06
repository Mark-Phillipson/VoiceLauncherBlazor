using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public interface ITalonAlphabetDataService
	{
		Task<List<TalonAlphabetDTO>> GetAllTalonAlphabetsAsync();
		Task<List<TalonAlphabetDTO>> SearchTalonAlphabetsAsync(string serverSearchTerm);
		Task<TalonAlphabetDTO?> AddTalonAlphabet(TalonAlphabetDTO talonAlphabetDTO);
		Task<TalonAlphabetDTO?> GetTalonAlphabetById(int Id);
		Task<TalonAlphabetDTO?> UpdateTalonAlphabet(TalonAlphabetDTO talonAlphabetDTO, string username);
		Task DeleteTalonAlphabet(int Id);
	}
}
