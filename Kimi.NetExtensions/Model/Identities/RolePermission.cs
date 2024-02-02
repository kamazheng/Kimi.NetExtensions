using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(RolePermission), Schema = DbSchema.Reference)]
public partial class RolePermission : IAuditableEntity, ISoftDeleteEntity, IWriteAccessEntity, IReadAccessEntity
{
    [Key]
    [Column("RolePermissionPK")]
    public int RolePermissionPk { get; set; }

    [Column("RoleFK")]
    [Required]
    public int RoleFk { get; set; }

    [Column("PermissionName")]
    [Required]
    [MaxLength(150)]
    [Display(Name = "PermissionName", ResourceType = typeof(L))]
    public string PermissionName { get; set; } = string.Empty;

    [Column("Description")]
    [MaxLength(500)]
    [Display(Name = "Description", ResourceType = typeof(L))]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string CreatedBy { get; set; } = string.Empty;

    [Precision(3)]
    public DateTime CreatedOn { get; set; }

    [Column("UPDATEDBY")]
    [StringLength(50)]
    public string Updatedby { get; set; } = null!;

    [Column("UPDATED")]
    [Precision(3)]
    public DateTime Updated { get; set; }

    [Column("ACTIVE")]
    public bool Active { get; set; }

    [ForeignKey("RoleFk")]
    [InverseProperty("RolePermissions")]
    public virtual Role Role { get; set; } = null!;
}