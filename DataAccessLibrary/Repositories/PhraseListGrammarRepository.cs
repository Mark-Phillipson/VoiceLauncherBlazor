
using AutoMapper;
using DataAccessLibrary.DTO;
using Microsoft.EntityFrameworkCore;

using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceLauncher.Repositories
{
    public class PhraseListGrammarRepository : IPhraseListGrammarRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public PhraseListGrammarRepository(IDbContextFactory<ApplicationDbContext> contextFactory,IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
		        public async Task<IEnumerable<PhraseListGrammarDTO>> GetAllPhraseListGrammarsAsync(int maxRows= 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var PhraseListGrammars= await context.PhraseListGrammars
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<PhraseListGrammarDTO> PhraseListGrammarsDTO = _mapper.Map<List<PhraseListGrammar>, IEnumerable<PhraseListGrammarDTO>>(PhraseListGrammars);
            return PhraseListGrammarsDTO;
        }
        public async Task<IEnumerable<PhraseListGrammarDTO>> SearchPhraseListGrammarsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var PhraseListGrammars= await context.PhraseListGrammars
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<PhraseListGrammarDTO> PhraseListGrammarsDTO = _mapper.Map<List<PhraseListGrammar>, IEnumerable<PhraseListGrammarDTO>>(PhraseListGrammars);
            return PhraseListGrammarsDTO;
        }

        public async Task<PhraseListGrammarDTO> GetPhraseListGrammarByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result =await context.PhraseListGrammars.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            PhraseListGrammarDTO phraseListGrammarDTO =_mapper.Map<PhraseListGrammar, PhraseListGrammarDTO>(result);
            return phraseListGrammarDTO;
        }

        public async Task<PhraseListGrammarDTO> AddPhraseListGrammarAsync(PhraseListGrammarDTO phraseListGrammarDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            PhraseListGrammar phraseListGrammar = _mapper.Map<PhraseListGrammarDTO, PhraseListGrammar>(phraseListGrammarDTO);
            var addedEntity = context.PhraseListGrammars.Add(phraseListGrammar);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            PhraseListGrammarDTO resultDTO =_mapper.Map<PhraseListGrammar, PhraseListGrammarDTO>(phraseListGrammar);
            return resultDTO;
        }

        public async Task<PhraseListGrammarDTO> UpdatePhraseListGrammarAsync(PhraseListGrammarDTO phraseListGrammarDTO)
        {
            PhraseListGrammar phraseListGrammar=_mapper.Map<PhraseListGrammarDTO, PhraseListGrammar>(phraseListGrammarDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundPhraseListGrammar = await context.PhraseListGrammars.AsNoTracking().FirstOrDefaultAsync(e => e.Id == phraseListGrammar.Id);

                if (foundPhraseListGrammar != null)
                {
                    var mappedPhraseListGrammar = _mapper.Map<PhraseListGrammar>(phraseListGrammar);
                    context.PhraseListGrammars.Update(mappedPhraseListGrammar);
                    await context.SaveChangesAsync();
                    PhraseListGrammarDTO resultDTO = _mapper.Map<PhraseListGrammar, PhraseListGrammarDTO>(mappedPhraseListGrammar);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeletePhraseListGrammarAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundPhraseListGrammar = context.PhraseListGrammars.FirstOrDefault(e => e.Id == Id);
            if (foundPhraseListGrammar == null)
            {
                return;
            }
            context.PhraseListGrammars.Remove(foundPhraseListGrammar);
            await context.SaveChangesAsync();
        }
    }
}