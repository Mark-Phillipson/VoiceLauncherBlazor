
using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApplication.Repositories
{
  public class SpokenFormRepository : ISpokenFormRepository
  {
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMapper _mapper;

    public SpokenFormRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
    {
      _contextFactory = contextFactory;
      this._mapper = mapper;
    }
    public async Task<IEnumerable<SpokenFormDTO>> GetAllSpokenFormsAsync(int WindowsSpeechVoiceCommandId)
    {
      using var context = _contextFactory.CreateDbContext();
      var SpokenForms = await context.SpokenForms
          .Where(v => v.WindowsSpeechVoiceCommandId == WindowsSpeechVoiceCommandId)
          //.OrderBy(v => v.?)
          .ToListAsync();
      IEnumerable<SpokenFormDTO> SpokenFormsDTO = _mapper.Map<List<SpokenForm>, IEnumerable<SpokenFormDTO>>(SpokenForms);
      return SpokenFormsDTO;
    }
    public async Task<IEnumerable<SpokenFormDTO>> SearchSpokenFormsAsync(string serverSearchTerm)
    {
      using var context = _contextFactory.CreateDbContext();
      var SpokenForms = await context.SpokenForms
          //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
          //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
          //)
          //.OrderBy(v => v.?)
          .Take(1000)
          .ToListAsync();
      IEnumerable<SpokenFormDTO> SpokenFormsDTO = _mapper.Map<List<SpokenForm>, IEnumerable<SpokenFormDTO>>(SpokenForms);
      return SpokenFormsDTO;
    }

    public async Task<SpokenFormDTO> GetSpokenFormByIdAsync(int Id)
    {
      using var context = _contextFactory.CreateDbContext();
      var result = await context.SpokenForms.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == Id);
      if (result == null) return new SpokenFormDTO();
      SpokenFormDTO spokenFormDTO = _mapper.Map<SpokenForm, SpokenFormDTO>(result);
      return spokenFormDTO;
    }

    public async Task<SpokenFormDTO> AddSpokenFormAsync(SpokenFormDTO spokenFormDTO)
    {
      using var context = _contextFactory.CreateDbContext();
      SpokenForm spokenForm = _mapper.Map<SpokenFormDTO, SpokenForm>(spokenFormDTO);
      var addedEntity = context.SpokenForms.Add(spokenForm);
      try
      {
        await context.SaveChangesAsync();
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception.Message);
        return new SpokenFormDTO();
      }
      SpokenFormDTO resultDTO = _mapper.Map<SpokenForm, SpokenFormDTO>(spokenForm);
      return resultDTO;
    }

    public async Task<SpokenFormDTO> UpdateSpokenFormAsync(SpokenFormDTO spokenFormDTO)
    {
      SpokenForm spokenForm = _mapper.Map<SpokenFormDTO, SpokenForm>(spokenFormDTO);
      using (var context = _contextFactory.CreateDbContext())
      {
        var foundSpokenForm = await context.SpokenForms.AsNoTracking().FirstOrDefaultAsync(e => e.Id == spokenForm.Id);

        if (foundSpokenForm != null)
        {
          var mappedSpokenForm = _mapper.Map<SpokenForm>(spokenForm);
          context.SpokenForms.Update(mappedSpokenForm);
          await context.SaveChangesAsync();
          SpokenFormDTO resultDTO = _mapper.Map<SpokenForm, SpokenFormDTO>(mappedSpokenForm);
          return resultDTO;
        }
      }
      return new SpokenFormDTO();
    }
    public async Task DeleteSpokenFormAsync(int Id)
    {
      using var context = _contextFactory.CreateDbContext();
      var foundSpokenForm = context.SpokenForms.FirstOrDefault(e => e.Id == Id);
      if (foundSpokenForm == null)
      {
        return;
      }
      context.SpokenForms.Remove(foundSpokenForm);
      await context.SaveChangesAsync();
    }
  }
}