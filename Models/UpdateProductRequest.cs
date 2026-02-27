namespace CachedRepository.Models;

public record UpdateProductRequest(string Name, decimal Price, string? Description = null);
