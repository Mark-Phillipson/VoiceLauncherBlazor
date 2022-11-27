
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Services
{
    public interface IPhraseListGrammarDataService
    {
        Task<List<PhraseListGrammarDTO>> GetAllPhraseListGrammarsAsync( );
        Task<List<PhraseListGrammarDTO>> SearchPhraseListGrammarsAsync(string serverSearchTerm);
        Task<PhraseListGrammarDTO> AddPhraseListGrammar(PhraseListGrammarDTO phraseListGrammarDTO);
        Task<PhraseListGrammarDTO> GetPhraseListGrammarById(int Id);
        Task<PhraseListGrammarDTO> UpdatePhraseListGrammar(PhraseListGrammarDTO phraseListGrammarDTO, string username);
        Task DeletePhraseListGrammar(int Id);
    }
}
