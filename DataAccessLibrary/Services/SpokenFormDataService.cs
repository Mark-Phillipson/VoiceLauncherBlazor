using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleApplication.Repositories;
using DataAccessLibrary.Services;
using DataAccessLibrary.DTO;

namespace SampleApplication.Services
{
    public class SpokenFormDataService : ISpokenFormDataService
    {
        private readonly ISpokenFormRepository _spokenFormRepository;

        public SpokenFormDataService(ISpokenFormRepository spokenFormRepository)
        {
            this._spokenFormRepository = spokenFormRepository;
        }
        public async Task<List<SpokenFormDTO>> GetAllSpokenFormsAsync(int WindowsSpeechVoiceCommandId)
        {
            var SpokenForms = await _spokenFormRepository.GetAllSpokenFormsAsync(WindowsSpeechVoiceCommandId);
            return SpokenForms.ToList();
        }
        public async Task<List<SpokenFormDTO>> SearchSpokenFormsAsync(string serverSearchTerm)
        {
            var SpokenForms = await _spokenFormRepository.SearchSpokenFormsAsync(serverSearchTerm);
            return SpokenForms.ToList();
        }

        public async Task<SpokenFormDTO?> GetSpokenFormById(int Id)
        {
            var spokenForm = await _spokenFormRepository.GetSpokenFormByIdAsync(Id);
            return spokenForm;
        }
        public async Task<SpokenFormDTO?> AddSpokenForm(SpokenFormDTO spokenFormDTO)
        {
            Guard.Against.Null(spokenFormDTO);
            var result = await _spokenFormRepository.AddSpokenFormAsync(spokenFormDTO);
            if (result == null)
            {
                throw new Exception($"Add of spokenForm failed ID: {spokenFormDTO.Id}");
            }
            return result;
        }
        public async Task<SpokenFormDTO?> UpdateSpokenForm(SpokenFormDTO spokenFormDTO, string? username)
        {
            Guard.Against.Null(spokenFormDTO);
            Guard.Against.Null(username);
            var result = await _spokenFormRepository.UpdateSpokenFormAsync(spokenFormDTO);
            if (result == null)
            {
                throw new Exception($"Update of spokenForm failed ID: {spokenFormDTO.Id}");
            }
            return result;
        }

        public async Task DeleteSpokenForm(int Id)
        {
            await _spokenFormRepository.DeleteSpokenFormAsync(Id);
        }
    }
}