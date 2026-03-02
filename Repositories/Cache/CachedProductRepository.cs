using CachedRepository.Cache;
using CachedRepository.Entities;

namespace CachedRepository.Repositories.Cache;

public class CachedProductRepository(
    IProductRepository inner,
    ICacheService cacheService,
    ILogger<CachedProductRepository> logger,
    int durationMinutes = 5) : CachedBaseRepository<Product>(inner, cacheService, durationMinutes), IProductRepository;
