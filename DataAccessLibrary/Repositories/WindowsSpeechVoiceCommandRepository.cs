
using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Repositories
{
  public class WindowsSpeechVoiceCommandRepository : IWindowsSpeechVoiceCommandRepository
  {
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMapper _mapper;
    private readonly ICustomWindowsSpeechCommandRepository _customWindowsSpeechCommandRepository;

    public WindowsSpeechVoiceCommandRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper, ICustomWindowsSpeechCommandRepository customWindowsSpeechCommandRepository)
    {
      _contextFactory = contextFactory;
      _mapper = mapper;
      _customWindowsSpeechCommandRepository = customWindowsSpeechCommandRepository;
    }
    public async Task<IEnumerable<WindowsSpeechVoiceCommandDTO>> GetAllWindowsSpeechVoiceCommandsAsync(int maxRows = 400, bool showAutoCreated = false)
    {
      using var context = _contextFactory.CreateDbContext();
      var WindowsSpeechVoiceCommands = await context.WindowsSpeechVoiceCommands
        .Include(v => v.SpokenForms)
          .Where(v => v.AutoCreated == showAutoCreated)
          .OrderByDescending(v => v.Id)
          .Take(maxRows)
          .ToListAsync();
      IEnumerable<WindowsSpeechVoiceCommandDTO> WindowsSpeechVoiceCommandsDTO = _mapper.Map<List<WindowsSpeechVoiceCommand>, IEnumerable<WindowsSpeechVoiceCommandDTO>>(WindowsSpeechVoiceCommands);
      return WindowsSpeechVoiceCommandsDTO;
    }
    public IEnumerable<WindowsSpeechVoiceCommandDTO> SearchWindowsSpeechVoiceCommandsAsync(string serverSearchTerm)
    {
      using var context = _contextFactory.CreateDbContext();
      //var SpokenForms = context.SpokenForms
      //   .Where(v => v.SpokenFormText != null && v.SpokenFormText.ToLower().Contains(serverSearchTerm.ToLower()))
      //   .Select(v => v.Id)
      //   .ToArray();
      var WindowsSpeechVoiceCommands = context.WindowsSpeechVoiceCommands
        .Include( v => v.SpokenForms)
          .Where(v => v.ApplicationName != null && v.ApplicationName.ToLower().Contains(serverSearchTerm.ToLower())
          || v.SpokenForms.Any(s => s.SpokenFormText != null && s.SpokenFormText.ToLower().Contains(serverSearchTerm.ToLower()))
          )
          .Take(1000)
          .ToList();
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
      if (windowsSpeechVoiceCommandDTO.SendKeysValue != null)
      {
        CustomWindowsSpeechCommandDTO customWindowsSpeechCommandDTO = new CustomWindowsSpeechCommandDTO();
        customWindowsSpeechCommandDTO.SendKeysValue = windowsSpeechVoiceCommandDTO.SendKeysValue;
        customWindowsSpeechCommandDTO.WindowsSpeechVoiceCommandId = addedEntity.Entity.Id;
        var result = await _customWindowsSpeechCommandRepository.AddCustomWindowsSpeechCommandAsync(customWindowsSpeechCommandDTO);
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
      var latestWindowSpeechVoiceCommand = await context.WindowsSpeechVoiceCommands
          .AsNoTracking()
          .OrderByDescending(v => v.Id)
          .FirstOrDefaultAsync();
      WindowsSpeechVoiceCommandDTO resultDTO = _mapper.Map<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(latestWindowSpeechVoiceCommand);
      return resultDTO;
    }
    public async Task<List<ApplicationDetail>> GetAllApplicationDetails()
    {
      var context = _contextFactory.CreateDbContext();
      var applicationDetails = await context.ApplicationDetails
          .AsNoTracking()
          .OrderBy(v => v.ApplicationTitle).ToListAsync();
      return applicationDetails;
    }

    public async Task<List<CommandsBreakdown>> GetCommandsBreakdown()
    {
      var context = _contextFactory.CreateDbContext();
      List<WindowsSpeechVoiceCommand> result = await context.WindowsSpeechVoiceCommands
          .Where(v => v.ApplicationName.Length > 0).ToListAsync();
      var breakdown = result
         .GroupBy(a => new { a.ApplicationName, a.AutoCreated })
         .Select(g => new CommandsBreakdown
         {
           ApplicationName = g.Key.ApplicationName,
           AutoCreated = g.Key.AutoCreated,
           Number = g.Count()
         }).ToList();
      return breakdown;
    }
    public async Task<List<SpokenForm>> GetSpokenFormsAsync(List<WindowsSpeechVoiceCommandDTO> result)
    {
      var context = _contextFactory.CreateDbContext();
      List<SpokenForm> spokenForms =  new List<SpokenForm>();
      foreach (var item in result)
      {
        SpokenForm spokenForm = await context.SpokenForms
                         .AsNoTracking()
                                        .FirstOrDefaultAsync(v => v.WindowsSpeechVoiceCommandId == item.Id);
        spokenForms?.Add(spokenForm);
      }

      return spokenForms;
    }

  }
}