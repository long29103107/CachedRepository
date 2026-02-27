using CachedRepository.Entities;
using CachedRepository.Models;

namespace CachedRepository.Services;

public interface ICategoryService
{
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Category?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
