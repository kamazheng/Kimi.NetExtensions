using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table(nameof(EmailTemplate), Schema = DbSchema.Reference)]
public partial class EmailTemplate : ISoftDeleteEntity, IWriteAccessEntity, IReadAccessEntity
{
    [Key]
    [Column("EmailTemplatePK")]
    public int EmailTemplatePk { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Name", ResourceType = typeof(L))]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Remark", ResourceType = typeof(L))]
    public string? Remark { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Subject", ResourceType = typeof(L))]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [Unicode(false)]
    [Display(Name = "ToList", ResourceType = typeof(L))]
    public string ToList { get; set; } = string.Empty;

    [Column("CCList")]
    [StringLength(1000)]
    [Unicode(false)]
    [Display(Name = "Cclist", ResourceType = typeof(L))]
    public string? Cclist { get; set; }

    [Required]
    [Display(Name = "TemplateHtml", ResourceType = typeof(L))]
    public string? TemplateHtml { get; set; }

    [Column("UPDATEDBY")]
    [StringLength(50)]
    public string Updatedby { get; set; } = string.Empty;

    [Column("UPDATED")]
    [Precision(3)]
    public DateTime Updated { get; set; }

    [Column("ACTIVE")]
    public bool Active { get; set; }
}