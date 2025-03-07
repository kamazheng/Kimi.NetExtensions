using Kimi.NetExtensions.Extensions;
using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System.Linq.Dynamic.Core;

public static class GenericTableTools
{
    static GenericTableTools()
    {
        
    }

    /// <summary> Get all table classes from dbcontext dbset<> </summary> <param
    /// name="isShortName"></param> <returns></returns>
    public static async Task<ObjectResult> GetTableListAsync(bool isShortName)
    {
        return await Task.Run(() => GetTableList(isShortName));
    }

    public static ObjectResult GetTableList(bool isShortName)
    {
        IEnumerable<string?> allTables = DbContextExtension.GetAllTableClasses().Select(t => isShortName ? t.Name : t.FullName);
        return new OkObjectResult(allTables);
    }

    /// <summary>
    /// </summary>
    /// <param name="tableQuery">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> GetItemsAsync(TableQuery tableQuery, IUser? user = null)
    {
        return await Task.Run(() => GetItems(tableQuery, user));
    }

    public static async Task<ObjectResult> GetItemsAsync(TableQuery tableQuery, DbContext db)
    {
        return await Task.Run(() => GetItems(tableQuery, db));
    }

    public static ObjectResult GetItems(TableQuery tableQuery, IUser? user = null)
    {
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            return new BadRequestObjectResult(L.TableFullnameCannotBeNull);
        }

