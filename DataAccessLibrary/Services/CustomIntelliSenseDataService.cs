using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;


namespace VoiceLauncher.Services
{
    public class CustomIntelliSenseDataService : ICustomIntelliSenseDataService
    {
        private readonly ICustomIntelliSenseRepository _customIntelliSenseRepository;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "CustomIntelliSense_";
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public CustomIntelliSenseDataService(ICustomIntelliSenseRepository customIntelliSenseRepository, IMemoryCache cache)
        {
            _customIntelliSenseRepository = customIntelliSenseRepository;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))  // Increased from 10 to 30 minutes
                .SetAbsoluteExpiration(TimeSpan.FromHours(4));   // Increased from 1 to 4 hours
        }

        public async Task<List<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync(int LanguageId, int CategoryId, int pageNumber, int pageSize)
        {
            string cacheKey = $"{CacheKeyPrefix}{LanguageId}_{CategoryId}_{pageNumber}_{pageSize}";
            
            if (!_cache.TryGetValue(cacheKey, out List<CustomIntelliSenseDTO>? customIntelliSenses))
            {
                var result = await _customIntelliSenseRepository.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, pageNumber, pageSize);
                customIntelliSenses = result.ToList();
                
                // Only cache if we have data
                if (customIntelliSenses.Any())
                {
                    _cache.Set(cacheKey, customIntelliSenses, _cacheOptions);
                }
            }
            
            return customIntelliSenses ?? new List<CustomIntelliSenseDTO>();
        }        public async Task<List<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm, int? languageId = null, int? categoryId = null)
        {
            // Don't cache search results as they're likely to be unique
            var result = await _customIntelliSenseRepository.SearchCustomIntelliSensesAsync(serverSearchTerm, languageId, categoryId);
            return result.ToList();
        }

        public void InvalidateCache()
        {
            // Find and remove all CustomIntelliSense cache entries
            var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entries = field?.GetValue(_cache) as System.Collections.IDictionary;
            
            if (entries != null)
            {
                var keys = entries.Keys.Cast<object>()
                                .Where(k => k.ToString()?.StartsWith(CacheKeyPrefix) == true)
                                .ToList();
                
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
            }
        }

        public async Task<CustomIntelliSenseDTO?> GetCustomIntelliSenseById(int Id)
        {
            var customIntelliSense = await _customIntelliSenseRepository.GetCustomIntelliSenseByIdAsync(Id);
            return customIntelliSense;
        }
        public async Task<CustomIntelliSenseDTO?> AddCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO)
        {
            Guard.Against.Null(customIntelliSenseDTO);
            var result = await _customIntelliSenseRepository.AddCustomIntelliSenseAsync(customIntelliSenseDTO);
            if (result == null)
            {
                throw new Exception($"Add of customIntelliSense failed ID: {customIntelliSenseDTO.Id}");
            }
            return result;
        }
        public async Task<CustomIntelliSenseDTO?> UpdateCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO, string? username)
        {
            Guard.Against.Null(customIntelliSenseDTO);
            Guard.Against.Null(username);
            var result = await _customIntelliSenseRepository.UpdateCustomIntelliSenseAsync(customIntelliSenseDTO);
            if (result == null)
            {
                throw new Exception($"Update of customIntelliSense failed ID: {customIntelliSenseDTO.Id}");
            }
            return result;
        }

        public async Task DeleteCustomIntelliSense(int Id)
        {
            await _customIntelliSenseRepository.DeleteCustomIntelliSenseAsync(Id);
        }
        public void SendSnippet(string itemToCopyAndPaste, CustomIntelliSenseDTO CustomIntelliSenseDTO)
        {
            InputSimulator simulator = new InputSimulator();
            simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
            simulator.Keyboard.Sleep(300);
            simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            simulator.Keyboard.Sleep(300);
            simulator.Keyboard.TextEntry(itemToCopyAndPaste);
            if (CustomIntelliSenseDTO.SelectWordFromRight > 1)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                for (int i = 0; i < CustomIntelliSenseDTO.SelectWordFromRight; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
                simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            }
            if (CustomIntelliSenseDTO.SelectWordFromRight > 0)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                simulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                simulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            }
            if (CustomIntelliSenseDTO.MoveCharactersLeft > 0)
            {
                for (int i = 0; i < CustomIntelliSenseDTO.MoveCharactersLeft; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
            }
            if (CustomIntelliSenseDTO.SelectCharactersLeft > 0)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                for (int i = 0; i < CustomIntelliSenseDTO.SelectCharactersLeft; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
                simulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            }
        }
    }
}