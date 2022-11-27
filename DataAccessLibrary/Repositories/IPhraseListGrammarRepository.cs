
using System.Collections.Generic;
using System.Threading.Tasks;

using VoiceLauncher.DTOs;

namespace VoiceLauncher.Repositories
{
    public interface IPhraseListGrammarRepository
    {
        Task<PhraseListGrammarDTO> AddPhraseListGrammarAsync(PhraseListGrammarDTO phraseListGrammarDTO);
        Task DeletePhraseListGrammarAsync(int Id);
        Task<IEnumerable<PhraseListGrammarDTO>> GetAllPhraseListGrammarsAsync(int maxRows);
        Task<IEnumerable<PhraseListGrammarDTO>> SearchPhraseListGrammarsAsync(string serverSearchTerm);
        Task<PhraseListGrammarDTO> GetPhraseListGrammarByIdAsync(int Id);
        Task<PhraseListGrammarDTO> UpdatePhraseListGrammarAsync(PhraseListGrammarDTO phraseListGrammarDTO);
    }
}