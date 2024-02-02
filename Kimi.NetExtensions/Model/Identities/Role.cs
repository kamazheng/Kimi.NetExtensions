using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(Role), Schema = DbSchema.Reference)]
public partial class Role : ISoftDeleteEntity, IAuditableEntity, IWriteAccessEntity, IReadAccessEntity
{
    [Key]
    [Column("RolePK")]
    public int RolePk { get; set; }

    [StringLength(50)]
    [Required]
    [Display(Name = "Name", ResourceType = typeof(L))]
    public string Name { get; set; } = null!;

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

    [InverseProperty("Role")]
    public virtual List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [InverseProperty("Role")]
    public virtual List<UserRole> UserRoles { get; set; } = new List<UserRole>();
}