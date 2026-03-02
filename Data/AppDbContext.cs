using System.Data;
using CachedRepository.Attributes;
using Microsoft.EntityFrameworkCore;
using CachedRepository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CachedRepository.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public IDbConnection Connection =>
        Database.GetDbConnection();

    public IDbTransaction? CurrentTransaction =>
        Database.CurrentTransaction?.GetDbTransaction();
    
    [CachedEntity]
    public DbSet<Product> Products => Set<Product>();

    [CachedEntity(10)]
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200);
        });

        // Audit columns configured via BaseEntity properties
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
