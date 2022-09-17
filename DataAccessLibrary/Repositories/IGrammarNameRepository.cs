
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IGrammarNameRepository
    {
        Task<GrammarNameDTO> AddGrammarNameAsync(GrammarNameDTO grammarNameDTO);
        Task DeleteGrammarNameAsync(int Id);
        Task<IEnumerable<GrammarNameDTO>> GetAllGrammarNamesAsync(int maxRows);
        Task<IEnumerable<GrammarNameDTO>> SearchGrammarNamesAsync(string serverSearchTerm);
        Task<GrammarNameDTO> GetGrammarNameByIdAsync(int Id);
        Task<GrammarNameDTO> UpdateGrammarNameAsync(GrammarNameDTO grammarNameDTO);
        Task<GrammarNameDTO> GetLatest();
    }
}