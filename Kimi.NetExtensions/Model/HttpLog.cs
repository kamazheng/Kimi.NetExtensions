using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table(nameof(HttpLog), Schema = DbSchema.Data)]
[Index(nameof(User))]
public class HttpLog : BaseObject, IReadAccessEntity, IWriteAccessEntity
{
    [MaxLength(200)]
    public string ExceptionType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string RequestPath { get; set; } = string.Empty;

    [MaxLength(-1)]
    public string? RequestBody { get; set; }

    [MaxLength(-1)]
    public string? ResponseBody { get; set; }

    [MaxLength(-1)]
    public string ErrorMessage { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? User { get; set; }
}