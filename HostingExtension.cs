using CachedRepository.Data;
using CachedRepository.Entities;
using Scalar.AspNetCore;

namespace CachedRepository;

public static class HostingExtension
{
    public static async Task ConfigureApplicationAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        await SeedDatabaseAsync(app);
    }

    private static async Task SeedDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Id = 1, Name = "Widget", Price = 9.99m, Description = "A small widget" },
                new Product { Id = 2, Name = "Gadget", Price = 19.99m, Description = "A useful gadget" });
            await db.SaveChangesAsync();
        }
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and accessories" });
            await db.SaveChangesAsync();
        }
    }
}