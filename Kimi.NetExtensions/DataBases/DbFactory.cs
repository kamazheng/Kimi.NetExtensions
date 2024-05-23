using Kimi.NetExtensions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Kimi.NetExtensions.DataBases;

/// <summary>
/// 根据每个用户创建一个单例DbContext
/// </summary>
/// <typeparam name="T"></typeparam>
public class DbFactory<T> where T : BaseDbContext
{
    private static readonly Dictionary<string, T> UserDbContexts = new Dictionary<string, T>();
    private static readonly object LockObject = new object();

    public static T GetDbContext(IUser user)
    {
        lock (LockObject)
        {
            if (UserDbContexts.ContainsKey(user.UserName))
            {
                //check if disposed,how?
                var db = UserDbContexts[user.UserName];
                if (IsDisposed(db))
                {
                    UserDbContexts.Remove(user.UserName);
                    return createDbContext(user);
                }
                else
                {
                    return UserDbContexts[user.UserName];
                }
            }
            return createDbContext(user);
        }
    }

    private static T createDbContext(IUser user)
    {
        var dbContext = (T)Activator.CreateInstance(typeof(T), user)!;
        UserDbContexts[user.UserName] = dbContext;
        return dbContext;
    }

    public static bool IsDisposed(DbContext dbContext)
    {
        var typeDbContext = typeof(DbContext);
        var isDisposedTypeField = typeDbContext.GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);

        if (isDisposedTypeField != null)
        {
            return (bool)isDisposedTypeField.GetValue(dbContext);
        }

        // If the field doesn't exist, assume it's not disposed
        return false;
    }
}