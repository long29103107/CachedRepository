using CachedRepository.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CachedRepository.Repositories.Cache;

public class CachedCategoryRepository(
    ICategoryRepository inner,
    IMemoryCache cache,
    ILogger<CachedCategoryRepository> logger,
    int durationMinutes = 5) : CachedBaseRepository<Category>(inner, cache, durationMinutes), ICategoryRepository
{
    private readonly ILogger<CachedCategoryRepository> _logger = logger;
    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting category with id {id}", id);
        var result = await base.GetByIdAsync(id, cancellationToken);
        return result;
    }
}
