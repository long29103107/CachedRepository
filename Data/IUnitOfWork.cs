using System.Data;
using CachedRepository.Repositories;

namespace CachedRepository.Data;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }

    Task BeginAsync();
    Task CommitAsync();
    Task RollbackAsync();

    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    void AddAfterCommit(Action action);
}