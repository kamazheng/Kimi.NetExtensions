using Microsoft.EntityFrameworkCore;

namespace Kimi.NetExtensions.Services;

public static class DbContextService
{
    public static async Task LockToRunAsync(this DbContext dbContext, Func<DbContext, Task> action)
    {
        var dbContextSemaphore = DbContextLockManager.Instance.GetLockForDbContext(dbContext);
        await dbContextSemaphore.WaitAsync(); // Asynchronously wait to enter the semaphore
        try
        {
            await action(dbContext).ConfigureAwait(true);
        }
        finally
        {
            dbContextSemaphore.Release(); // Release the semaphore so other tasks can enter
        }
    }
}