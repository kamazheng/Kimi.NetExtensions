using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model.Identities;

[Table(nameof(User), Schema = DbSchema.Reference)]
public partial class User : ISoftDeleteEntity, IAuditableEntity, IWriteAccessEntity, IReadAccessEntity
{
    [Key]
    [Column("UserPK")]
    public int UserPk { get; set; }

    [StringLength(50)]
    [Required]
    [Display(Name = "LoginId", ResourceType = typeof(L))]
    public string LoginId { get; set; } = null!;

    [StringLength(200)]
    [Required]
    [Display(Name = "Name", ResourceType = typeof(L))]
    public string Name { get; set; } = null!;

    [StringLength(100, MinimumLength = 8)]
    [Required]
    [Display(Name = "Password", ResourceType = typeof(L))]
    public string Password { get; set; } = null!;

    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "Email", ResourceType = typeof(L))]
    public string? Email { get; set; }

    [StringLength(200)]
    [Display(Name = "Description", ResourceType = typeof(L))]
    public string? Description { get; set; }

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
}