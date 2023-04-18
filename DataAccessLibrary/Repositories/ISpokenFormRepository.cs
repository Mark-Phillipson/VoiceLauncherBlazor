using DataAccessLibrary.DTO;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApplication.Repositories
{
  public interface ISpokenFormRepository
  {
    Task<SpokenFormDTO> AddSpokenFormAsync(SpokenFormDTO spokenFormDTO);
    Task DeleteSpokenFormAsync(int Id);
    Task<IEnumerable<SpokenFormDTO>> GetAllSpokenFormsAsync(int WindowsSpeechVoiceCommandId);
    Task<IEnumerable<SpokenFormDTO>> SearchSpokenFormsAsync(string serverSearchTerm);
    Task<SpokenFormDTO> GetSpokenFormByIdAsync(int Id);
    Task<SpokenFormDTO> UpdateSpokenFormAsync(SpokenFormDTO spokenFormDTO);
  }
}