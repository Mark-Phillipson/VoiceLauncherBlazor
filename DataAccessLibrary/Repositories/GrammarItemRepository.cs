
using AutoMapper;
using DataAccessLibrary.DTO;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceLauncher.Repositories
{
    public class GrammarItemRepository : IGrammarItemRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public GrammarItemRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
        public async Task<IEnumerable<GrammarItemDTO>> GetAllGrammarItemsAsync(int GrammarNameId)
        {
            using var context = _contextFactory.CreateDbContext();
            var GrammarItems = await context.GrammarItems
                .Where(v => v.GrammarNameId == GrammarNameId)
                //.OrderBy(v => v.?)
                .ToListAsync();
            IEnumerable<GrammarItemDTO> GrammarItemsDTO = _mapper.Map<List<GrammarItem>, IEnumerable<GrammarItemDTO>>(GrammarItems);
            return GrammarItemsDTO;
        }
        public async Task<IEnumerable<GrammarItemDTO>> SearchGrammarItemsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var GrammarItems = await context.GrammarItems
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<GrammarItemDTO> GrammarItemsDTO = _mapper.Map<List<GrammarItem>, IEnumerable<GrammarItemDTO>>(GrammarItems);
            return GrammarItemsDTO;
        }

        public async Task<GrammarItemDTO?> GetGrammarItemByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.GrammarItems.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            GrammarItemDTO grammarItemDTO = _mapper.Map<GrammarItem, GrammarItemDTO>(result);
            return grammarItemDTO;
        }

        public async Task<GrammarItemDTO?> AddGrammarItemAsync(GrammarItemDTO grammarItemDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            GrammarItem grammarItem = _mapper.Map<GrammarItemDTO, GrammarItem>(grammarItemDTO);
            var addedEntity = context.GrammarItems.Add(grammarItem);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            GrammarItemDTO resultDTO = _mapper.Map<GrammarItem, GrammarItemDTO>(grammarItem);
            return resultDTO;
        }

        public async Task<GrammarItemDTO?> UpdateGrammarItemAsync(GrammarItemDTO grammarItemDTO)
        {
            GrammarItem grammarItem = _mapper.Map<GrammarItemDTO, GrammarItem>(grammarItemDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundGrammarItem = await context.GrammarItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == grammarItem.Id);

                if (foundGrammarItem != null)
                {
                    var mappedGrammarItem = _mapper.Map<GrammarItem>(grammarItem);
                    context.GrammarItems.Update(mappedGrammarItem);
                    await context.SaveChangesAsync();
                    GrammarItemDTO resultDTO = _mapper.Map<GrammarItem, GrammarItemDTO>(mappedGrammarItem);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteGrammarItemAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundGrammarItem = context.GrammarItems.FirstOrDefault(e => e.Id == Id);
            if (foundGrammarItem == null)
            {
                return;
            }
            context.GrammarItems.Remove(foundGrammarItem);
            await context.SaveChangesAsync();
        }

        public async Task<bool> SaveAllAsync(List<GrammarItemDTO> grammarItemsDTO)
        {
            bool successful = true;
            using var context = _contextFactory.CreateDbContext();
            GrammarItemDTO? result;
            foreach (GrammarItemDTO grammarItem in grammarItemsDTO)
            {
                if (grammarItem.Id > 0)
                {
                    result = await UpdateGrammarItemAsync(grammarItem);
                }
                else
                {
                    result = await AddGrammarItemAsync(grammarItem);
                }
                if (result == null)
                {
                    successful = false;
                }
            }
            await context.SaveChangesAsync();
            return successful;
        }
    }
}