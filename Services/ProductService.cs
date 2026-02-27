using CachedRepository.Entities;
using CachedRepository.Models;
using CachedRepository.Repositories;

namespace CachedRepository.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        var nextId = products.Count > 0 ? products.Max(p => p.Id) + 1 : 1;
        var product = new Product
        {
            Id = nextId,
            Name = request.Name,
            Price = request.Price,
            Description = request.Description
        };
        return await _repository.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null) return null;

        product.Name = request.Name;
        product.Price = request.Price;
        product.Description = request.Description;
        await _repository.UpdateAsync(product, cancellationToken);
        return product;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null) return false;

        await _repository.RemoveAsync(product, cancellationToken);
        return true;
    }
}
