using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApplication.Repositories
{
    public interface IPromptRepository
    {
        Task<PromptDTO> AddPromptAsync(PromptDTO promptDTO);
        Task DeletePromptAsync(int Id);
        Task<IEnumerable<PromptDTO>> GetAllPromptsAsync(int maxRows);
        Task<IEnumerable<PromptDTO>> SearchPromptsAsync(string serverSearchTerm);
        Task<PromptDTO> GetPromptByIdAsync(int Id);
        Task<PromptDTO> UpdatePromptAsync(PromptDTO promptDTO);
    }
}