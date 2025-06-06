using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceLauncher.Repositories
{
   public class CustomIntelliSenseRepository : ICustomIntelliSenseRepository
   {
      private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
      private readonly IMapper _mapper;

      public CustomIntelliSenseRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper)
      {
         _contextFactory = contextFactory;
         this._mapper = mapper;
      }
      public async Task<IEnumerable<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync(int LanguageId, int CategoryId, int pageNumber, int pageSize)
      {
         using var context = _contextFactory.CreateDbContext();
    
         // Get total count for the given filters
         var totalCount = await context.CustomIntelliSenses
             .Where(v => v.CategoryId == CategoryId && v.LanguageId == LanguageId)
             .CountAsync();

         // Get page of data with related entities in a single query
         var customIntelliSenses = await context.CustomIntelliSenses
             .Where(v => v.CategoryId == CategoryId && v.LanguageId == LanguageId)
             .Include(x => x.Language)
             .Include(x => x.Category)
             .OrderBy(x => x.DisplayValue)
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
             .AsNoTracking() // Optimization since we're only reading
             .ToListAsync();

         // Map to DTOs - no need for additional queries since related data is included
         var customIntelliSenseDTOs = _mapper.Map<List<CustomIntelliSense>, IEnumerable<CustomIntelliSenseDTO>>(customIntelliSenses);
    
         // Set the total count in the first DTO for the UI to use
         if (customIntelliSenseDTOs.Any())
         {
             customIntelliSenseDTOs.First().TotalCount = totalCount;
         }

         return customIntelliSenseDTOs;
      }
      public async Task<IEnumerable<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm)
      {
         using var context = _contextFactory.CreateDbContext();
         var CustomIntelliSenses = await context.CustomIntelliSenses
             .Where(x => x.DisplayValue != null && x.DisplayValue.ToLower().Contains(serverSearchTerm.ToLower()))
             .OrderBy(v => v.DisplayValue)
             .Take(1000)
             .ToListAsync();
         IEnumerable<CustomIntelliSenseDTO> CustomIntelliSensesDTO = _mapper.Map<List<CustomIntelliSense>, IEnumerable<CustomIntelliSenseDTO>>(CustomIntelliSenses);
         foreach (var item in CustomIntelliSensesDTO)
         {
            var language = context.Languages.Where(x => x.Id == item.LanguageId).FirstOrDefault();
            if (language != null)
            {
               item.LanguageName = language.LanguageName;
            }
            var category = context.Categories.Where(x => x.Id == item.CategoryId).FirstOrDefault();
            if (category != null)
            {
               item.CategoryName = category.CategoryName;
            }
            item.Sensitive = context.Categories.Where(x => x.Id == item.CategoryId).Select(x => x.Sensitive).FirstOrDefault();
         }
         return CustomIntelliSensesDTO;
      }

      public async Task<CustomIntelliSenseDTO?> GetCustomIntelliSenseByIdAsync(int Id)
      {
         using var context = _contextFactory.CreateDbContext();
         var result = await context.CustomIntelliSenses.AsNoTracking()
           .FirstOrDefaultAsync(c => c.Id == Id);
         if (result == null) return null;
         CustomIntelliSenseDTO customIntelliSenseDTO = _mapper.Map<CustomIntelliSense, CustomIntelliSenseDTO>(result);
         return customIntelliSenseDTO;
      }

      public async Task<CustomIntelliSenseDTO?> AddCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO)
      {
         using var context = _contextFactory.CreateDbContext();
         CustomIntelliSense customIntelliSense = _mapper.Map<CustomIntelliSenseDTO, CustomIntelliSense>(customIntelliSenseDTO);
         if (customIntelliSense.LanguageId == 0)
         {
            customIntelliSense.LanguageId = 1;//Will be not applicable by default
         }
         var addedEntity = context.CustomIntelliSenses.Add(customIntelliSense);

         try
         {
            await context.SaveChangesAsync();
         }
         catch (Exception exception)
         {
            Console.WriteLine(exception.Message);
            return null;
         }
         CustomIntelliSenseDTO resultDTO = _mapper.Map<CustomIntelliSense, CustomIntelliSenseDTO>(customIntelliSense);
         return resultDTO;
      }

      public async Task<CustomIntelliSenseDTO?> UpdateCustomIntelliSenseAsync(CustomIntelliSenseDTO customIntelliSenseDTO)
      {
         CustomIntelliSense customIntelliSense = _mapper.Map<CustomIntelliSenseDTO, CustomIntelliSense>(customIntelliSenseDTO);
         using (var context = _contextFactory.CreateDbContext())
         {
            var foundCustomIntelliSense = await context.CustomIntelliSenses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == customIntelliSense.Id);

            if (foundCustomIntelliSense != null)
            {
               var mappedCustomIntelliSense = _mapper.Map<CustomIntelliSense>(customIntelliSense);
               context.CustomIntelliSenses.Update(mappedCustomIntelliSense);
               await context.SaveChangesAsync();
               CustomIntelliSenseDTO resultDTO = _mapper.Map<CustomIntelliSense, CustomIntelliSenseDTO>(mappedCustomIntelliSense);
               return resultDTO;
            }
         }
         return null;
      }
      public async Task DeleteCustomIntelliSenseAsync(int Id)
      {
         using var context = _contextFactory.CreateDbContext();
         var foundCustomIntelliSense = context.CustomIntelliSenses.FirstOrDefault(e => e.Id == Id);
         if (foundCustomIntelliSense == null)
         {
            return;
         }
         var additionalCommands = await context.AdditionalCommands.Where(e => e.CustomIntelliSenseId == Id).ToListAsync();
         if (additionalCommands != null)
         {
            context.AdditionalCommands.RemoveRange(additionalCommands);
         }
         context.CustomIntelliSenses.Remove(foundCustomIntelliSense);
         await context.SaveChangesAsync();
      }
   }
}