using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CachedRepository.Data.AppDbContext>(options =>
    options.UseInMemoryDatabase("CachedRepositoryDb"));
builder.Services.AddScoped<CachedRepository.Repositories.IProductRepository, CachedRepository.Repositories.ProductRepository>();

var app = builder.Build();

// Seed in-memory database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CachedRepository.Data.AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new CachedRepository.Entities.Product { Id = 1, Name = "Widget", Price = 9.99m, Description = "A small widget" },
            new CachedRepository.Entities.Product { Id = 2, Name = "Gadget", Price = 19.99m, Description = "A useful gadget" });
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Product endpoints
app.MapGet("/products", async (CachedRepository.Repositories.IProductRepository repo, CancellationToken ct) =>
    await repo.GetAllAsync(ct))
.WithName("GetProducts");

app.MapGet("/products/{id:int}", async (int id, CachedRepository.Repositories.IProductRepository repo, CancellationToken ct) =>
{
    var product = await repo.GetByIdAsync(id, ct);
    return product is null ? Results.NotFound() : Results.Ok(product);
})
.WithName("GetProductById");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
