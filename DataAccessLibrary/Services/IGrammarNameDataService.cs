
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface IGrammarNameDataService
    {
        Task<List<GrammarNameDTO>> GetAllGrammarNamesAsync( );
        Task<List<GrammarNameDTO>> SearchGrammarNamesAsync(string serverSearchTerm);
        Task<GrammarNameDTO> AddGrammarName(GrammarNameDTO grammarNameDTO);
        Task<GrammarNameDTO> GetGrammarNameById(int Id);
        Task<GrammarNameDTO> UpdateGrammarName(GrammarNameDTO grammarNameDTO, string username);
        Task DeleteGrammarName(int Id);
        Task<GrammarNameDTO> GetLatest();
    }
}
