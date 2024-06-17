using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Model.Auditing;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// BaseDbContext, implement below functionalities:
/// 1. SoftDelete
/// 2. Auditing
/// </summary>
public class BaseDbContext : DbContext
{
    public IUser DbUser { get; set; }

    public DbSet<Trail> AuditTrails => Set<Trail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // QueryFilters need to be applied before base.OnModelCreating
        modelBuilder.AppendGlobalQueryFilter<ISoftDeleteEntity>(s => s.Active == true);
        base.OnModelCreating(modelBuilder);
        LicenceHelper.CheckLicense();
    }

    //public BaseDbContext()
    //{
    //}

    //public BaseDbContext(IUser user, DbContextOptions options) : base(options)
    //{
    //    DbUser = user;
    //}

    public BaseDbContext(IUser user)
    {
        DbUser = user;
    }

    //public BaseDbContext(DbContextOptions<BaseDbContext> options, IUserAccessor userAccessor)
    //: base(options)
    //{
    //    DbUser = userAccessor.User;
    //}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        ChangeTracker.DetectChanges();
        softDelete();
        var auditEntries = HandleAuditingBeforeSaveChanges(DbUser.UserName);

        //ChangeTracker.DetectChanges();
        int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await HandleAuditingAfterSaveChangesAsync(auditEntries, cancellationToken);

        return result;
    }

    private List<AuditTrail> HandleAuditingBeforeSaveChanges(string userId)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToList())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedOn = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedBy).IsModified = false;
                entry.Property(x => x.CreatedOn).IsModified = false;
            }
        }

        ChangeTracker.DetectChanges();

        var trailEntries = new List<AuditTrail>();
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .ToList())
        {
            var trailEntry = new AuditTrail(entry)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId
            };
            trailEntries.Add(trailEntry);
            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    trailEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trailEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        trailEntry.TrailType = TrailType.Create;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        trailEntry.TrailType = TrailType.Delete;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (propertyName == nameof(IAuditableEntity.CreatedOn))
                        {
                            property.CurrentValue = property.OriginalValue;
                        }
                        if (property.IsModified && entry.Entity is ISoftDeleteEntity
                            && propertyName == nameof(ISoftDeleteEntity.Active) && Convert.ToBoolean(property.CurrentValue) == false)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Delete;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        else if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            if (trailEntry.TrailType == default) trailEntry.TrailType = TrailType.Update;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }

                        break;
                }
            }
        }

        foreach (var auditEntry in trailEntries.Where(e => !e.HasTemporaryProperties))
        {
            AuditTrails.Add(auditEntry.ToAuditTrail());
        }

        return trailEntries.Where(e => e.HasTemporaryProperties).ToList();
    }

    private Task HandleAuditingAfterSaveChangesAsync(List<AuditTrail> trailEntries, CancellationToken cancellationToken = new())
    {
        if (trailEntries == null || trailEntries.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var entry in trailEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditTrails.Add(entry.ToAuditTrail());
        }
        //Important to call base method, otherwise, will trigger again.
        return base.SaveChangesAsync(cancellationToken);
    }

    private void softDelete()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeleteEntity>().ToList())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.Entity.Active = false;
                entry.State = EntityState.Modified;
                entry.Entity.Updated = DateTime.UtcNow;
                entry.Entity.Updatedby = DbUser.UserName;
            }
            else if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                entry.Entity.Active = true;
                entry.Entity.Updated = DateTime.UtcNow;
                entry.Entity.Updatedby = DbUser.UserName;
            }
        }
    }

    public override int SaveChanges()
    {
        return AsyncUtil.RunSync(() => SaveChangesAsync());
    }
}