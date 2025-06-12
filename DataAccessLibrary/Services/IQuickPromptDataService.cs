using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Services
{
    public interface IQuickPromptDataService
    {
        Task<List<QuickPromptDTO>> GetAllQuickPromptsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<List<QuickPromptDTO>> SearchQuickPromptsAsync(string serverSearchTerm);
        Task<QuickPromptDTO?> AddQuickPrompt(QuickPromptDTO quickPromptDTO);
        Task<QuickPromptDTO?> GetQuickPromptById(int Id);
        Task<QuickPromptDTO?> UpdateQuickPrompt(QuickPromptDTO quickPromptDTO, string? username);
        Task DeleteQuickPrompt(int Id);
        Task<int> GetTotalCount();
        Task<List<QuickPromptDTO>> GetQuickPromptsByType(string type);
        Task<QuickPromptDTO?> GetQuickPromptByCommand(string command);
        Task<List<string>> GetQuickPromptTypes();
    }
}
