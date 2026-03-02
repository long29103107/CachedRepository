using CachedRepository.Data;
using CachedRepository.Entities;
using CachedRepository.Models;

namespace CachedRepository.Endpoints;

/// <summary>
/// Sample endpoints demonstrating UnitOfWork transaction usage.
/// </summary>
public static class TransactionSampleEndpoints
{
    public static IEndpointRouteBuilder MapTransactionSampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/samples").WithTags("Transaction Samples");

        group.MapPost("/transaction", async (
                IUnitOfWork uow,
                bool rollback = false,
                CancellationToken ct = default) =>
            {
                using (uow)
                {
                    try
                    {
                        await uow.BeginAsync();

                        var category = await uow.Categories.AddAsync(new Category
                        {
                            Name = "CategoryName",
                            Description = "CategoryDescription"
                        }, ct);

                        var product = await uow.Products.AddAsync(new Product
                        {
                            Name = "ProductName",
                            Price = 11111,
                            Description = "ProductDescription"
                        }, ct);

                        if (rollback)
                        {
                            await uow.RollbackAsync();
                            return Results.Ok(new
                            {
                                message = "Transaction rolled back (demo). No data persisted.",
                                category,
                                product
                            });
                        }

                        await uow.CommitAsync();

                        return Results.Created("/samples/transaction", new
                        {
                            message = "Category and product created in single transaction.",
                            category,
                            product
                        });
                    }
                    catch (Exception ex)
                    {
                        await uow.RollbackAsync();
                        throw;
                    }
                }
            })
            .WithName("CreateCategoryAndProductInTransaction")
            .WithSummary("Create a category and product atomically in one transaction")
            .WithDescription("Uses UnitOfWork.BeginAsync/CommitAsync. Pass ?rollback=true to demo RollbackAsync.");

        return group;
    }
}
