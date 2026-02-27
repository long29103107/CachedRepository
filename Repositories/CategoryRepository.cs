using CachedRepository.Data;
using CachedRepository.Entities;
using CachedRepository.Repositories.Base;

namespace CachedRepository.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext db) : base(db)
    {
    }
}
