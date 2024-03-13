using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table("Setting", Schema = DbSchema.Reference)]
public partial class Setting : ISoftDeleteEntity, IAuditableEntity, IWriteAccessEntity
{
    [Key]
    [Column("SettingPK")]
    public int SettingPk { get; set; }

    [StringLength(50)]
    [Display(Name = "Key", ResourceType = typeof(L))]
    public string Key { get; set; } = null!;

    [Display(Name = "IsSystem", ResourceType = typeof(L))]
    public bool IsSystem { get; set; } = false;

    [Display(Name = "NumericValue", ResourceType = typeof(L))]
    public double? NumericValue { get; set; }

    [Display(Name = "StringValue", ResourceType = typeof(L))]
    public string? StringValue { get; set; }

    [Column(TypeName = "datetime")]
    [Display(Name = "DateValue", ResourceType = typeof(L))]
    public DateTime? DateValue { get; set; }

    [Display(Name = "BoolValue", ResourceType = typeof(L))]
    public bool? BoolValue { get; set; }

    [Display(Name = "SysDescription", ResourceType = typeof(L))]
    [StringLength(200)]
    public string? SysDescription { get; set; }

    [Display(Name = "UserDescription", ResourceType = typeof(L))]
    [StringLength(200)]
    public string? UserDescription { get; set; }

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