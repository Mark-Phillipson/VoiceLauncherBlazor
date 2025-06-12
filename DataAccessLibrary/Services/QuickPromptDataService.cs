using Ardalis.GuardClauses;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Repositories;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Services
{
    public class QuickPromptDataService : IQuickPromptDataService
    {
        private readonly IQuickPromptRepository _quickPromptRepository;

        public QuickPromptDataService(IQuickPromptRepository quickPromptRepository)
        {
            this._quickPromptRepository = quickPromptRepository;
        }

        public async Task<List<QuickPromptDTO>> GetAllQuickPromptsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            var quickPrompts = await _quickPromptRepository.GetAllQuickPromptsAsync(pageNumber, pageSize, serverSearchTerm);
            return quickPrompts.ToList();
        }

        public async Task<List<QuickPromptDTO>> SearchQuickPromptsAsync(string serverSearchTerm)
        {
            var quickPrompts = await _quickPromptRepository.SearchQuickPromptsAsync(serverSearchTerm);
            return quickPrompts.ToList();
        }

        public async Task<QuickPromptDTO?> GetQuickPromptById(int Id)
        {
            var quickPrompt = await _quickPromptRepository.GetQuickPromptByIdAsync(Id);
            return quickPrompt;
        }

        public async Task<QuickPromptDTO?> AddQuickPrompt(QuickPromptDTO quickPromptDTO)
        {
            Guard.Against.Null(quickPromptDTO);
            var result = await _quickPromptRepository.AddQuickPromptAsync(quickPromptDTO);
            if (result == null)
            {
                throw new Exception($"Add of quick prompt failed ID: {quickPromptDTO.Id}");
            }
            return result;
        }

        public async Task<QuickPromptDTO?> UpdateQuickPrompt(QuickPromptDTO quickPromptDTO, string? username)
        {
            Guard.Against.Null(quickPromptDTO);
            Guard.Against.Null(username);
            var result = await _quickPromptRepository.UpdateQuickPromptAsync(quickPromptDTO);
            if (result == null)
            {
                throw new Exception($"Update of quick prompt failed ID: {quickPromptDTO.Id}");
            }
            return result;
        }

        public async Task DeleteQuickPrompt(int Id)
        {
            await _quickPromptRepository.DeleteQuickPromptAsync(Id);
        }

        public async Task<int> GetTotalCount()
        {
            int result = await _quickPromptRepository.GetTotalCountAsync();
            return result;
        }

        public async Task<List<QuickPromptDTO>> GetQuickPromptsByType(string type)
        {
            var quickPrompts = await _quickPromptRepository.GetQuickPromptsByTypeAsync(type);
            return quickPrompts.ToList();
        }

        public async Task<QuickPromptDTO?> GetQuickPromptByCommand(string command)
        {
            var quickPrompt = await _quickPromptRepository.GetQuickPromptByCommandAsync(command);
            return quickPrompt;
        }

        public async Task<List<string>> GetQuickPromptTypes()
        {
            var allPrompts = await _quickPromptRepository.GetAllQuickPromptsAsync(1, 1000, "");
            var types = allPrompts.Select(p => p.Type).Distinct().OrderBy(t => t).ToList();
            return types;
        }
    }
}
