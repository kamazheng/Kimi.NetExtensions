using Kimi.NetExtensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(UserRole), Schema = DbSchema.Reference)]
public partial class UserRole : BaseAuditableObject, IWriteAccessEntity, IReadAccessEntity
{
    [Display(Name = "UserId", ResourceType = typeof(L))]
    public string UserId { get; set; } = null!;

    [Display(Name = "Domain", ResourceType = typeof(L))]
    public string Domain { get; set; } = null!;

    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;
}