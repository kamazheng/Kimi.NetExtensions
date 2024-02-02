using Kimi.NetExtensions.Interfaces;
using Kimi.NetExtensions.Localization;
using Kimi.NetExtensions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System.Linq.Dynamic.Core;

namespace Kimi.NetExtensions.Controllers;

/// <summary>
/// CRUD for generic table by using dynamic linq
/// </summary>
[ApiController]
[Route("[controller]")]
[MiddlewareFilter(typeof(HttpLogMiddlewareAttribute))]
public class GenericTableController : ControllerBase
{
    private readonly ILogger<GenericTableController> _logger;
    private readonly IUser _user;

    /// <summary>
    /// </summary>
    /// <param name="logger">
    /// </param>
    public GenericTableController(ILogger<GenericTableController> logger, IUser user)
    {
        _logger = logger;
        _user = user;
    }

    /// <summary>
    /// Get All Table List by reflection, get dbsets of dbcontexts
    /// </summary>
    /// <param name="isShortName">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpGet]
    [Route("GetTableList/{isShortName?}")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<IActionResult> GetTableList(bool isShortName = true)
    {
        IEnumerable<string?> allTables = await Task.Run(() => DbContextExtension.GetAllTableClasses().Select(t => isShortName ? t.Name : t.FullName));
        return Ok(allTables);
    }

    /// <summary>
    /// Get all resource permission defined by ResourcePermission class, including table implement
    /// IRead/IWriteAccessEntity and manual definition
    /// </summary>
    /// <returns>
    /// </returns>
    [HttpGet]
    [Route("GetResourcePermissions")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public IActionResult GetResourcePermissions()
    {
        var result = ResourcePermission.GetAllResourcePermissions();
        var jsonResult = JsonConvert.SerializeObject(result);
        return Ok(jsonResult);
    }

    /// <summary>
    /// Get table records by dynamic linq
    /// </summary>
    /// <param name="tableQuery">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [Route("GetItems")]
    [ProducesResponseType(typeof(List<object>), 200)]
    public async Task<IActionResult> GetItems([FromBody] TableQuery tableQuery)
    {
        var aa = JsonConvert.SerializeObject(tableQuery);
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            return BadRequest(L.TableFullnameCannotBeNull);
        }

        var result = await Task.Run(() => DbContextExtension.GetDbRecordsByDynamicLinq(tableQuery, _user));
        var json = JsonConvert.SerializeObject(result);
        if (json.Contains("AnonymousType"))
        {
            var noTypeResult = JsonConvert.SerializeObject(result.ToDynamicList<object>(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None });
            return Ok(noTypeResult);
        }
        return Ok(json);
    }

    /// <summary>
    /// Find table record by Id
    /// </summary>
    /// <param name="query">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [Route("GetItem")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetItem([FromBody] RecordQuery query)
    {
        if (string.IsNullOrEmpty(query?.TableClassFullName))
        {
            return BadRequest(L.TableFullnameCannotBeNull);
        }
        var result = DbContextExtension.GetItem(query, _user);
        return Ok(result);
    }

    /// <summary>
    /// Update or Insert table item
    /// </summary>
    /// <param name="upsertBody">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPut]
    [Route("Upsert")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Upsert([FromBody] UpsertBody upsertBody)
    {
        var result = await DbContextExtension.UpsertAsync(upsertBody.JsonRecord, upsertBody.TableClassFullName, _user);
        var json = JsonConvert.SerializeObject(result);
        return Ok(json);
    }

    /// <summary>
    /// Delete table record by Id
    /// </summary>
    /// <param name="query">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpDelete]
    [Route("Delete")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Delete([FromBody] RecordQuery query)
    {
        var result = await DbContextExtension.DeleteAsync(query.TableClassFullName, query.Id, _user);
        var json = JsonConvert.SerializeObject(result);
        return Ok(json);
    }

    [HttpDelete]
    [Route("DeleteModel")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> DeleteModel([FromBody] UpsertBody query)
    {
        var result = await DbContextExtension.DeleteAsync(query.JsonRecord, _user);
        var json = JsonConvert.SerializeObject(result);
        return Ok(json);
    }

    /// <summary>
    /// Delete table records according filter expression
    /// </summary>
    /// <param name="query">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpDelete]
    [Route("DeleteWhere")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> DeleteWhere([FromBody] TableQuery query)
    {
        if (string.IsNullOrEmpty(query?.TableClassFullName)) return BadRequest(L.TableFullnameCannotBeNull);
        if (string.IsNullOrEmpty(query?.WhereClause)) return BadRequest(L.WhereClauseCannotBeNull);

        var result = await DbContextExtension.DeleteWhereAsync(query.TableClassFullName, query.WhereClause, _user);
        var json = JsonConvert.SerializeObject(result);
        return Ok(json);
    }

    /// <summary>
    /// Export table records to excel file
    /// </summary>
    /// <param name="tableQuery">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [Route("ExportExcel")]
    public async Task<IActionResult> Export([FromBody] TableQuery tableQuery)
    {
        if (string.IsNullOrEmpty(tableQuery?.TableClassFullName))
        {
            return BadRequest(L.TableFullnameCannotBeNull);
        }

        var result = await Task.Run(() => DbContextExtension
            .GetDbRecordsByDynamicLinq(tableQuery, _user)
            .ToDynamicList<object>());
        var file = await Task.Run(() => ExcelService.GenerateExcelWorkbook(result));
        var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"{tableQuery?.TableClassFullName}.xlsx";
        return File(file, mimeType, fileName);
    }

    /// <summary>
    /// Import table records from excel, empty primary key is insert, otherwise is update
    /// </summary>
    /// <param name="file">
    /// </param>
    /// <returns>
    /// </returns>
    [HttpPost]
    [Route("ImportExcel")]
    public async Task<IActionResult> ImportExcel([FromForm(Name = "file")] IFormFile file)
    {
        var tableType = file.FileName.GetClassType();
        if (tableType == null)
        {
            return BadRequest($"{L.TableNotFound} {file.FileName}");
        }

        IWorkbook workbook;
        using (var stream = file.OpenReadStream())
        {
            workbook = WorkbookFactory.Create(stream);
        }
        ISheet sheet = workbook.GetSheetAt(0);
        var objectList = sheet.GetList(tableType);
        var dbContext = tableType.GetDbContextFromTableClassType(_user);
        if (dbContext == null)
        {
            return BadRequest($"{L.DatabaseNotFound} <= {file.FileName}");
        }
        foreach (var item in objectList)
        {
            await dbContext.UpsertWithoutSaveAsync(item);
        }
        await dbContext.SaveChangesAsync();

        return Ok(L.Success);
    }
}