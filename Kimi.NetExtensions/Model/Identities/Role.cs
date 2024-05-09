using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(Role), Schema = DbSchema.Reference)]
public partial class Role : BaseAuditableObject, IWriteAccessEntity, IReadAccessEntity
{
    public virtual List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual List<UserRole> UserRoles { get; set; } = new List<UserRole>();
}