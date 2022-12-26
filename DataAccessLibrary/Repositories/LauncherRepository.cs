
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
    public class LauncherRepository : ILauncherRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public LauncherRepository(IDbContextFactory<ApplicationDbContext> contextFactory,IMapper mapper)
        {
            _contextFactory = contextFactory;
            this._mapper = mapper;
        }
		        public async Task<IEnumerable<LauncherDTO>> GetAllLaunchersAsync(int CategoryID)
        {
            using var context = _contextFactory.CreateDbContext();
            var Launchers= await context.Launcher
                .Where(v => v.CategoryId==CategoryID)
                .OrderBy(v => v.Name)
                .ToListAsync();
            IEnumerable<LauncherDTO> LaunchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(Launchers);
            return LaunchersDTO;
        }
        public async Task<IEnumerable<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var Launchers= await context.Launcher
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<LauncherDTO> LaunchersDTO = _mapper.Map<List<Launcher>, IEnumerable<LauncherDTO>>(Launchers);
            return LaunchersDTO;
        }

        public async Task<LauncherDTO> GetLauncherByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result =await context.Launcher.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            LauncherDTO launcherDTO=_mapper.Map<Launcher,LauncherDTO>(result);
            return launcherDTO;
        }

        public async Task<LauncherDTO> AddLauncherAsync(LauncherDTO launcherDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            Launcher launcher = _mapper.Map<LauncherDTO, Launcher>(launcherDTO);
            var addedEntity = context.Launcher.Add(launcher);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            LauncherDTO resultDTO=_mapper.Map<Launcher, LauncherDTO>(launcher);
            return resultDTO;
        }

        public async Task<LauncherDTO> UpdateLauncherAsync(LauncherDTO launcherDTO)
        {
            Launcher launcher=_mapper.Map<LauncherDTO, Launcher>(launcherDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundLauncher = await context.Launcher.AsNoTracking().FirstOrDefaultAsync(e => e.Id == launcher.Id);

                if (foundLauncher != null)
                {
                    var mappedLauncher = _mapper.Map<Launcher>(launcher);
                    context.Launcher.Update(mappedLauncher);
                    await context.SaveChangesAsync();
                    LauncherDTO resultDTO = _mapper.Map<Launcher, LauncherDTO>(mappedLauncher);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteLauncherAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundLauncher = context.Launcher.FirstOrDefault(e => e.Id == Id);
            if (foundLauncher == null)
            {
                return;
            }
            context.Launcher.Remove(foundLauncher);
            await context.SaveChangesAsync();
        }
    }
}