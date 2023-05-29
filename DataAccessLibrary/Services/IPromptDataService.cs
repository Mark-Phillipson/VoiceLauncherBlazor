using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApplication.Services
{
    public interface IPromptDataService
    {
        Task<List<PromptDTO>> GetAllPromptsAsync( );
        Task<List<PromptDTO>> SearchPromptsAsync(string serverSearchTerm);
        Task<PromptDTO> AddPrompt(PromptDTO promptDTO);
        Task<PromptDTO> GetPromptById(int Id);
        Task<PromptDTO> UpdatePrompt(PromptDTO promptDTO, string username);
        Task DeletePrompt(int Id);
    }
}
