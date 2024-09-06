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
    public class GrammarItemDataService : IGrammarItemDataService
    {
        private readonly IGrammarItemRepository _grammarItemRepository;

        public GrammarItemDataService(IGrammarItemRepository grammarItemRepository)
        {
            this._grammarItemRepository = grammarItemRepository;
        }
        public async Task<List<GrammarItemDTO>> GetAllGrammarItemsAsync(int GrammarNameId)
        {
            var GrammarItems = await _grammarItemRepository.GetAllGrammarItemsAsync(GrammarNameId);
            return GrammarItems.ToList();
        }
        public async Task<List<GrammarItemDTO>> SearchGrammarItemsAsync(string serverSearchTerm)
        {
            var GrammarItems = await _grammarItemRepository.SearchGrammarItemsAsync(serverSearchTerm);
            return GrammarItems.ToList();
        }

        public async Task<GrammarItemDTO?> GetGrammarItemById(int Id)
        {
            var grammarItem = await _grammarItemRepository.GetGrammarItemByIdAsync(Id);
            return grammarItem;
        }
        public async Task<GrammarItemDTO?> AddGrammarItem(GrammarItemDTO grammarItemDTO)
        {
            Guard.Against.Null(grammarItemDTO);
            var result = await _grammarItemRepository.AddGrammarItemAsync(grammarItemDTO);
            if (result == null)
            {
                throw new Exception($"Add of grammarItem failed ID: {grammarItemDTO.Id}");
            }
            return result;
        }
        public async Task<GrammarItemDTO?> UpdateGrammarItem(GrammarItemDTO grammarItemDTO, string username)
        {
            Guard.Against.Null(grammarItemDTO);
            Guard.Against.Null(username);
            var result = await _grammarItemRepository.UpdateGrammarItemAsync(grammarItemDTO);
            if (result == null)
            {
                throw new Exception($"Update of grammarItem failed ID: {grammarItemDTO.Id}");
            }
            return result;
        }

        public async Task DeleteGrammarItem(int Id)
        {
            await _grammarItemRepository.DeleteGrammarItemAsync(Id);
        }

        public async Task<bool> SaveAllAsync(List<GrammarItemDTO> grammarItemDTO)
        {
            var result = await _grammarItemRepository.SaveAllAsync(grammarItemDTO);
            return result;
        }
    }
}