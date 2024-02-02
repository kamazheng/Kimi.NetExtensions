using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// This is to be sure the AuthorizeDbContext is returned with the current user name without cache
/// for mutiple user without this impelementation, the dbcontext will be cached for performance with
/// same user When design time, return without username to make sure migration successfully.
/// 该函数是一个类，用于创建用于缓存模型的键。它根据上下文的不同类型和设计时间选项返回不同的键值。 如果上下文是特定类型的（AuthorizeDbContext），则返回键值为上下类型和登录用户名的组合。在设计时间，仅返回上下类型的键值。
/// </summary>
public class UserNameModelCacheKeyFactory : IModelCacheKeyFactory
{
    public UserNameModelCacheKeyFactory(IServiceProvider serviceProvider)
    {
        LicenceHelper.CheckLicense();
    }

    public object Create(DbContext context)
    {
        if (context is AuthorizeDbContext)
        {
            return (context.GetType(), (context as AuthorizeDbContext)?.DbUser?.UserName ?? "");
        }
        return context.GetType();
    }

    public object Create(DbContext context, bool designTime)
    {
        if (designTime)
        {
            return context.GetType();
        }
        else
        {
            return Create(context);
        }
    }
}