using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace RazorClassLibrary.Services
{
    public class ComponentCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public ComponentCacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        }

        public T? GetOrSet<T>(string key, Func<T> factory)
        {
            if (!_cache.TryGetValue(key, out T? value))
            {
                value = factory();
                if (value != null)
                {
                    _cache.Set(key, value, _cacheOptions);
                }
            }
            return value;
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
        {
            if (!_cache.TryGetValue(key, out T? value))
            {
                value = await factory();
                if (value != null)
                {
                    _cache.Set(key, value, _cacheOptions);
                }
            }
            return value;
        }

        public void InvalidateCache(string prefix)
        {
            // Find and remove all cache entries that start with the given prefix
            var field = typeof(MemoryCache).GetField("_entries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var entries = field?.GetValue(_cache) as System.Collections.IDictionary;
            
            if (entries != null)
            {
                var keys = entries.Keys.Cast<object>()
                    .Where(k => k.ToString()?.StartsWith(prefix) == true)
                    .ToList();
                
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
            }
        }
    }
}
