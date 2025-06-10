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
      }      public async Task<IEnumerable<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm, int? languageId = null, int? categoryId = null)
      {
         using var context = _contextFactory.CreateDbContext();
         
         var searchTermLower = serverSearchTerm.ToLower().Trim();
         Console.WriteLine($"Search term after processing: '{searchTermLower}' (length: {searchTermLower.Length})");
         
         Console.WriteLine($"Starting global search for '{serverSearchTerm}' with languageId={languageId}, categoryId={categoryId}");
             // Build the query with proper filtering - same logic as the main service
         var query = context.CustomIntelliSenses
             .Include(x => x.Language)
             .Include(x => x.Category)
             .AsQueryable();
             
         // Check total count before any filtering
         var totalRecords = await query.CountAsync();
         Console.WriteLine($"Total records in database: {totalRecords}");
           // For global search (no specific language/category), be more permissive with filtering
         // Only apply filters if we're doing a scoped search
         if (languageId.HasValue && languageId.Value > 0)
         {
             query = query.Where(x => x.LanguageId == languageId.Value);
             Console.WriteLine($"Applied language filter: {languageId}");
             
             // Apply active language filter only when filtering by language
             query = query.Where(x => x.Language != null && x.Language.Active);
             Console.WriteLine("Applied active language filter");
         }
         
         if (categoryId.HasValue && categoryId.Value > 0)
         {
             query = query.Where(x => x.CategoryId == categoryId.Value);
             Console.WriteLine($"Applied category filter: {categoryId}");
             
             // Filter out sensitive categories only when filtering by category
             if (Environment.MachineName != "J40L4V3")
             {
                 query = query.Where(x => x.Category != null && x.Category.Sensitive == false);
                 Console.WriteLine("Applied sensitive category filter");
             }
         }
         
         // For true global search (no languageId or categoryId), apply minimal filtering
         if (!languageId.HasValue && !categoryId.HasValue)
         {
             Console.WriteLine("Global search mode - applying minimal filtering");
             // Only filter out obviously inactive data, but be permissive
         }         // Apply the actual search but be very permissive
         Console.WriteLine($"About to apply search filter for term: '{searchTermLower}'");
         
         // Try a very simple search first - just DisplayValue
         var searchQuery = query.Where(x => 
             x.DisplayValue != null && x.DisplayValue.ToLower().Contains(searchTermLower)
         );
         
         Console.WriteLine($"Search query built for DisplayValue only");
         
         // Check how many records match the search before taking results
         var searchMatchCount = await searchQuery.CountAsync();
         Console.WriteLine($"Records matching search term '{searchTermLower}' in DisplayValue: {searchMatchCount}");
         
         // If no results in DisplayValue, try other fields too
         if (searchMatchCount == 0)
         {
             Console.WriteLine($"No DisplayValue matches, trying all fields...");
             searchQuery = query.Where(x => 
                 (x.DisplayValue != null && x.DisplayValue.ToLower().Contains(searchTermLower)) ||
                 (x.SendKeysValue != null && x.SendKeysValue.ToLower().Contains(searchTermLower)) ||
                 (x.CommandType != null && x.CommandType.ToLower().Contains(searchTermLower)) ||
                 (x.DeliveryType != null && x.DeliveryType.ToLower().Contains(searchTermLower))
             );
             
             searchMatchCount = await searchQuery.CountAsync();
             Console.WriteLine($"Records matching search term '{searchTermLower}' in any field: {searchMatchCount}");
         }
         
         var CustomIntelliSenses = await searchQuery
             .OrderBy(v => v.DisplayValue)
             .Take(100) // Reasonable limit for global search
             .AsNoTracking()
             .ToListAsync();
         
         Console.WriteLine($"DEBUG: Query returned {CustomIntelliSenses.Count} records total");
         foreach (var item in CustomIntelliSenses)
         {
             var sendKeysLength = item.SendKeysValue?.Length ?? 0;
             var sendKeysPreview = sendKeysLength > 0 && item.SendKeysValue != null 
                 ? item.SendKeysValue.Substring(0, Math.Min(30, sendKeysLength)) 
                 : "";
             Console.WriteLine($"  - ID: {item.Id}, Display: '{item.DisplayValue}', SendKeys: '{sendKeysPreview}'");
         }
         
         Console.WriteLine($"=== SEARCH DEBUG END ===");
             
         // Map to DTOs - related data is already included
         var CustomIntelliSensesDTO = _mapper.Map<List<CustomIntelliSense>, IEnumerable<CustomIntelliSenseDTO>>(CustomIntelliSenses);
           // Set additional properties from the included relationships
         foreach (var item in CustomIntelliSensesDTO)
         {
            var source = CustomIntelliSenses.FirstOrDefault(x => x.Id == item.Id);
            if (source != null)
            {
               item.LanguageName = source.Language?.LanguageName ?? string.Empty;
               item.CategoryName = source.Category?.CategoryName ?? string.Empty;
               item.Sensitive = source.Category?.Sensitive ?? false;
            }
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