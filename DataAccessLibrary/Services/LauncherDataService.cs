using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLibrary.Models;
using DataAccessLibrary.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace DataAccessLibrary.Services
{
    public class LauncherDataService : ILauncherDataService
    {
        private readonly ILauncherRepository _launcherRepository;
        private readonly IMemoryCache _cache;
        private const string LAUNCHERS_CACHE_KEY = "launchers_dto";
        private const string FAVORITE_LAUNCHERS_CACHE_KEY = "favorite_launchers_dto";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public LauncherDataService(ILauncherRepository launcherRepository, IMemoryCache cache)
        {
            _launcherRepository = launcherRepository;
            _cache = cache;
        }

        public async Task<List<LauncherDTO>> GetAllLaunchersAsync(int CategoryID)
        {
            string cacheKey = $"{LAUNCHERS_CACHE_KEY}_{CategoryID}";
            
            if (!_cache.TryGetValue(cacheKey, out List<LauncherDTO> launchers))
            {
                var result = await _launcherRepository.GetAllLaunchersAsync(CategoryID);
                launchers = result.ToList();
                _cache.Set(cacheKey, launchers, _cacheDuration);
            }
            
            return launchers;
        }

        public async Task<List<LauncherDTO>> SearchLaunchersAsync(string serverSearchTerm)
        {
            // Don't cache search results as they're likely to be different each time
            var launchers = await _launcherRepository.SearchLaunchersAsync(serverSearchTerm);
            return launchers.ToList();
        }

        public async Task<LauncherDTO?> GetLauncherById(int Id)
        {
            string cacheKey = $"{LAUNCHERS_CACHE_KEY}_item_{Id}";
            
            if (!_cache.TryGetValue(cacheKey, out LauncherDTO? launcher))
            {
                launcher = await _launcherRepository.GetLauncherByIdAsync(Id);
                if (launcher != null)
                {
                    _cache.Set(cacheKey, launcher, _cacheDuration);
                }
            }
            
            return launcher;
        }

        public async Task<List<LauncherDTO>> GetFavoriteLaunchersAsync()
        {
            if (!_cache.TryGetValue(FAVORITE_LAUNCHERS_CACHE_KEY, out List<LauncherDTO> launchers))
            {
                var result = await _launcherRepository.GetFavoriteLaunchersAsync();
                launchers = result.ToList();
                _cache.Set(FAVORITE_LAUNCHERS_CACHE_KEY, launchers, _cacheDuration);
            }
            
            return launchers;
        }

        public async Task<LauncherDTO?> AddLauncher(LauncherDTO launcherDTO)
        {
            Guard.Against.Null(launcherDTO);
            var result = await _launcherRepository.AddLauncherAsync(launcherDTO);
            if (result == null)
            {
                throw new Exception($"Add of launcher failed ID: {launcherDTO.Id}");
            }
            InvalidateCache();
            return result;
        }

        public async Task<LauncherDTO?> UpdateLauncher(LauncherDTO launcherDTO, string username)
        {
            Guard.Against.Null(launcherDTO);
            Guard.Against.Null(username);
            var result = await _launcherRepository.UpdateLauncherAsync(launcherDTO);
            if (result == null)
            {
                throw new Exception($"Update of launcher failed ID: {launcherDTO.Id}");
            }
            InvalidateCache();
            return result;
        }

        public async Task DeleteLauncher(int Id)
        {
            await _launcherRepository.DeleteLauncherAsync(Id);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            // Remove all launcher-related cache entries
            var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entries = field?.GetValue(_cache) as System.Collections.IDictionary;
            
            if (entries != null)
            {
                var keys = entries.Keys.Cast<object>()
                                .Where(k => k.ToString()?.StartsWith(LAUNCHERS_CACHE_KEY) == true ||
                                          k.ToString()?.StartsWith(FAVORITE_LAUNCHERS_CACHE_KEY) == true)
                                .ToList();
                
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
            }
        }
    }
}