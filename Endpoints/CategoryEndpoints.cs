using CachedRepository.Models;
using CachedRepository.Services;

namespace CachedRepository.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService categoryService, CancellationToken ct) =>
            Results.Ok(await categoryService.GetAllAsync(ct)))
            .WithName("GetCategories")
            .WithSummary("Get all categories");

        group.MapGet("/{id:int}", async (int id, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.GetByIdAsync(id, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
            .WithName("GetCategoryById")
            .WithSummary("Get a category by ID");

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.CreateAsync(request, ct);
            return Results.Created($"/categories/{category.Id}", category);
        })
            .WithName("CreateCategory")
            .WithSummary("Create a new category");

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService categoryService, CancellationToken ct) =>
        {
            var category = await categoryService.UpdateAsync(id, request, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category");

        group.MapDelete("/{id:int}", async (int id, ICategoryService categoryService, CancellationToken ct) =>
        {
            var deleted = await categoryService.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteCategory")
            .WithSummary("Delete a category");

        return group;
    }
}
