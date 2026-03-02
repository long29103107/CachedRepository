using CachedRepository.Cache;
using CachedRepository.Entities;
using CachedRepository.Repositories.Base;

namespace CachedRepository.Repositories.Cache;

/// <summary>
/// Cache Aside decorator for IBaseRepository. Implements ICachedBaseRepository.
/// Defers cache writes when ICacheService.IsInTransaction until Commit.
/// </summary>
public class CachedBaseRepository<T>(
    IBaseRepository<T> inner,
    ICacheService cacheService,
    int durationMinutes = 5)
    : ICachedBaseRepository<T>
    where T : BaseEntity
{
    private readonly IBaseRepository<T> _inner = inner;
    private readonly ICacheService _cache = cacheService;
    private readonly TimeSpan _duration = TimeSpan.FromMinutes(durationMinutes);
    private readonly string _keyPrefix = typeof(T).Name;

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(id);
        if (_cache.TryGet<T>(key, out var cached))
            return cached;

        var entity = await _inner.GetByIdAsync(id, cancellationToken);
        if (entity is not null)
            _cache.Set(key, entity, _duration);

        return entity;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _inner.GetAllAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddAsync(entity, cancellationToken);
        _cache.Set(CacheKey(result.Id), result, _duration);
        return result;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        _cache.Set(CacheKey(entity.Id), entity, _duration);
    }

    public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        _cache.Remove(CacheKey(entity.Id));
    }

    private string CacheKey(int id) => $"{_keyPrefix}:{id}";
}
