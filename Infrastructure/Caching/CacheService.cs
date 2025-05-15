// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoubDownloader.Infrastructure.Caching;

/// <summary>In-memory caching service with TTL support</summary>
public interface ICacheService
{
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    bool TryGet<T>(string key, out T? value);
    T? Get<T>(string key);
    void Remove(string key);
    void Clear();
    CacheStatistics GetStatistics();
}

/// <summary>In-memory cache implementation with time-based expiration</summary>
public class MemoryCacheService : ICacheService
{
    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccessCount { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpirationTime;
    }

    private readonly Dictionary<string, CacheEntry> _cache = [];
    private readonly object _lockObj = new();
    private readonly TimeSpan _defaultTtl;

    private int _hits;
    private int _misses;

    /// <summary>Initialize cache with default TTL</summary>
    public MemoryCacheService(int defaultTtlSeconds = 3600)
    {
        _defaultTtl = TimeSpan.FromSeconds(defaultTtlSeconds);
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        lock (_lockObj)
        {
            var expiration = DateTime.UtcNow.Add(ttl ?? _defaultTtl);
            _cache[key] = new CacheEntry
            {
                Value = value,
                ExpirationTime = expiration,
                CreatedAt = DateTime.UtcNow,
                AccessCount = 0
            };

            CleanupExpiredEntries();
        }
    }

    public bool TryGet<T>(string key, out T? value)
    {
        lock (_lockObj)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired)
                {
                    _cache.Remove(key);
                    _misses++;
                    value = default;
                    return false;
                }

                entry.AccessCount++;
                _hits++;
                value = (T?)entry.Value;
                return true;
            }

            _misses++;
            value = default;
            return false;
        }
    }

    public T? Get<T>(string key)
    {
        TryGet<T>(key, out var value);
        return value;
    }

    public void Remove(string key)
    {
        lock (_lockObj)
        {
            _cache.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_lockObj)
        {
            _cache.Clear();
        }
    }

    public CacheStatistics GetStatistics()
    {
        lock (_lockObj)
        {
            var total = _hits + _misses;
            return new CacheStatistics
            {
                TotalEntries = _cache.Count,
                Hits = _hits,
                Misses = _misses,
                HitRate = total > 0 ? (double)_hits / total : 0,
                Size = _cache.Values.Sum(e => EstimateSize(e.Value))
            };
        }
    }

    private void CleanupExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
            _cache.Remove(key);
    }

    private long EstimateSize(object? obj)
    {
        if (obj == null) return 0;
        if (obj is string str) return str.Length * 2;
        return 1024; // Rough estimate
    }
}

/// <summary>Cache statistics for monitoring</summary>
public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public int Hits { get; set; }
    public int Misses { get; set; }
    public double HitRate { get; set; }
    public long Size { get; set; }
}

/// <summary>Distributed cache adapter for multi-instance scenarios</summary>
public class DistributedCacheAdapter : ICacheService
{
    private readonly ICacheService _localCache;
    private readonly List<ICacheService> _remoteCaches = [];

    public DistributedCacheAdapter(ICacheService localCache)
    {
        _localCache = localCache;
    }

    public void AddRemoteCache(ICacheService remoteCache)
    {
        _remoteCaches.Add(remoteCache);
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        _localCache.Set(key, value, ttl);

        foreach (var remote in _remoteCaches)
        {
            try
            {
                remote.Set(key, value, ttl);
            }
            catch
            {
                // Silently fail on remote cache write
            }
        }
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_localCache.TryGet(key, out value))
            return true;

        foreach (var remote in _remoteCaches)
        {
            try
            {
                if (remote.TryGet(key, out T? remoteValue))
                {
                    _localCache.Set(key, remoteValue); // Cache locally
                    value = remoteValue;
                    return true;
                }
            }
            catch
            {
                // Continue to next remote cache
            }
        }

        return false;
    }

    public T? Get<T>(string key)
    {
        TryGet(key, out T? value);
        return value;
    }

    public void Remove(string key)
    {
        _localCache.Remove(key);
        foreach (var remote in _remoteCaches)
        {
            try
            {
                remote.Remove(key);
            }
            catch
            {
                // Silently fail
            }
        }
    }

    public void Clear()
    {
        _localCache.Clear();
        foreach (var remote in _remoteCaches)
        {
            try
            {
                remote.Clear();
            }
            catch
            {
                // Silently fail
            }
        }
    }

    public CacheStatistics GetStatistics()
    {
        return _localCache.GetStatistics();
    }
}
