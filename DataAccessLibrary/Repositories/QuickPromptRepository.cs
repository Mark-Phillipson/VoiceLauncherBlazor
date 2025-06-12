using System;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Repositories
{
    public class QuickPromptRepository : IQuickPromptRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public QuickPromptRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<QuickPromptDTO>> GetAllQuickPromptsAsync(int pageNumber, int pageSize, string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            List<QuickPrompt> quickPrompts;
            
            if (!string.IsNullOrWhiteSpace(serverSearchTerm))
            {
                quickPrompts = await context.QuickPrompts
                    .Where(q => q.IsActive && (
                        (q.Type != null && q.Type.ToLower().Contains(serverSearchTerm.ToLower())) ||
                        (q.Command != null && q.Command.ToLower().Contains(serverSearchTerm.ToLower())) ||
                        (q.PromptText != null && q.PromptText.ToLower().Contains(serverSearchTerm.ToLower())) ||
                        (q.Description != null && q.Description.ToLower().Contains(serverSearchTerm.ToLower()))
                    ))
                    .OrderBy(q => q.Type).ThenBy(q => q.Command)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                quickPrompts = await context.QuickPrompts
                    .Where(q => q.IsActive)
                    .OrderBy(q => q.Type).ThenBy(q => q.Command)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            
            IEnumerable<QuickPromptDTO> quickPromptsDTO = _mapper.Map<List<QuickPrompt>, IEnumerable<QuickPromptDTO>>(quickPrompts);
            return quickPromptsDTO;
        }

        public async Task<IEnumerable<QuickPromptDTO>> SearchQuickPromptsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var quickPrompts = await context.QuickPrompts
                .Where(q => q.IsActive && (
                    (q.Type != null && q.Type.ToLower().Contains(serverSearchTerm.ToLower())) ||
                    (q.Command != null && q.Command.ToLower().Contains(serverSearchTerm.ToLower())) ||
                    (q.PromptText != null && q.PromptText.ToLower().Contains(serverSearchTerm.ToLower())) ||
                    (q.Description != null && q.Description.ToLower().Contains(serverSearchTerm.ToLower()))
                ))
                .OrderBy(q => q.Type).ThenBy(q => q.Command)
                .Take(1000)
                .ToListAsync();
                
            IEnumerable<QuickPromptDTO> quickPromptsDTO = _mapper.Map<List<QuickPrompt>, IEnumerable<QuickPromptDTO>>(quickPrompts);
            return quickPromptsDTO;
        }

        public async Task<QuickPromptDTO?> GetQuickPromptByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.QuickPrompts.AsNoTracking()
              .FirstOrDefaultAsync(q => q.Id == Id);
            if (result == null) return null;
            QuickPromptDTO quickPromptDTO = _mapper.Map<QuickPrompt, QuickPromptDTO>(result);
            return quickPromptDTO;
        }

        public async Task<QuickPromptDTO?> AddQuickPromptAsync(QuickPromptDTO quickPromptDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            QuickPrompt quickPrompt = _mapper.Map<QuickPromptDTO, QuickPrompt>(quickPromptDTO);
            quickPrompt.CreatedDate = DateTime.UtcNow;
            
            var addedEntity = context.QuickPrompts.Add(quickPrompt);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            QuickPromptDTO resultDTO = _mapper.Map<QuickPrompt, QuickPromptDTO>(quickPrompt);
            return resultDTO;
        }

        public async Task<QuickPromptDTO?> UpdateQuickPromptAsync(QuickPromptDTO quickPromptDTO)
        {
            QuickPrompt quickPrompt = _mapper.Map<QuickPromptDTO, QuickPrompt>(quickPromptDTO);
            quickPrompt.LastModifiedDate = DateTime.UtcNow;
            
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundQuickPrompt = await context.QuickPrompts.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quickPrompt.Id);

                if (foundQuickPrompt != null)
                {
                    var mappedQuickPrompt = _mapper.Map<QuickPrompt>(quickPrompt);
                    context.QuickPrompts.Update(mappedQuickPrompt);
                    await context.SaveChangesAsync();
                    QuickPromptDTO resultDTO = _mapper.Map<QuickPrompt, QuickPromptDTO>(mappedQuickPrompt);
                    return resultDTO;
                }
            }
            return null;
        }

        public async Task DeleteQuickPromptAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundQuickPrompt = context.QuickPrompts.FirstOrDefault(q => q.Id == Id);
            if (foundQuickPrompt == null)
            {
                return;
            }
            context.QuickPrompts.Remove(foundQuickPrompt);
            await context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.QuickPrompts.Where(q => q.IsActive).CountAsync();
        }

        public async Task<IEnumerable<QuickPromptDTO>> GetQuickPromptsByTypeAsync(string type)
        {
            using var context = _contextFactory.CreateDbContext();
            var quickPrompts = await context.QuickPrompts
                .Where(q => q.IsActive && q.Type == type)
                .OrderBy(q => q.Command)
                .ToListAsync();
                
            IEnumerable<QuickPromptDTO> quickPromptsDTO = _mapper.Map<List<QuickPrompt>, IEnumerable<QuickPromptDTO>>(quickPrompts);
            return quickPromptsDTO;
        }

        public async Task<QuickPromptDTO?> GetQuickPromptByCommandAsync(string command)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.QuickPrompts.AsNoTracking()
                .FirstOrDefaultAsync(q => q.IsActive && q.Command == command);
            if (result == null) return null;
            QuickPromptDTO quickPromptDTO = _mapper.Map<QuickPrompt, QuickPromptDTO>(result);
            return quickPromptDTO;
        }
    }
}
