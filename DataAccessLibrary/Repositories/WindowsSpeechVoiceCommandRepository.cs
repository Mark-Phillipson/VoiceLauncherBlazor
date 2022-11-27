
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace DataAccessLibrary.Repositories
{
    public class WindowsSpeechVoiceCommandRepository : IWindowsSpeechVoiceCommandRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly ICustomWindowsSpeechCommandRepository _customWindowsSpeechCommandRepository;

        public WindowsSpeechVoiceCommandRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper,ICustomWindowsSpeechCommandRepository customWindowsSpeechCommandRepository)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _customWindowsSpeechCommandRepository = customWindowsSpeechCommandRepository;
        }
        public async Task<IEnumerable<WindowsSpeechVoiceCommandDTO>> GetAllWindowsSpeechVoiceCommandsAsync(int maxRows = 400, bool showAutoCreated= false )
        {
            using var context = _contextFactory.CreateDbContext();
            var WindowsSpeechVoiceCommands = await context.WindowsSpeechVoiceCommands
                .Where(v =>  v.AutoCreated==showAutoCreated)
                .OrderByDescending(v => v.Id)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<WindowsSpeechVoiceCommandDTO> WindowsSpeechVoiceCommandsDTO = _mapper.Map<List<WindowsSpeechVoiceCommand>, IEnumerable<WindowsSpeechVoiceCommandDTO>>(WindowsSpeechVoiceCommands);
            return WindowsSpeechVoiceCommandsDTO;
        }
        public async Task<IEnumerable<WindowsSpeechVoiceCommandDTO>> SearchWindowsSpeechVoiceCommandsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var WindowsSpeechVoiceCommands = await context.WindowsSpeechVoiceCommands
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<WindowsSpeechVoiceCommandDTO> WindowsSpeechVoiceCommandsDTO = _mapper.Map<List<WindowsSpeechVoiceCommand>, IEnumerable<WindowsSpeechVoiceCommandDTO>>(WindowsSpeechVoiceCommands);
            return WindowsSpeechVoiceCommandsDTO;
        }

        public async Task<WindowsSpeechVoiceCommandDTO> GetWindowsSpeechVoiceCommandByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.WindowsSpeechVoiceCommands.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO = _mapper.Map<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(result);
            return windowsSpeechVoiceCommandDTO;
        }

        public async Task<WindowsSpeechVoiceCommandDTO> AddWindowsSpeechVoiceCommandAsync(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            WindowsSpeechVoiceCommand windowsSpeechVoiceCommand = _mapper.Map<WindowsSpeechVoiceCommandDTO, WindowsSpeechVoiceCommand>(windowsSpeechVoiceCommandDTO);
            var addedEntity = context.WindowsSpeechVoiceCommands.Add(windowsSpeechVoiceCommand);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            if (windowsSpeechVoiceCommandDTO.SendKeysValue!= null )
            {
                CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO= new CustomWindowsSpeechCommandDTO();
                customWindowsSpeechCommandDTO.SendKeysValue= windowsSpeechVoiceCommandDTO.SendKeysValue;
                customWindowsSpeechCommandDTO.WindowsSpeechVoiceCommandId = addedEntity.Entity.Id;
                var result= await _customWindowsSpeechCommandRepository.AddCustomWindowsSpeechCommandAsync(customWindowsSpeechCommandDTO);
            }
            WindowsSpeechVoiceCommandDTO resultDTO = _mapper.Map<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(windowsSpeechVoiceCommand);
            return resultDTO;
        }

        public async Task<WindowsSpeechVoiceCommandDTO> UpdateWindowsSpeechVoiceCommandAsync(WindowsSpeechVoiceCommandDTO windowsSpeechVoiceCommandDTO)
        {
            WindowsSpeechVoiceCommand windowsSpeechVoiceCommand = _mapper.Map<WindowsSpeechVoiceCommandDTO, WindowsSpeechVoiceCommand>(windowsSpeechVoiceCommandDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundWindowsSpeechVoiceCommand = await context.WindowsSpeechVoiceCommands.AsNoTracking().FirstOrDefaultAsync(e => e.Id == windowsSpeechVoiceCommand.Id);

                if (foundWindowsSpeechVoiceCommand != null)
                {
                    var mappedWindowsSpeechVoiceCommand = _mapper.Map<WindowsSpeechVoiceCommand>(windowsSpeechVoiceCommand);
                    context.WindowsSpeechVoiceCommands.Update(mappedWindowsSpeechVoiceCommand);
                    await context.SaveChangesAsync();
                    WindowsSpeechVoiceCommandDTO resultDTO = _mapper.Map<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(mappedWindowsSpeechVoiceCommand);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteWindowsSpeechVoiceCommandAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundWindowsSpeechVoiceCommand = context.WindowsSpeechVoiceCommands.FirstOrDefault(e => e.Id == Id);
            if (foundWindowsSpeechVoiceCommand == null)
            {
                return;
            }
            context.WindowsSpeechVoiceCommands.Remove(foundWindowsSpeechVoiceCommand);
            await context.SaveChangesAsync();
        }
        public async Task<WindowsSpeechVoiceCommandDTO> GetLatestAdded()
        {
            var context = _contextFactory.CreateDbContext();
            var latestWindowSpeechVoiceCommand = await  context.WindowsSpeechVoiceCommands
                .AsNoTracking()
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync();
            WindowsSpeechVoiceCommandDTO resultDTO = _mapper.Map<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(latestWindowSpeechVoiceCommand);
            return resultDTO;
        }
        public async Task<List<ApplicationDetail>> GetAllApplicationDetails()
        {
             var context=_contextFactory.CreateDbContext();
            var applicationDetails = await context.ApplicationDetails
                .AsNoTracking()
                .OrderBy(v => v.ApplicationTitle).ToListAsync();
             return applicationDetails;
        }

	}
}