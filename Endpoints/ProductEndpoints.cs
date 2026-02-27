using CachedRepository.Models;
using CachedRepository.Services;

namespace CachedRepository.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (IProductService productService, CancellationToken ct) =>
            Results.Ok(await productService.GetAllAsync(ct)))
            .WithName("GetProducts")
            .WithSummary("Get all products");

        group.MapGet("/{id:int}", async (int id, IProductService productService, CancellationToken ct) =>
        {
            var product = await productService.GetByIdAsync(id, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
            .WithName("GetProductById")
            .WithSummary("Get a product by ID");

        group.MapPost("/", async (CreateProductRequest request, IProductService productService, CancellationToken ct) =>
        {
            var product = await productService.CreateAsync(request, ct);
            return Results.Created($"/products/{product.Id}", product);
        })
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:int}", async (int id, UpdateProductRequest request, IProductService productService, CancellationToken ct) =>
        {
            var product = await productService.UpdateAsync(id, request, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product");

        group.MapDelete("/{id:int}", async (int id, IProductService productService, CancellationToken ct) =>
        {
            var deleted = await productService.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");

        return group;
    }
}
