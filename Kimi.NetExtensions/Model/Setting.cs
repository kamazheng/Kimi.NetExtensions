using Kimi.NetExtensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table("Setting", Schema = DbSchema.Reference)]
public partial class Setting : BaseAuditableObject, ISoftDeleteEntity, IWriteAccessEntity
{
    [Display(Name = "Key", ResourceType = typeof(L))]
    public string Key { get; set; } = null!;

    [Display(Name = "IsSystem", ResourceType = typeof(L))]
    public bool IsSystem { get; set; } = false;

    [Display(Name = "NumericValue", ResourceType = typeof(L))]
    public double? NumericValue { get; set; }

    [MaxLength(-1)]
    [Display(Name = "StringValue", ResourceType = typeof(L))]
    public string? StringValue { get; set; }

    [Column(TypeName = "datetime")]
    [Display(Name = "DateValue", ResourceType = typeof(L))]
    public DateTime? DateValue { get; set; }

    [Display(Name = "BoolValue", ResourceType = typeof(L))]
    public bool? BoolValue { get; set; }

    [MaxLength(256)]
    [Display(Name = "SysDescription", ResourceType = typeof(L))]
    public string? SysDescription { get; set; }

    [MaxLength(256)]
    [Display(Name = "UserDescription", ResourceType = typeof(L))]
    public string? UserDescription { get; set; }
}