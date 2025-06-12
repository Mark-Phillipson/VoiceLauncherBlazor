using DataAccessLibrary.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
    public interface IQuickPromptRepository
    {
        Task<QuickPromptDTO?> AddQuickPromptAsync(QuickPromptDTO quickPromptDTO);
        Task DeleteQuickPromptAsync(int Id);
        Task<IEnumerable<QuickPromptDTO>> GetAllQuickPromptsAsync(int pageNumber, int pageSize, string serverSearchTerm);
        Task<IEnumerable<QuickPromptDTO>> SearchQuickPromptsAsync(string serverSearchTerm);
        Task<QuickPromptDTO?> GetQuickPromptByIdAsync(int Id);
        Task<QuickPromptDTO?> UpdateQuickPromptAsync(QuickPromptDTO quickPromptDTO);
        Task<int> GetTotalCountAsync();
        Task<IEnumerable<QuickPromptDTO>> GetQuickPromptsByTypeAsync(string type);
        Task<QuickPromptDTO?> GetQuickPromptByCommandAsync(string command);
    }
}
