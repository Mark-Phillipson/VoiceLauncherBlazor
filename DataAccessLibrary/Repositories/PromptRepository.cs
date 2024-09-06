
using AutoMapper;
using DataAccessLibrary.DTO;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;

using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SampleApplication.Repositories
{
    public class PromptRepository : IPromptRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public PromptRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
        public async Task<IEnumerable<PromptDTO>> GetAllPromptsAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var Prompts = await context.Prompts

                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<PromptDTO> PromptsDTO = _mapper.Map<List<Prompt>, IEnumerable<PromptDTO>>(Prompts);
            return PromptsDTO;
        }
        public async Task<IEnumerable<PromptDTO>> SearchPromptsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var Prompts = await context.Prompts
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<PromptDTO> PromptsDTO = _mapper.Map<List<Prompt>, IEnumerable<PromptDTO>>(Prompts);
            return PromptsDTO;
        }

        public async Task<PromptDTO?> GetPromptByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.Prompts.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            PromptDTO promptDTO = _mapper.Map<Prompt, PromptDTO>(result);
            return promptDTO;
        }

        public async Task<PromptDTO?> AddPromptAsync(PromptDTO promptDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            Prompt prompt = _mapper.Map<PromptDTO, Prompt>(promptDTO);
            var addedEntity = context.Prompts.Add(prompt);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            PromptDTO resultDTO = _mapper.Map<Prompt, PromptDTO>(prompt);
            return resultDTO;
        }

        public async Task<PromptDTO?> UpdatePromptAsync(PromptDTO promptDTO)
        {
            Prompt prompt = _mapper.Map<PromptDTO, Prompt>(promptDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundPrompt = await context.Prompts.AsNoTracking().FirstOrDefaultAsync(e => e.Id == prompt.Id);

                if (foundPrompt != null)
                {
                    var mappedPrompt = _mapper.Map<Prompt>(prompt);
                    context.Prompts.Update(mappedPrompt);
                    await context.SaveChangesAsync();
                    PromptDTO resultDTO = _mapper.Map<Prompt, PromptDTO>(mappedPrompt);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeletePromptAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundPrompt = context.Prompts.FirstOrDefault(e => e.Id == Id);
            if (foundPrompt == null)
            {
                return;
            }
            context.Prompts.Remove(foundPrompt);
            await context.SaveChangesAsync();
        }
    }
}