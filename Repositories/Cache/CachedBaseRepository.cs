using CachedRepository.Entities;
using CachedRepository.Repositories.Base;
using Microsoft.Extensions.Caching.Memory;

namespace CachedRepository.Repositories.Cache;

/// <summary>
/// Cache Aside decorator for IBaseRepository. Implements ICachedBaseRepository.
/// </summary>
public class CachedBaseRepository<T>(
    IBaseRepository<T> inner,
    IMemoryCache cache,
    int durationMinutes = 5)
    : ICachedBaseRepository<T>
    where T : BaseEntity
{
    private readonly IBaseRepository<T> _inner = inner;
    private readonly IMemoryCache _cache = cache;
    private readonly int _duration = durationMinutes;
    private readonly string _keyPrefix = typeof(T).Name;

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(id);
        if (_cache.TryGetValue(key, out T? cached))
            return cached;

        var entity = await _inner.GetByIdAsync(id, cancellationToken);
        if (entity is not null)
            _cache.Set(key, entity, TimeSpan.FromMinutes(_duration));

        return entity;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _inner.GetAllAsync(cancellationToken);
        return list;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddAsync(entity, cancellationToken);
        InvalidateAll();
        _cache.Set(CacheKey(result.Id), result, TimeSpan.FromMinutes(_duration));
        return result;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        InvalidateAll();
        _cache.Set(CacheKey(entity.Id), entity, TimeSpan.FromMinutes(_duration));
    }

    public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        InvalidateAll();
        _cache.Remove(CacheKey(entity.Id));
    }

    private string CacheKey(int id) => $"{_keyPrefix}:{id}";
    private string AllKey => $"{_keyPrefix}:all";

    private void InvalidateAll()
    {
        _cache.Remove(AllKey);
    }
}
