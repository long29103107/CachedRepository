using CachedRepository.Data;
using CachedRepository.Entities;
using CachedRepository.Repositories.Base;

namespace CachedRepository.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext db) : base(db)
    {
    }
}
