using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleApplication.Repositories;


namespace SampleApplication.Services
{
    public class PromptDataService : IPromptDataService
    {
        private readonly IPromptRepository _promptRepository;

        public PromptDataService(IPromptRepository promptRepository)
        {
            this._promptRepository = promptRepository;
        }
        public async Task<List<PromptDTO>> GetAllPromptsAsync()
        {
            var Prompts = await _promptRepository.GetAllPromptsAsync(300);
            return Prompts.ToList();
        }
        public async Task<List<PromptDTO>> SearchPromptsAsync(string serverSearchTerm)
        {
            var Prompts = await _promptRepository.SearchPromptsAsync(serverSearchTerm);
            return Prompts.ToList();
        }

        public async Task<PromptDTO?> GetPromptById(int Id)
        {
            var prompt = await _promptRepository.GetPromptByIdAsync(Id);
            return prompt;
        }
        public async Task<PromptDTO?> AddPrompt(PromptDTO promptDTO)
        {
            Guard.Against.Null(promptDTO);
            var result = await _promptRepository.AddPromptAsync(promptDTO);
            if (result == null)
            {
                throw new Exception($"Add of prompt failed ID: {promptDTO.Id}");
            }
            return result;
        }
        public async Task<PromptDTO?> UpdatePrompt(PromptDTO promptDTO, string username)
        {
            Guard.Against.Null(promptDTO);
            Guard.Against.Null(username);
            var result = await _promptRepository.UpdatePromptAsync(promptDTO);
            if (result == null)
            {
                throw new Exception($"Update of prompt failed ID: {promptDTO.Id}");
            }
            return result;
        }

        public async Task DeletePrompt(int Id)
        {
            await _promptRepository.DeletePromptAsync(Id);
        }
    }
}