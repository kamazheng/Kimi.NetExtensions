using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Index(nameof(Updated), nameof(Updatedby))]
public abstract partial class BaseObject : IBaseOject, ISoftDeleteEntity
{
    [Column(nameof(Id), Order = 1)]
    [Key]
    public int Id { get; set; }

    [MaxLength(256)]
    [Column(nameof(Name), Order = 2)]
    [Display(Name = nameof(Name), ResourceType = typeof(L))]
    public string Name { get; set; } = String.Empty;

    [MaxLength(512)]
    [Column(nameof(Description), Order = 3)]
    [Display(Name = nameof(Description), ResourceType = typeof(L))]
    public string? Description { get; set; } = string.Empty;

    [Column("UPDATEDBY", Order = int.MaxValue - 2)]
    [MaxLength(50)]
    [Display(Name = nameof(Updatedby), ResourceType = typeof(L))]
    public string Updatedby { get; set; } = string.Empty;

    [Column("UPDATED", Order = int.MaxValue - 1)]
    [Precision(3)]
    [Display(Name = nameof(Updated), ResourceType = typeof(L))]
    public DateTime Updated { get; set; }

    [Column("ACTIVE", Order = int.MaxValue)]
    [Display(Name = nameof(Active), ResourceType = typeof(L))]
    public bool Active { get; set; } = true;
}