using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Index(nameof(Name), nameof(Updated), nameof(Updatedby))]
public abstract class BaseOject : IBaseOject, ISoftDeleteEntity
{
    [Column(nameof(Id), Order = 1)]
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column(nameof(Name), Order = 2)]
    [Display(Name = nameof(Name), ResourceType = typeof(L))]
    public string Name { get; set; } = String.Empty;

    [StringLength(200)]
    [Column(nameof(Description), Order = 3)]
    [Display(Name = nameof(Description), ResourceType = typeof(L))]
    public string? Description { get; set; } = string.Empty;

    [Column("UPDATEDBY", Order = int.MaxValue - 2)]
    [StringLength(50)]
    [Required]
    [Display(Name = nameof(Updatedby), ResourceType = typeof(L))]
    public string Updatedby { get; set; } = string.Empty;

    [Column("UPDATED", Order = int.MaxValue - 1)]
    [Precision(3)]
    [Required]
    [Display(Name = nameof(Updated), ResourceType = typeof(L))]
    public DateTime Updated { get; set; }

    [Column("ACTIVE", Order = int.MaxValue)]
    [Required]
    [Display(Name = nameof(Active), ResourceType = typeof(L))]
    public bool Active { get; set; } = true;
}