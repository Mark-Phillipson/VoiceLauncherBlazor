using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;
using VoiceLauncher.DTOs;


namespace VoiceLauncher.Services
{
    public class GrammarNameDataService : IGrammarNameDataService
    {
        private readonly IGrammarNameRepository _grammarNameRepository;

        public GrammarNameDataService(IGrammarNameRepository grammarNameRepository)
        {
            this._grammarNameRepository = grammarNameRepository;
        }
        public async Task<List<GrammarNameDTO>> GetAllGrammarNamesAsync()
        {
            var GrammarNames = await _grammarNameRepository.GetAllGrammarNamesAsync(300);
            return GrammarNames.ToList();
        }
        public async Task<List<GrammarNameDTO>> SearchGrammarNamesAsync(string serverSearchTerm)
        {
            var GrammarNames = await _grammarNameRepository.SearchGrammarNamesAsync(serverSearchTerm);
            return GrammarNames.ToList();
        }

        public async Task<GrammarNameDTO?> GetGrammarNameById(int Id)
        {
            var grammarName = await _grammarNameRepository.GetGrammarNameByIdAsync(Id);
            return grammarName;
        }
        public async Task<GrammarNameDTO?> AddGrammarName(GrammarNameDTO grammarNameDTO)
        {
            Guard.Against.Null(grammarNameDTO);
            var result = await _grammarNameRepository.AddGrammarNameAsync(grammarNameDTO);
            if (result == null)
            {
                throw new Exception($"Add of grammarName failed ID: {grammarNameDTO.Id}");
            }
            return result;
        }
        public async Task<GrammarNameDTO> UpdateGrammarName(GrammarNameDTO grammarNameDTO, string? username)
        {
            Guard.Against.Null(grammarNameDTO);
            Guard.Against.Null(username);
            var result = await _grammarNameRepository.UpdateGrammarNameAsync(grammarNameDTO);
            if (result == null)
            {
                throw new Exception($"Update of grammarName failed ID: {grammarNameDTO.Id}");
            }
            return result;
        }

        public async Task DeleteGrammarName(int Id)
        {
            await _grammarNameRepository.DeleteGrammarNameAsync(Id);
        }

        public async Task<GrammarNameDTO> GetLatest()
        {
            var result = await _grammarNameRepository.GetLatest();
            return result;
        }
    }
}