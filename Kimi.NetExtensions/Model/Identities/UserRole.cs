using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(UserRole), Schema = DbSchema.Reference)]
public partial class UserRole : ISoftDeleteEntity, IAuditableEntity, IWriteAccessEntity, IReadAccessEntity
{
    [Key]
    [Column("UserRolePK")]
    public int UserRolePk { get; set; }

    [StringLength(50)]
    [Display(Name = "Domain", ResourceType = typeof(L))]
    public string Domain { get; set; } = null!;

    [StringLength(50)]
    [Display(Name = "UserId", ResourceType = typeof(L))]
    public string UserId { get; set; } = null!;

    [Column("RoleFK")]
    public int RoleFk { get; set; }

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
    [InverseProperty("UserRoles")]
    public virtual Role Role { get; set; } = null!;
}