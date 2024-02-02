using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.Linq.Dynamic.Core;
using System.Text;

/// <summary>
/// DbContext extension
/// </summary>
public static class DbContextExtension
{
    static DbContextExtension()
    {
        LicenceHelper.CheckLicense();
    }

    public static IQueryable<object>? Set(this DbContext _context, Type t)
    {
        var method = _context.GetType().GetMethods().First(m => m.Name == "Set" && m.IsGenericMethod).MakeGenericMethod(t);
        var result = method.Invoke(_context, null);
        return (IQueryable<object>?)result;
    }

    public static IQueryable<object>? Set(this DbContext _context, string entityName)
    {
        try
        {
            var st = _context?.Model?.FindEntityType(entityName)?.ClrType;
            if (st is null) return null;
            IQueryable<object>? ObjectContext = _context?.Set(st!);
            return ObjectContext;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static bool HasDbSet(this DbContext context, Type entityType)
    {
        return context.GetType().GetProperties()
            .Any(p => p.PropertyType.IsGenericType &&
                      p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                      p.PropertyType.GetGenericArguments()[0] == entityType);
    }

    public static object? GetSingleKeyValue(this DbContext _context, object entity)
    {
        var keyName = _context.GetKeyNames(entity)?.Single();
        if (string.IsNullOrEmpty(keyName)) return null;
        var result = entity?.GetType()?.GetProperty(keyName)?.GetValue(entity, null);
        return result;
    }

    public static string? GetKeyName(this DbContext _context, object entity)
    {
        var keyName = _context.GetKeyNames(entity)?.Single();
        return keyName;
    }

    public static IEnumerable<string>? GetKeyNames(this DbContext _context, object entity)
    {
        var keyNames = _context.Model?.FindEntityType(entity.GetType())?.FindPrimaryKey()?.Properties
            .Select(x => x.Name);
        return keyNames;
    }

    public static IEnumerable<string>? GetKeyNames(this DbContext _context, Type entityType)
    {
        var keyNames = _context.Model?.FindEntityType(entityType)?.FindPrimaryKey()?.Properties
            .Select(x => x.Name);
        return keyNames;
    }

    public static string GetTableFullName(this DbContext dbContext, Type entityType)
    {
        var ientityType = dbContext.Model.FindEntityType(entityType);
        if (ientityType == null) return string.Empty;
        var schema = ientityType.GetSchema();
        var tableName = ientityType.GetTableName();
        return $"[{schema}].[{tableName}]";
    }

    //https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core
    public static async Task<List<T>> RawSqlQueryAsync<T>(this DbContext context, string query, Func<DbDataReader, T> map)
    {
        using (context)
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (await result.ReadAsync())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }
    }

    public static async Task<DataTable> RawSqlQueryAsync(this DbContext context, string query)
    {
        using (var command = context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            await context.Database.OpenConnectionAsync();

            DataTable myTable = new DataTable();
            myTable.Load(await command.ExecuteReaderAsync());
            return myTable;
        }
    }

    public static async Task<int> SaveChangesAndCommitAsync(this DbContext _dbContext, object entity, string username, bool active = true, bool autoGuid = true)
    {
        await _dbContext.SaveChangesWithoutCommitAsync(entity);
        _dbContext.MolexAuditTrailUpdate(username, active);
        return await _dbContext.SaveChangesAsync();
    }

    public static void MolexAuditTrailUpdate(this DbContext _dbContext, string username, bool active)
    {
        List<object> addedOrModifiedEntries = _dbContext.ChangeTracker
            .Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .Select(x => x.Entity)
            .ToList();
        if (addedOrModifiedEntries?.Any() != true)
        {
            return;
        }
        var proertyNameList = addedOrModifiedEntries?.FirstOrDefault()?.GetType()?.GetProperties()?.Select(x => x.Name);
        if (proertyNameList == null) return;
        foreach (object entry in addedOrModifiedEntries!)
        {
            foreach (var pName in proertyNameList)
            {
                if (string.Equals(pName, "Updated", StringComparison.CurrentCultureIgnoreCase))
                {
                    entry.SetPropertyValue(pName, DateTime.UtcNow);
                }
                if (string.Equals(pName, "Updatedby", StringComparison.CurrentCultureIgnoreCase))
                {
                    entry.SetPropertyValue(pName, username);
                }
                if (string.Equals(pName, "Active", StringComparison.CurrentCultureIgnoreCase))
                {
                    entry.SetPropertyValue(pName, active);
                }
            }
        }
    }

    public static void SaveChangesWithoutCommit(this DbContext _dbContext, object entity)
    {
        AsyncUtil.RunSync(() => _dbContext.SaveChangesWithoutCommitAsync(entity));
    }

    public async static Task SaveChangesWithoutCommitAsync(this DbContext _dbContext, object entity)
    {
        object? pkValue = _dbContext.GetSingleKeyValue(entity) ?? default;
        object? exist = _dbContext.Find(entity.GetType(), pkValue);
        if (pkValue.IsPrimaryKeyDefault() || exist == null)
        {
            var pkName = _dbContext.GetKeyName(entity);
            var pkProperty = entity.GetType().GetProperty(pkName!);
            if (pkProperty!.PropertyType == typeof(Guid) && pkValue.IsPrimaryKeyDefault())
            {
                entity.SetPropertyValue(pkName!, GuidExtensions.GenerateComb());
            }
            await _dbContext.AddAsync(entity);
        }
        else
        {
            _dbContext.Entry(exist).State = EntityState.Detached; //Important !!!
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.Update(entity);
        }
    }

    public static IQueryable<T> ActiveItems<T>(this DbSet<T> dbset) where T : class
    {
        var db = dbset.GetService<ICurrentDbContext>().Context;
        var fulltblName = db.GetTableFullName(typeof(T));
        return dbset.FromSqlRaw($"SELECT * FROM {fulltblName} WHERE ACTIVE=1");
    }

    public static int CommitWithAuditTrailUpdate(this DbContext dbContext, string username, bool active = true)
    {
        dbContext.MolexAuditTrailUpdate(username, active);
        return dbContext.SaveChanges();
    }

    public async static Task CommitWithAuditTrailUpdateAsync(this DbContext dbContext, string username, bool active = true)
    {
        dbContext.MolexAuditTrailUpdate(username, active);
        await dbContext.SaveChangesAsync();
    }

    public static DbContext? GetDbContextFromEntityType(Type entityType)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(DbContext)))
                {
                    var context = Activator.CreateInstance(type) as DbContext;
                    if (context?.Set(entityType)?.Count() >= 0)
                    {
                        return context;
                    }
                }
            }
        }
        return null;
    }

    public static DbContext? GetDbContextFromTableClassType(this Type tableClassType, IUser? user = null)
    {
        var dbContextTypes = TypeExtensions.NotSystemAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(DbContext)))
            .ToList();
        var userDbContextTypes = dbContextTypes.Where(t => t.GetConstructors().Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(IUser)))).ToList();
        var notUserDbContextTypes = dbContextTypes.Where(t => t.GetConstructors().All(c => c.GetParameters().All(p => p.ParameterType != typeof(IUser)))).ToList();
        try
        {
            var dbContextType = userDbContextTypes.FirstOrDefault(t => Activator.CreateInstance(t, user) is DbContext dbContext && dbContext.Set(tableClassType.FullName!) != null);
            var returnIUserDb = dbContextType != null ? Activator.CreateInstance(dbContextType, user) as DbContext : null;
            dbContextType = notUserDbContextTypes.FirstOrDefault(t => Activator.CreateInstance(t) is DbContext dbContext && dbContext.Set(tableClassType.FullName!) != null);
            var returnNotIUserDb = dbContextType != null ? Activator.CreateInstance(dbContextType) as DbContext : null;
            return returnIUserDb ?? returnNotIUserDb;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public static IEnumerable<Type> GetTableClassesFromDbContext(this DbContext dbContext)
    {
        return dbContext.GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0]);
    }

    /// <summary>
    /// Get Table classes by DbContext DbSet property
    /// </summary>
    /// <returns>
    /// </returns>
    public static IEnumerable<Type> GetAllTableClasses()
    {
        return TypeExtensions.NotSystemAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(DbContext)))
            .SelectMany(type => type.GetProperties()
                .Where(p => p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]));
    }

    public static DbContext? GetDbContextFromEntityName(string entityName)
    {
        var type = entityName.CreateClassInstance();
        if (type == null) throw new Exception("No table found!");
        return GetDbContextFromEntityType(type.GetType());
    }

    public static async Task<IEnumerable<object>> GetDbRecordsByRawSql(string tableFullName,
    string whereClause = "", int topQty = 1000, string orderBy = "", bool isDescending = false, bool activeOnly = true, IUser? user = null)
    {
        string[] sensitiveWords = new string[] { "DROP ", "DELETE ", "TRUNCATE " };
        bool result = ContainsSensitiveWords(whereClause, sensitiveWords);
        if (result) { throw new Exception("Where Clause cannot have senstive words"); }
        var tableClassType = tableFullName.GetClassType();
        if (tableClassType == null) throw new Exception("Table Not Found");
        var dbContext = tableClassType.GetDbContextFromTableClassType(user);
        if (dbContext == null) throw new Exception("DbContext Not Found");
        var entityType = dbContext.Model.FindEntityType(tableClassType.FullName!);
        if (entityType == null) throw new Exception("Entity Type Not Found");
        var schemaName = entityType.GetSchema();
        var tableName = entityType.GetTableName();
        var queryBuilder = new StringBuilder($"SELECT TOP {topQty} * FROM [{schemaName}].[{tableName}]");
        if (activeOnly && entityType.GetProperties().Any(p => string.Equals(p.Name, "Active", StringComparison.CurrentCultureIgnoreCase)))
        {
            queryBuilder.Append($" WHERE Active=1");
            if (!string.IsNullOrEmpty(whereClause)) queryBuilder.Append($" AND ({whereClause})");
        }
        else
        {
            if (!string.IsNullOrEmpty(whereClause)) queryBuilder.Append($" WHERE {whereClause}");
        }
        if (!string.IsNullOrEmpty(orderBy)) queryBuilder.Append($" ORDER BY {orderBy}");
        if (isDescending && !string.IsNullOrEmpty(orderBy)) queryBuilder.Append(" DESC");
        var query = queryBuilder.ToString();

        //replace the class property name with table field name.
        var entityProperties = entityType.GetProperties();
        var args = new Dictionary<string, string>();
        foreach (var property in entityProperties)
        {
            args.Add(property.Name, property.GetColumnName());
        }
        query = query.ReplaceWordsInSquareBracket(args);

        using var tableContext = await dbContext.RawSqlQueryAsync(query);
        return tableContext.MapTableToList(tableClassType);
    }

    private static bool ContainsSensitiveWords(string sqlString, string[] sensitiveWords)
    {
        foreach (string word in sensitiveWords)
        {
            if (sqlString.Contains(word, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public static async Task<object?> UpsertAsync(string tableFullName, string recordJson, string username, bool _active = true, IUser? user = null)
    {
        var tableClassType = tableFullName.GetClassType();
        if (tableClassType == null) throw new Exception(L.TableNotFound);
        using var dbContext = tableClassType.GetDbContextFromTableClassType(user);
        if (dbContext == null) throw new Exception(L.DbContextNotFound);
        var entity = JsonConvert.DeserializeObject(recordJson, tableClassType);
        if (entity == null) throw new Exception("Record is null"); ;
        await dbContext.SaveChangesAndCommitAsync(entity, username, active: _active);
        return entity;
    }

    public static async Task<object?> UpsertAsync(string recordJson, string typeFullName, IUser? user = null)
    {
        object? input;
        if (string.IsNullOrEmpty(typeFullName))
        {
            input = JsonConvert.DeserializeObject(recordJson);
        }
        else
        {
            var type = typeFullName.GetClassType() ?? throw new Exception(L.IsNotCorrectNoDatabaseEntityFound);
            input = JsonConvert.DeserializeObject(recordJson, type);
        }
        var _dbContext = input?.GetType().GetDbContextFromTableClassType(user) ?? throw new Exception(L.JsonContainsWrongType);
        await _dbContext.UpsertWithoutSaveAsync(input);
        await _dbContext.SaveChangesAsync();
        return input;
    }

    public static async Task UpsertWithoutSaveAsync(this DbContext _dbContext, string recordJson)
    {
        var input = JsonConvert.DeserializeObject(recordJson) ?? throw new Exception(L.JsonContainsWrongType);
        await _dbContext.UpsertWithoutSaveAsync(input);
    }

    public static async Task UpsertWithoutSaveAsync(this DbContext _dbContext, object input)
    {
        object? pkValue = _dbContext.GetSingleKeyValue(input) ?? default;
        object? exist = _dbContext.Find(input.GetType(), pkValue);
        if (pkValue.IsPrimaryKeyDefault() || exist == null)
        {
            var pkName = _dbContext.GetKeyName(input);
            var pkProperty = input.GetType().GetProperty(pkName!);
            if (pkProperty!.PropertyType == typeof(Guid) && pkValue.IsPrimaryKeyDefault())
            {
                input.SetPropertyValue(pkName!, GuidExtensions.GenerateComb());
            }
            await _dbContext.AddAsync(input);
        }
        else
        {
            _dbContext.Entry(exist).CurrentValues.SetValues(input); //Important !!!
            _dbContext.Entry(exist).State = EntityState.Modified;
        }
    }

    public static async Task<object?> DeleteAsync(string recordJson, IUser? user = null)
    {
        var input = JsonConvert.DeserializeObject(recordJson);
        var _dbContext = input?.GetType().GetDbContextFromTableClassType(user) ?? throw new Exception(L.JsonContainsWrongType);

        object? pkValue = _dbContext.GetSingleKeyValue(input) ?? default;
        object? exist = _dbContext.Find(input.GetType(), pkValue);
        if (pkValue.IsPrimaryKeyDefault() || exist == null)
        {
            throw new Exception(L.RecordNotFound);
        }
        else
        {
            _dbContext.Entry(exist).State = EntityState.Deleted;
        }
        await _dbContext.SaveChangesAsync();
        return input;
    }

    /// <summary>
    /// </summary>
    /// <param name="dto">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    /// <exception cref="Exception">
    /// </exception>
    public static IQueryable GetDbRecordsByDynamicLinq(TableQuery dto, IUser? user = null)
    {
        var tableClassType = dto.TableClassFullName!.GetClassType()
            ?? throw new Exception($"{dto.TableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        var db = tableClassType.GetDbContextFromTableClassType(user)
            ?? throw new Exception($"{dto.TableClassFullName} {L.IsNotCorrectNoDatabaseFound}!");
        var dbSet = db.Set(dto.TableClassFullName!)!.AsQueryable()
            ?? throw new Exception($"{dto.TableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        IQueryable<object> result = dbSet;
        if (!string.IsNullOrEmpty(dto.WhereClause))
        {
            result = result.Where(dto.WhereClause);
        }
        else
        {
            result = dbSet.Where("1==1");
        }
        if (!string.IsNullOrEmpty(dto.OrderBy))
        {
            result = result.OrderBy(dto.OrderBy);
        }
        else
        {
            if (typeof(ISoftDeleteEntity).IsAssignableFrom(tableClassType))
            {
                result = result.OrderBy(nameof(ISoftDeleteEntity.Updated) + " desc");
            }
        }
        if (dto.IgnoreAutoInclude && dto.Fields?.Any() != true)
        {
            result = result.IgnoreAutoIncludes();
        }
        if (dto.Includes?.Any() == true)
        {
            foreach (var include in dto.Includes)
            {
                result = result.Include(include);
            }
        }
        if (dto.Fields?.Any() == true)
        {
            string fieldsString = string.Join(",", dto.Fields.Distinct().Select(s => s + " AS " + s.Replace(".", ""))); //navigation has "." will cause error
            var dynamicResult = (result as IQueryable).Select($"new ({fieldsString})");
            return dynamicResult.Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize);
        }
        else
        {
            var finResult = result.Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize);
            return finResult;
        }
    }

    public static object GetItem(RecordQuery query, IUser? user = null)
    {
        var tableClassType = query.TableClassFullName!.GetClassType()
            ?? throw new Exception($"{query.TableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        var db = tableClassType.GetDbContextFromTableClassType(user)
            ?? throw new Exception($"{query.TableClassFullName} {L.IsNotCorrectNoDatabaseFound}!");
        var dbSet = db.Set(query.TableClassFullName!)
            ?? throw new Exception($"{query.TableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        // Get the primary key property's type
        var pkProperty = db.Model.FindEntityType(tableClassType)?.FindPrimaryKey()?.Properties[0]
            ?? throw new Exception($"{query.TableClassFullName} {L.NoPrimaryKeyFound}!");
        var pkType = pkProperty.ClrType;
        // Convert the primary key value to the correct type
        object convertedPk;
        if (pkType == typeof(Guid))
        {
            convertedPk = Guid.Parse(query.Id.ToString()!);
        }
        else
        {
            convertedPk = Convert.ChangeType(query.Id, pkType);
        }
        var result = dbSet.Where("1==1");
        var dynamicLambda = $"{pkProperty.Name}==\"{convertedPk}\"";
        if (query.IgnoreAutoInclude)
        {
            result = result.Where(dynamicLambda).IgnoreAutoIncludes();
        }
        else
        {
            result = result.Where(dynamicLambda);
        }
        foreach (var include in query.Includes)
        {
            result = result.Include(include);
        }
        return result.FirstOrDefault()
            ?? throw new Exception($"{query.TableClassFullName} {L.RecordNotFound}!");
    }

    public static async Task<object> DeleteAsync(string tableClassFullName, object pk, IUser? user = null)
    {
        var tableClassType = tableClassFullName!.GetClassType()
            ?? throw new Exception($"{tableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        var db = tableClassType.GetDbContextFromTableClassType(user)
            ?? throw new Exception($"{tableClassFullName} {L.IsNotCorrectNoDatabaseFound}!");
        // Get the primary key property's type
        var pkProperty = db.Model.FindEntityType(tableClassType)?.FindPrimaryKey()?.Properties[0]
            ?? throw new Exception($"{tableClassFullName} {L.NoPrimaryKeyFound}!");
        var pkType = pkProperty.ClrType;
        // Convert the primary key value to the correct type
        var convertedPk = Convert.ChangeType(pk, pkType);
        var result = db.Find(tableClassType, convertedPk) ?? throw new Exception($"{tableClassFullName} Id: {pk} {L.RecordNotFound}!");
        db.Remove(result);
        await db.SaveChangesAsync();
        return result;
    }

    public static async Task<object> DeleteWhereAsync(string tableClassFullName, string where, IUser? user = null)
    {
        var tableClassType = tableClassFullName!.GetClassType()
            ?? throw new Exception($"{tableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        var db = tableClassType.GetDbContextFromTableClassType(user)
            ?? throw new Exception($"{tableClassFullName} {L.IsNotCorrectNoDatabaseFound}!");
        var dbSet = db.Set(tableClassFullName)
            ?? throw new Exception($"{tableClassFullName} {L.IsNotCorrectNoDatabaseEntityFound}!");
        var result = dbSet.Where(where).ToList();
        db.RemoveRange(result);
        await db.SaveChangesAsync();
        return result;
    }
}