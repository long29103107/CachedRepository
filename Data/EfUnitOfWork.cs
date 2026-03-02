using System.Data;
using CachedRepository.Cache;
using CachedRepository.Repositories;

namespace CachedRepository.Data;

public class EfUnitOfWork(
    AppDbContext context,
    IProductRepository products,
    ICategoryRepository categories,
    ICacheService cacheService) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private readonly ICacheService _cacheService = cacheService;
    private readonly List<Action> _afterCommitActions = new();
    
    public IProductRepository Products { get; } = products;
    public ICategoryRepository Categories { get; } = categories;

    public IDbConnection Connection => _context.Connection;
    public IDbTransaction? Transaction => _context.CurrentTransaction;

    public async Task BeginAsync()
    {
        await _context.Database.BeginTransactionAsync();
        _cacheService.BeginTransaction();
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        await _context.Database.CommitTransactionAsync();

        foreach (var action in _afterCommitActions)
            action();

        _afterCommitActions.Clear();
        _cacheService.Commit();
    }

    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
        _afterCommitActions.Clear();
        _cacheService.Rollback();
    }

    public void AddAfterCommit(Action action)
    {
        _afterCommitActions.Add(action);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}