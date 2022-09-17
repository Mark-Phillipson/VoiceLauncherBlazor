
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IGrammarItemRepository
    {
        Task<GrammarItemDTO> AddGrammarItemAsync(GrammarItemDTO grammarItemDTO);
        Task DeleteGrammarItemAsync(int Id);
		Task<IEnumerable<GrammarItemDTO>> GetAllGrammarItemsAsync(int GrammarNameId);
        Task<IEnumerable<GrammarItemDTO>> SearchGrammarItemsAsync(string serverSearchTerm);
        Task<GrammarItemDTO> GetGrammarItemByIdAsync(int Id);
        Task<GrammarItemDTO> UpdateGrammarItemAsync(GrammarItemDTO grammarItemDTO);
        Task<bool> SaveAllAsync(List<GrammarItemDTO> grammarItemDTO);
    }
}