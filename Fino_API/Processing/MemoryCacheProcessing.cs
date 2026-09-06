using Microsoft.Extensions.Caching.Memory;

namespace FintechApp.Processing
{
    public class MemoryCacheProcessing<TItem>
    {
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public TItem? GetOrCreate(object key, Func<TItem> createItem)
        {
            TItem? cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                // Key not in cache, so get data
                cacheEntry = createItem();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Remove from cache after this time, regardless of sliding expiration
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(MemoryCacheSetting.ExpiryTimeInMinute * 60)); // 45 min

                // Save data in cache
                _cache.Set(key, cacheEntry, cacheEntryOptions);
            }
            return cacheEntry;
        }

        public TItem? OnlyGet(object key)
        {
            TItem? cacheEntry;
            _cache.TryGetValue(key, out cacheEntry);
            return cacheEntry;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Dispose();
            _cache = new MemoryCache(new MemoryCacheOptions());
        }
    }

    public class MemoryCacheSetting
    {
        public static int ExpiryTimeInMinute = 45;
    }
}
