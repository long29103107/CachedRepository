using CachedRepository.Data;
using CachedRepository.Entities;

namespace CachedRepository.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext db) : base(db)
    {
    }
}
