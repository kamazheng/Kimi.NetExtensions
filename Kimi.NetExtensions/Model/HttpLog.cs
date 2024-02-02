using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table(nameof(HttpLog), Schema = DbSchema.Data)]
[Index(nameof(User))]
public class HttpLog : BaseOject, IReadAccessEntity, IWriteAccessEntity
{
    [StringLength(200)]
    public string ExceptionType { get; set; } = string.Empty;

    [StringLength(50)]
    public string RequestPath { get; set; } = string.Empty;

    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    [StringLength(50)]
    public string? User { get; set; }
}