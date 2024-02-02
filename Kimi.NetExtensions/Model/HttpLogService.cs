using Kimi.NetExtensions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Kimi.NetExtensions.Model;

public class HttpLogService : IHttpLog
{
    private DbContext _db;

    public int Id { get; set; }
    public string ExceptionType { get; set; }
    public string RequestPath { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public string ErrorMessage { get; set; }
    public string? User { get; set; }
    public string Updatedby { get; set; }
    public DateTime Updated { get; set; }
    public bool Active { get; set; }

    public HttpLogService()
    {
        _db = typeof(HttpLog).GetDbContextFromTableClassType(new SysUser())!;
    }

    private HttpLog ToHttpLog()
    {
        return this.JsonCopy<HttpLog>()!;
    }

    public async Task<bool> IsLogOnAsync()
    {
        var tableQuery = new TableQuery
        {
            TableClassFullName = typeof(Setting).FullName,
            WhereClause = $"{nameof(Setting.Key)}=\"HttpRequestLog\"",
        };
        var result = await Task.Run(() => DbContextExtension.GetDbRecordsByDynamicLinq(tableQuery, new SysUser()));
        var setting = result?.ToDynamicList<Setting>()?.FirstOrDefault();
        return setting?.BoolValue ?? false;
    }

    public async Task SaveAsync()
    {
        _db.Add(this.ToHttpLog());
        await _db.SaveChangesAsync();
    }
}