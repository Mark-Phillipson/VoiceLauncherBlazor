using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;


namespace VoiceLauncher.Services
{
    public class PhraseListGrammarDataService : IPhraseListGrammarDataService
    {
        private readonly IPhraseListGrammarRepository _phraseListGrammarRepository;

        public PhraseListGrammarDataService(IPhraseListGrammarRepository phraseListGrammarRepository)
        {
            _phraseListGrammarRepository = phraseListGrammarRepository;
        }
        public async Task<List<PhraseListGrammarDTO>> GetAllPhraseListGrammarsAsync()
        {
            var PhraseListGrammars = await _phraseListGrammarRepository.GetAllPhraseListGrammarsAsync(300);
            return PhraseListGrammars.ToList();
        }
        public async Task<List<PhraseListGrammarDTO>> SearchPhraseListGrammarsAsync(string serverSearchTerm)
        {
            var PhraseListGrammars = await _phraseListGrammarRepository.SearchPhraseListGrammarsAsync(serverSearchTerm);
            return PhraseListGrammars.ToList();
        }

        public async Task<PhraseListGrammarDTO> GetPhraseListGrammarById(int Id)
        {
            var phraseListGrammar = await _phraseListGrammarRepository.GetPhraseListGrammarByIdAsync(Id);
            return phraseListGrammar;
        }
        public async Task<PhraseListGrammarDTO> AddPhraseListGrammar(PhraseListGrammarDTO phraseListGrammarDTO)
        {
            Guard.Against.Null(phraseListGrammarDTO);
            var result = await _phraseListGrammarRepository.AddPhraseListGrammarAsync(phraseListGrammarDTO);
            if (result == null)
            {
                throw new Exception($"Add of phraseListGrammar failed ID: {phraseListGrammarDTO.Id}");
            }
            return result;
        }
        public async Task<PhraseListGrammarDTO> UpdatePhraseListGrammar(PhraseListGrammarDTO phraseListGrammarDTO, string username)
        {
            Guard.Against.Null(phraseListGrammarDTO);
            Guard.Against.Null(username);
            var result = await _phraseListGrammarRepository.UpdatePhraseListGrammarAsync(phraseListGrammarDTO);
            if (result == null)
            {
                throw new Exception($"Update of phraseListGrammar failed ID: {phraseListGrammarDTO.Id}");
            }
            return result;
        }

        public async Task DeletePhraseListGrammar(int Id)
        {
            await _phraseListGrammarRepository.DeletePhraseListGrammarAsync(Id);
        }
    }
}