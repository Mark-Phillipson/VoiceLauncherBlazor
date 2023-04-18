using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
  public interface ISpokenFormDataService
  {
    Task<List<SpokenFormDTO>> GetAllSpokenFormsAsync(int WindowsSpeechVoiceCommandId);
    Task<List<SpokenFormDTO>> SearchSpokenFormsAsync(string serverSearchTerm);
    Task<SpokenFormDTO> AddSpokenForm(SpokenFormDTO spokenFormDTO);
    Task<SpokenFormDTO> GetSpokenFormById(int Id);
    Task<SpokenFormDTO> UpdateSpokenForm(SpokenFormDTO spokenFormDTO, string username);
    Task DeleteSpokenForm(int Id);
  }
}
