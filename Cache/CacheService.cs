using Microsoft.Extensions.Caching.Memory;

namespace CachedRepository.Cache;

public class CacheService(IMemoryCache memoryCache) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private bool _isInTransaction;
    private readonly List<Action> _pendingOperations = [];

    public bool IsInTransaction => _isInTransaction;

    public bool TryGet<T>(string key, out T? value)
    {
        return _memoryCache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = expiration.HasValue
            ? new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration.Value)
            : new MemoryCacheEntryOptions();

        if (_isInTransaction)
        {
            _pendingOperations.Add(() => _memoryCache.Set(key, value, options));
        }
        else
        {
            _memoryCache.Set(key, value, options);
        }
    }

    public void Remove(string key)
    {
        if (_isInTransaction)
        {
            _pendingOperations.Add(() => _memoryCache.Remove(key));
        }
        else
        {
            _memoryCache.Remove(key);
        }
    }

    public void BeginTransaction()
    {
        _isInTransaction = true;
    }

    public void Commit()
    {
        foreach (var operation in _pendingOperations)
            operation();

        _pendingOperations.Clear();
        _isInTransaction = false;
    }

    public void Rollback()
    {
        _pendingOperations.Clear();
        _isInTransaction = false;
    }
}
