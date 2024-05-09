using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Auditing;

[Table("AuditTrail", Schema = "Data")]
[Index(nameof(TableName), nameof(AuditOn), nameof(Updated), nameof(Updatedby))]
public class Trail : ISoftDeleteEntity, IReadAccessEntity
{
    [Key]
    [Column("AuditTrailPK")]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Type { get; set; }

    [MaxLength(100)]
    public string? TableName { get; set; }

    [Precision(3)]
    public DateTime AuditOn { get; set; }

    [MaxLength(-1)]
    public string? OldValues { get; set; }

    [MaxLength(-1)]
    public string? NewValues { get; set; }

    [MaxLength(500)]
    public string? AffectedColumns { get; set; }

    [MaxLength(100)]
    public string? PrimaryKey { get; set; }

    [Column("UPDATEDBY")]
    [MaxLength(50)]
    public string Updatedby { get; set; } = null!;

    [Column("UPDATED")]
    [Precision(3)]
    public DateTime Updated { get; set; }

    [Column("ACTIVE")]
    public bool Active { get; set; }
}