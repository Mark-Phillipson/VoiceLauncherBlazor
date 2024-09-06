using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories;

public interface ILanguageRepository
{
   Task<LanguageDTO?> AddLanguageAsync(LanguageDTO languageDTO);
   Task DeleteLanguageAsync(int Id);
   Task<IEnumerable<LanguageDTO>> GetAllLanguagesAsync(int maxRows);
   Task<IEnumerable<LanguageDTO>> SearchLanguagesAsync(string serverSearchTerm);
   Task<LanguageDTO?> GetLanguageByIdAsync(int Id);
   Task<LanguageDTO?> UpdateLanguageAsync(LanguageDTO languageDTO);
}