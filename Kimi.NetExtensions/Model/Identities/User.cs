using Kimi.NetExtensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(User), Schema = DbSchema.Reference)]
public partial class User : BaseAuditableObject, IWriteAccessEntity, IReadAccessEntity
{
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Password", ResourceType = typeof(L))]
    public string Password { get; set; } = null!;

    [EmailAddress]
    [Display(Name = "Email", ResourceType = typeof(L))]
    public string? Email { get; set; }
}