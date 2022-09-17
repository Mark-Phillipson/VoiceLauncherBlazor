
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface IGrammarItemDataService
    {
        Task<List<GrammarItemDTO>> GetAllGrammarItemsAsync(int GrammarNameId);
        Task<List<GrammarItemDTO>> SearchGrammarItemsAsync(string serverSearchTerm);
        Task<GrammarItemDTO> AddGrammarItem(GrammarItemDTO grammarItemDTO);
        Task<GrammarItemDTO> GetGrammarItemById(int Id);
        Task<GrammarItemDTO> UpdateGrammarItem(GrammarItemDTO grammarItemDTO, string? username);
        Task DeleteGrammarItem(int Id);
        Task<bool> SaveAllAsync(List<GrammarItemDTO> filteredGrammarItemDTO);
    }
}
