using AutoMapper;
using Microsoft.Extensions.Configuration;

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
      private readonly IConfiguration? _configuration;

      public CustomIntelliSenseRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IMapper mapper, IConfiguration? configuration = null)
      {
         _contextFactory = contextFactory;
         this._mapper = mapper;
         _configuration = configuration;
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
public async Task<IEnumerable<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm, string? languageFilter = null, string? categoryFilter = null)
      {
         using var context = _contextFactory.CreateDbContext();

         var searchTermLower = (serverSearchTerm ?? string.Empty).Trim();
         var normalizedSearchTerm = searchTermLower.ToLower();
         Console.WriteLine($"Search term after processing: '{normalizedSearchTerm}' (length: {normalizedSearchTerm.Length})");

         Console.WriteLine($"Starting global search for '{serverSearchTerm}' with languageFilter='{languageFilter}', categoryFilter='{categoryFilter}'");

         var query = context.CustomIntelliSenses
             .Include(x => x.Language)
             .Include(x => x.Category)
             .AsQueryable();

         var languageTerms = SplitFilterTerms(languageFilter);
         if (languageTerms.Count > 0)
         {
            var normalizedLanguageTerms = languageTerms
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();

            if (normalizedLanguageTerms.Count > 0)
            {
               query = query.Where(x =>
                   x.Language != null &&
                   x.Language.LanguageName != null &&
                   normalizedLanguageTerms.Any(term => x.Language.LanguageName.ToLower().Contains(term))
               );
               Console.WriteLine($"Applied language text filter: {string.Join(", ", normalizedLanguageTerms)}");
            }
         }

         var categoryTerms = SplitFilterTerms(categoryFilter);
         if (categoryTerms.Count > 0)
         {
            var normalizedCategoryTerms = categoryTerms
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();

            if (normalizedCategoryTerms.Count > 0)
            {
               query = query.Where(x =>
                   x.Category != null &&
                   x.Category.CategoryName != null &&
                   normalizedCategoryTerms.Any(term => x.Category.CategoryName.ToLower().Contains(term))
               );
               Console.WriteLine($"Applied category text filter: {string.Join(", ", normalizedCategoryTerms)}");
            }
         }

         if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
         {
            Console.WriteLine($"About to apply search filter for term: '{normalizedSearchTerm}'");

            query = query.Where(x =>
                (x.DisplayValue != null && x.DisplayValue.ToLower().Contains(normalizedSearchTerm)) ||
                (x.SendKeysValue != null && x.SendKeysValue.ToLower().Contains(normalizedSearchTerm)) ||
                (x.CommandType != null && x.CommandType.ToLower().Contains(normalizedSearchTerm)) ||
                (x.DeliveryType != null && x.DeliveryType.ToLower().Contains(normalizedSearchTerm))
            );
         }

         var customIntelliSenses = await query
             .OrderBy(v => v.DisplayValue)
             .Take(100)
             .AsNoTracking()
             .ToListAsync();

         Console.WriteLine($"DEBUG: Query returned {customIntelliSenses.Count} records total");

         var customIntelliSenseDtos = _mapper.Map<List<CustomIntelliSense>, IEnumerable<CustomIntelliSenseDTO>>(customIntelliSenses);

         foreach (var item in customIntelliSenseDtos)
         {
            var source = customIntelliSenses.FirstOrDefault(x => x.Id == item.Id);
            if (source != null)
            {
               item.LanguageName = source.Language?.LanguageName ?? string.Empty;
               item.CategoryName = source.Category?.CategoryName ?? string.Empty;
               item.Sensitive = source.Category?.Sensitive ?? false;
            }
         }

         return customIntelliSenseDtos;
      }

      private static List<string> SplitFilterTerms(string? filterText)
      {
         if (string.IsNullOrWhiteSpace(filterText))
         {
            return new List<string>();
         }

         return filterText
             .Split(new[] { ',', ';', '|', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
             .Where(t => !string.IsNullOrWhiteSpace(t))
             .ToList();
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