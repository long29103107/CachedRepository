using CachedRepository.Entities;
using CachedRepository.Models;

namespace CachedRepository.Services;

public interface IProductService
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Product?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
