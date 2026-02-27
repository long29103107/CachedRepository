using CachedRepository.Entities;
using CachedRepository.Models;
using CachedRepository.Repositories;

namespace CachedRepository.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public async Task<Category> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);
        var nextId = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1;
        var category = new Category
        {
            Id = nextId,
            Name = request.Name,
            Description = request.Description
        };
        return await _repository.AddAsync(category, cancellationToken);
    }

    public async Task<Category?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null) return null;

        category.Name = request.Name;
        category.Description = request.Description;
        await _repository.UpdateAsync(category, cancellationToken);
        return category;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null) return false;

        await _repository.RemoveAsync(category, cancellationToken);
        return true;
    }
}
