using CachedRepository.Cache;
using CachedRepository.Entities;

namespace CachedRepository.Repositories.Cache;

public class CachedCategoryRepository(
    ICategoryRepository inner,
    ICacheService cacheService,
    ILogger<CachedCategoryRepository> logger,
    int durationMinutes = 5) : CachedBaseRepository<Category>(inner, cacheService, durationMinutes), ICategoryRepository
{
    private readonly ILogger<CachedCategoryRepository> _logger = logger;
    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting category with id {id}", id);
        var result = await base.GetByIdAsync(id, cancellationToken);
        return result;
    }
}
