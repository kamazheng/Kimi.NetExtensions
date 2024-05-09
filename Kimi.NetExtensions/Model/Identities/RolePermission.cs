using Kimi.NetExtensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(RolePermission), Schema = DbSchema.Reference)]
public partial class RolePermission : BaseAuditableObject, IWriteAccessEntity, IReadAccessEntity
{
    [Required]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(150)]
    [Display(Name = "PermissionName", ResourceType = typeof(L))]
    public string PermissionName { get; set; } = string.Empty;

    public virtual Role Role { get; set; } = null!;
}