namespace CachedRepository.Models;

public record CreateProductRequest(string Name, decimal Price, string? Description = null);