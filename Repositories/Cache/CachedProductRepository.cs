using CachedRepository.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CachedRepository.Repositories.Cache;

public class CachedProductRepository(
    IProductRepository inner,
    IMemoryCache cache,
    ILogger<CachedProductRepository> logger,
    int durationMinutes = 5) : CachedBaseRepository<Product>(inner, cache, durationMinutes), IProductRepository;
