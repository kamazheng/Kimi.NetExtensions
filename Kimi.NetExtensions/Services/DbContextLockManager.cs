using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Kimi.NetExtensions.Services;

public class DbContextLockManager
{
    private static readonly Lazy<DbContextLockManager> _instance =
        new Lazy<DbContextLockManager>(() => new DbContextLockManager());

    private readonly ConcurrentDictionary<DbContextId, SemaphoreSlim> _dbContextLocks =
        new ConcurrentDictionary<DbContextId, SemaphoreSlim>();

    private DbContextLockManager()
    {
    }

    public static DbContextLockManager Instance => _instance.Value;

    public SemaphoreSlim GetLockForDbContext(DbContext dbContext)
    {
        // GetOrAdd is atomic and thread-safe, no need for explicit locking
        return _dbContextLocks.GetOrAdd(dbContext.ContextId, _ => new SemaphoreSlim(1, 1));
    }

    public void RemoveLockForDbContext(DbContext dbContext)
    {
        if (_dbContextLocks.TryRemove(dbContext.ContextId, out var semaphore))
        {
            semaphore.Dispose();
        }
    }
}