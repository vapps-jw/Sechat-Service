using Microsoft.EntityFrameworkCore;

namespace Sechat.Data.Repositories;

public abstract class RepositoryBase<TContext> where TContext : DbContext
{
    protected readonly TContext _context;

    protected RepositoryBase(TContext context) => _context = context;

    public Task<int> SaveChanges() => _context.SaveChangesAsync();

    public Task<int> SaveChanges(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);

    public void ClearTracker() => _context.ChangeTracker.Clear();
}

