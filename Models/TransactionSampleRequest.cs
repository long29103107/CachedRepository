namespace CachedRepository.Models;

/// <summary>
/// Request to create a category and product together in a single transaction.
/// </summary>
public record TransactionSampleRequest(
    string CategoryName,
    string? CategoryDescription,
    string ProductName,
    decimal ProductPrice,
    string? ProductDescription = null);
