
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
    public class IdiosyncrasyRepository : IIdiosyncrasyRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public IdiosyncrasyRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
        public async Task<IEnumerable<IdiosyncrasyDTO>> GetAllIdiosyncrasiesAsync(int maxRows = 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var Idiosyncrasies = await context.Idiosyncrasies
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<IdiosyncrasyDTO> IdiosyncrasiesDTO = _mapper.Map<List<Idiosyncrasy>, IEnumerable<IdiosyncrasyDTO>>(Idiosyncrasies);
            return IdiosyncrasiesDTO;
        }
        public async Task<IEnumerable<IdiosyncrasyDTO>> SearchIdiosyncrasiesAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var Idiosyncrasies = await context.Idiosyncrasies
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<IdiosyncrasyDTO> IdiosyncrasiesDTO = _mapper.Map<List<Idiosyncrasy>, IEnumerable<IdiosyncrasyDTO>>(Idiosyncrasies);
            return IdiosyncrasiesDTO;
        }

        public async Task<IdiosyncrasyDTO?> GetIdiosyncrasyByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.Idiosyncrasies.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            IdiosyncrasyDTO idiosyncrasyDTO = _mapper.Map<Idiosyncrasy, IdiosyncrasyDTO>(result);
            return idiosyncrasyDTO;
        }

        public async Task<IdiosyncrasyDTO?> AddIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            Idiosyncrasy idiosyncrasy = _mapper.Map<IdiosyncrasyDTO, Idiosyncrasy>(idiosyncrasyDTO);
            var addedEntity = context.Idiosyncrasies.Add(idiosyncrasy);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            IdiosyncrasyDTO resultDTO = _mapper.Map<Idiosyncrasy, IdiosyncrasyDTO>(idiosyncrasy);
            return resultDTO;
        }

        public async Task<IdiosyncrasyDTO?> UpdateIdiosyncrasyAsync(IdiosyncrasyDTO idiosyncrasyDTO)
        {
            Idiosyncrasy idiosyncrasy = _mapper.Map<IdiosyncrasyDTO, Idiosyncrasy>(idiosyncrasyDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundIdiosyncrasy = await context.Idiosyncrasies.AsNoTracking().FirstOrDefaultAsync(e => e.Id == idiosyncrasy.Id);

                if (foundIdiosyncrasy != null)
                {
                    var mappedIdiosyncrasy = _mapper.Map<Idiosyncrasy>(idiosyncrasy);
                    context.Idiosyncrasies.Update(mappedIdiosyncrasy);
                    await context.SaveChangesAsync();
                    IdiosyncrasyDTO resultDTO = _mapper.Map<Idiosyncrasy, IdiosyncrasyDTO>(mappedIdiosyncrasy);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteIdiosyncrasyAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundIdiosyncrasy = context.Idiosyncrasies.FirstOrDefault(e => e.Id == Id);
            if (foundIdiosyncrasy == null)
            {
                return;
            }
            context.Idiosyncrasies.Remove(foundIdiosyncrasy);
            await context.SaveChangesAsync();
        }
    }
}