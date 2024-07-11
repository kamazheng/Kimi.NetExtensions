using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

/// <summary>
/// Auto check the authentication for database operation which impelement IWriteAccessEntity or IReadAccessEntity
/// </summary>
public class AuthorizeDbContext : BaseDbContext
{
    public AuthorizeDbContext(IUser user) : base(user)
    {
        LicenceHelper.CheckLicense();
    }

    //public AuthorizeDbContext(DbContextOptions<BaseDbContext> options, IUserAccessor userAccessor)
    //: base(options, userAccessor)
    //{
    //    DbUser = userAccessor.User;
    //}

    //public AuthorizeDbContext(IUser basicUser, DbContextOptions options) : base(basicUser, options)
    //{
    //    LicenceHelper.CheckLicense();
    //}

    //https://learn.microsoft.com/en-us/ef/core/modeling/dynamic-model
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, UserNameModelCacheKeyFactory>();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyReadAccessCheck<IReadAccessEntity>(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IWriteAccessEntity>().ToList())
        {
            var tableType = entry.Entity.GetType();
            if (tableType.Namespace == ProxyNameSpace)
            {
                tableType = tableType.BaseType;
            }
            var tableName = tableType.FullName;
            if (entry.State != EntityState.Unchanged && !DbUser.CanWriteTable(tableName!))
            {
                var errorMsg = $"{DbUser?.UserName} {L.NotAuthorized} WRITE TABLE {tableName}";
                throw new Exception(errorMsg);
            }
        }
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges()
    {
        return AsyncUtil.RunSync(() => SaveChangesAsync());
    }

    /// <summary>
    /// Only can read return, no warning.
    /// </summary>
    /// <typeparam name="TInterface">
    /// </typeparam>
    /// <param name="modelBuilder">
    /// </param>
    /// <returns>
    /// </returns>
    public ModelBuilder ApplyReadAccessCheck<TInterface>(ModelBuilder modelBuilder)
    {
        // get a list of entities without a baseType that implement the interface TInterface
        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(e => e.BaseType is null && e.ClrType.GetInterface(typeof(TInterface).Name) is not null)
            .Select(e => e.ClrType);

        foreach (var entity in entities)
        {
            var m = Expression.Parameter(entity);
            bool hasReadAccess = DbUser.CanReadTable(entity.FullName ?? entity.Name);
            var aExp = Expression.Constant(hasReadAccess);
            var filter = Expression.Lambda(aExp, m);

            var parameterType = Expression.Parameter(modelBuilder.Entity(entity).Metadata.ClrType);
            var filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), parameterType, filter.Body);

            // get the existing query filter
            if (modelBuilder.Entity(entity).Metadata.GetQueryFilter() is { } existingFilter)
            {
                var existingFilterBody = ReplacingExpressionVisitor.Replace(existingFilter.Parameters.Single(), parameterType, existingFilter.Body);

                // combine the existing query filter with the new query filter
                filterBody = Expression.AndAlso(existingFilterBody, filterBody);
            }

            // apply the new query filter
            modelBuilder.Entity(entity).HasQueryFilter(Expression.Lambda(filterBody, parameterType));
        }

        return modelBuilder;
    }
}