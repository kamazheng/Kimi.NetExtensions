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

    [StringLength(50)]
    public string? Type { get; set; }

    [StringLength(100)]
    public string? TableName { get; set; }

    [Precision(3)]
    public DateTime AuditOn { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    [StringLength(500)]
    public string? AffectedColumns { get; set; }

    [StringLength(100)]
    public string? PrimaryKey { get; set; }

    [Column("UPDATEDBY")]
    [StringLength(50)]
    public string Updatedby { get; set; } = null!;

    [Column("UPDATED")]
    [Precision(3)]
    public DateTime Updated { get; set; }

    [Column("ACTIVE")]
    public bool Active { get; set; }
}