        var result = DbContextExtension.GetDbRecordsByDynamicLinq(tableQuery, user);
        var json = JsonConvert.SerializeObject(result);
        if (json.Contains("AnonymousType"))
        {
            var noTypeResult = JsonConvert.SerializeObject(result.ToDynamicList<object>(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None });
            return new OkObjectResult(noTypeResult);
        }
        return new OkObjectResult(json);
    }

    public static ObjectResult GetItems(TableQuery tableQuery, DbContext db)
    {
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            return new BadRequestObjectResult(L.TableFullnameCannotBeNull);
        }

        var result = DbContextExtension.GetDbRecordsByDynamicLinq(tableQuery, db);
        var json = JsonConvert.SerializeObject(result);
        if (json.Contains("AnonymousType"))
        {
            var noTypeResult = JsonConvert.SerializeObject(result.ToDynamicList<object>(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None });
            return new OkObjectResult(noTypeResult);
        }
        return new OkObjectResult(json);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="filter">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<List<T>?> GetItemsByFilterAsync<T>(string filter, IUser? user = null)
    {
        return await Task.Run(() => GetItemsByFilter<T>(filter, user));
    }

    public static async Task<List<T>?> GetItemsByFilterAsync<T>(string filter, DbContext db)
    {
        return await Task.Run(() => GetItemsByFilter<T>(filter, db));
    }

    public static List<T>? GetItemsByFilter<T>(string filter, IUser? user = null)
    {
        var result = DbContextExtension.GetDbRecordsByDynamicLinq(new TableQuery
        {
            WhereClause = filter,
            TableClassFullName = typeof(T).FullName!
        }, user);
        return result.ToDynamicList<T>();
    }

    public static List<T>? GetItemsByFilter<T>(string filter, DbContext db)
    {
        var result = DbContextExtension.GetDbRecordsByDynamicLinq(new TableQuery
        {
            WhereClause = filter,
            TableClassFullName = typeof(T).FullName!
        }, db);
        return result.ToDynamicList<T>();
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="id">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static T? GetItem<T>(object id, IUser? user = null)
    {
        var result = DbContextExtension.GetItem(new RecordQuery
        {
            Id = id,
            TableClassFullName = typeof(T).FullName!
        }, user);
        if (result != null)
        {
            return (T)result;
        }
        else
        {
            return default;
        }
    }

    public static object? GetItem(object id, Type tableClassType, IUser? user = null)
    {
        var result = DbContextExtension.GetItem(new RecordQuery
        {
            Id = id,
            TableClassFullName = tableClassType.FullName!
        }, user);
        return result;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="where">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<T?> GetItemByFilterAsync<T>(string where, IUser? user = null)
    {
        return await Task.Run(() => GetItemByFilter<T>(where, user));
    }

    public static T? GetItemByFilter<T>(string where, IUser? user = null)
    {
        var results = GetItemsByFilter<T>(where, user);
        if (results?.Any() == true)
        {
            return results.First();
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Update or Insert table item
    /// </summary>
    /// <param name="upsertBody">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> UpsertAsync(UpsertBody upsertBody, IUser? user = null)
    {
        var result = await DbContextExtension.UpsertAsync(upsertBody.JsonRecord, upsertBody.TableClassFullName, user);
        var json = JsonConvert.SerializeObject(result);
        return new OkObjectResult(json);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="record">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<T> UpsertAsync<T>(T record, IUser? user = null)
    {
        var result = await UpsertAsync(new UpsertBody { JsonRecord = record.ToJson(), TableClassFullName = typeof(T).FullName! }, user);
        ValidObjectResult(result);
        return result!.Value!.ToString()!.ToType<T>()!;
    }

    /// <summary>
    /// Delete table record by Id
    /// </summary>
    /// <param name="query">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> DeleteAsync(RecordQuery query, IUser? user = null)
    {
        var result = await DbContextExtension.DeleteAsync(query.TableClassFullName, query.Id, user);
        var json = JsonConvert.SerializeObject(result);
        return new OkObjectResult(json);
    }

    public static async Task DeleteAsync<T>(object id, IUser? user = null)
    {
        var result = await DeleteAsync(new RecordQuery { TableClassFullName = typeof(T).FullName!, Id = id }, user);
        ValidObjectResult(result);
    }

    /// <summary>
    /// Delete table records according filter expression
    /// </summary>
    /// <param name="query">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> DeleteByFilterAsync(TableQuery query, IUser? user = null)
    {
        if (string.IsNullOrEmpty(query?.TableClassFullName)) return new BadRequestObjectResult(L.TableFullnameCannotBeNull);
        if (string.IsNullOrEmpty(query?.WhereClause)) return new BadRequestObjectResult(L.WhereClauseCannotBeNull);

        var result = await DbContextExtension.DeleteWhereAsync(query.TableClassFullName, query.WhereClause, user);
        var json = JsonConvert.SerializeObject(result);
        return new OkObjectResult(json);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="filter">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task DeleteByFilterAsync<T>(string filter, IUser? user = null)
    {
        var result = await DeleteByFilterAsync(new TableQuery { TableClassFullName = typeof(T).FullName!, WhereClause = filter }, user);
        ValidObjectResult(result);
    }

    private static void ValidObjectResult(ObjectResult result)
    {
        if (!result.IsSuccessCode())
        {
            throw new Exception(result?.Value!.ToString());
        }
    }

    /// <summary>
    /// Export table records to excel file
    /// </summary>
    /// <param name="tableQuery">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<FileContentResult> ExportExcelAsync(TableQuery tableQuery, IUser? user = null)
    {
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            throw new Exception(L.TableFullnameCannotBeNull);
        }

        var result = await Task.Run(() => DbContextExtension
            .GetDbRecordsByDynamicLinq(tableQuery, user)
            .ToDynamicList<object>());
        var fileName = $"{tableQuery?.TableClassFullName}.xlsx";
        var file = await Task.Run(() => ExcelService.GetExcelFile(result, fileName));
        return file;
    }

    public static async Task<FileContentResult> ExportExcelAsync(TableQuery tableQuery, DbContext db)
    {
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            throw new Exception(L.TableFullnameCannotBeNull);
        }

        var result = await Task.Run(() => DbContextExtension
            .GetDbRecordsByDynamicLinq(tableQuery, db)
            .ToDynamicList<object>());
        var fileName = $"{tableQuery?.TableClassFullName}.xlsx";
        var file = await Task.Run(() => ExcelService.GetExcelFile(result, fileName));
        return file;
    }

    /// <summary>
    /// Import table records from excel, empty primary key is insert, otherwise is update
    /// </summary>
    /// <param name="file">
    /// file.FileName must be the fullname of table class
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> ImportExcelAsync(IFormFile file, IUser? user = null)
    {
        var stream = file.OpenReadStream();
        return await ImportExcelAsync(file.FileName, stream, user);
    }

    /// <summary>
    /// </summary>
    /// <param name="tableFullName">
    /// </param>
    /// <param name="stream">
    /// </param>
    /// <param name="user">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task<ObjectResult> ImportExcelAsync(string tableFullName, Stream stream, IUser? user = null)
    {
        var workbook = WorkbookFactory.Create(stream);
        var tableType = tableFullName.GetClassType();
        if (tableType == null)
        {
            return new BadRequestObjectResult($"{L.TableNotFound} {tableFullName}");
        }

        ISheet sheet = workbook.GetSheetAt(0);
        var objectList = sheet.GetList(tableType);
        var dbContext = tableType.GetDbContextFromTableClassType(user);
        if (dbContext == null)
        {
            return new BadRequestObjectResult($"{L.DatabaseNotFound} <= {tableFullName}");
        }
        foreach (var item in objectList)
        {
            await dbContext.UpsertWithoutSaveAsync(item);
        }
        await dbContext.SaveChangesAsync();

        return new OkObjectResult(L.Success);
    }

    public static async Task<ObjectResult> ImportExcelAsync(string tableFullName, Stream stream, DbContext dbContext)
    {
        var workbook = WorkbookFactory.Create(stream);
        var tableType = tableFullName.GetClassType();
        if (tableType == null)
        {
            return new BadRequestObjectResult($"{L.TableNotFound} {tableFullName}");
        }

        ISheet sheet = workbook.GetSheetAt(0);
        var objectList = sheet.GetList(tableType);
        foreach (var item in objectList)
        {
            await dbContext.UpsertWithoutSaveAsync(item);
        }
        await dbContext.SaveChangesAsync();

        return new OkObjectResult(L.Success);
    }
}