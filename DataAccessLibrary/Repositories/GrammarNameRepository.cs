
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using VoiceLauncher.DTOs;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceLauncher.Repositories
{
    public class GrammarNameRepository : IGrammarNameRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public GrammarNameRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
        public async Task<IEnumerable<GrammarNameDTO>> GetAllGrammarNamesAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var GrammarNames = await context.GrammarNames
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<GrammarNameDTO> GrammarNamesDTO = _mapper.Map<List<GrammarName>, IEnumerable<GrammarNameDTO>>(GrammarNames);
            return GrammarNamesDTO;
        }
        public async Task<IEnumerable<GrammarNameDTO>> SearchGrammarNamesAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var GrammarNames = await context.GrammarNames
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<GrammarNameDTO> GrammarNamesDTO = _mapper.Map<List<GrammarName>, IEnumerable<GrammarNameDTO>>(GrammarNames);
            return GrammarNamesDTO;
        }

        public async Task<GrammarNameDTO?> GetGrammarNameByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.GrammarNames.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            GrammarNameDTO grammarNameDTO = _mapper.Map<GrammarName, GrammarNameDTO>(result);
            return grammarNameDTO;
        }

        public async Task<GrammarNameDTO?> AddGrammarNameAsync(GrammarNameDTO grammarNameDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            GrammarName grammarName = _mapper.Map<GrammarNameDTO, GrammarName>(grammarNameDTO);
            var addedEntity = context.GrammarNames.Add(grammarName);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            GrammarNameDTO resultDTO = _mapper.Map<GrammarName, GrammarNameDTO>(grammarName);
            return resultDTO;
        }

        public async Task<GrammarNameDTO?> UpdateGrammarNameAsync(GrammarNameDTO grammarNameDTO)
        {
            GrammarName grammarName = _mapper.Map<GrammarNameDTO, GrammarName>(grammarNameDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundGrammarName = await context.GrammarNames.AsNoTracking().FirstOrDefaultAsync(e => e.Id == grammarName.Id);

                if (foundGrammarName != null)
                {
                    var mappedGrammarName = _mapper.Map<GrammarName>(grammarName);
                    context.GrammarNames.Update(mappedGrammarName);
                    await context.SaveChangesAsync();
                    GrammarNameDTO resultDTO = _mapper.Map<GrammarName, GrammarNameDTO>(mappedGrammarName);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteGrammarNameAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundGrammarName = context.GrammarNames.FirstOrDefault(e => e.Id == Id);
            if (foundGrammarName == null)
            {
                return;
            }
            context.GrammarNames.Remove(foundGrammarName);
            await context.SaveChangesAsync();
        }

        public  async Task<GrammarNameDTO> GetLatest()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var latest =await  context.GrammarNames.AsNoTracking().OrderByDescending(v => v.Id).FirstOrDefaultAsync();
                if (latest != null)
                {
                    GrammarNameDTO resultDTO = _mapper.Map<GrammarName, GrammarNameDTO>(latest);
                    return resultDTO;
                }
            }
            return null;
        }
    }
}