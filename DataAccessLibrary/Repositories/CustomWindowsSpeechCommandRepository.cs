
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using DataAccessLibrary.DTO;

namespace DataAccessLibrary.Repositories
{
    public class CustomWindowsSpeechCommandRepository : ICustomWindowsSpeechCommandRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public CustomWindowsSpeechCommandRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CustomWindowsSpeechCommandDTO>> GetAllCustomWindowsSpeechCommandsAsync(int windowsSpeechVoiceCommandId)
        {
            using var context = _contextFactory.CreateDbContext();
            var CustomWindowsSpeechCommands = await context.CustomWindowsSpeechCommands
                .Where(v => v.WindowsSpeechVoiceCommandId == windowsSpeechVoiceCommandId)
                .ToListAsync();
            IEnumerable<CustomWindowsSpeechCommandDTO> CustomWindowsSpeechCommandsDTO = _mapper.Map<List<CustomWindowsSpeechCommand>, IEnumerable<CustomWindowsSpeechCommandDTO>>(CustomWindowsSpeechCommands);
            return CustomWindowsSpeechCommandsDTO;
        }
        public async Task<IEnumerable<CustomWindowsSpeechCommandDTO>> SearchCustomWindowsSpeechCommandsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var CustomWindowsSpeechCommands = await context.CustomWindowsSpeechCommands
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<CustomWindowsSpeechCommandDTO> CustomWindowsSpeechCommandsDTO = _mapper.Map<List<CustomWindowsSpeechCommand>, IEnumerable<CustomWindowsSpeechCommandDTO>>(CustomWindowsSpeechCommands);
            return CustomWindowsSpeechCommandsDTO;
        }

        public async Task<CustomWindowsSpeechCommandDTO?> GetCustomWindowsSpeechCommandByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result = await context.CustomWindowsSpeechCommands.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO = _mapper.Map<CustomWindowsSpeechCommand, CustomWindowsSpeechCommandDTO>(result);
            return customWindowsSpeechCommandDTO;
        }

        public async Task<CustomWindowsSpeechCommandDTO?> AddCustomWindowsSpeechCommandAsync(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            CustomWindowsSpeechCommand customWindowsSpeechCommand = _mapper.Map<CustomWindowsSpeechCommandDTO, CustomWindowsSpeechCommand>(customWindowsSpeechCommandDTO);
            var addedEntity = context.CustomWindowsSpeechCommands.Add(customWindowsSpeechCommand);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            CustomWindowsSpeechCommandDTO resultDTO = _mapper.Map<CustomWindowsSpeechCommand, CustomWindowsSpeechCommandDTO>(customWindowsSpeechCommand);
            return resultDTO;
        }

        public async Task<CustomWindowsSpeechCommandDTO?> UpdateCustomWindowsSpeechCommandAsync(CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO)
        {
            CustomWindowsSpeechCommand customWindowsSpeechCommand = _mapper.Map<CustomWindowsSpeechCommandDTO, CustomWindowsSpeechCommand>(customWindowsSpeechCommandDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundCustomWindowsSpeechCommand = await context.CustomWindowsSpeechCommands.AsNoTracking().FirstOrDefaultAsync(e => e.Id == customWindowsSpeechCommand.Id);

                if (foundCustomWindowsSpeechCommand != null)
                {
                    var mappedCustomWindowsSpeechCommand = _mapper.Map<CustomWindowsSpeechCommand>(customWindowsSpeechCommand);
                    context.CustomWindowsSpeechCommands.Update(mappedCustomWindowsSpeechCommand);
                    await context.SaveChangesAsync();
                    CustomWindowsSpeechCommandDTO resultDTO = _mapper.Map<CustomWindowsSpeechCommand, CustomWindowsSpeechCommandDTO>(mappedCustomWindowsSpeechCommand);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteCustomWindowsSpeechCommandAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundCustomWindowsSpeechCommand = context.CustomWindowsSpeechCommands.FirstOrDefault(e => e.Id == Id);
            if (foundCustomWindowsSpeechCommand == null)
            {
                return;
            }
            context.CustomWindowsSpeechCommands.Remove(foundCustomWindowsSpeechCommand);
            await context.SaveChangesAsync();
        }
    }
}