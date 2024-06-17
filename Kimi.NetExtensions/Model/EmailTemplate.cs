using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

[Table(nameof(EmailTemplate), Schema = DbSchema.Reference)]
public partial class EmailTemplate : BaseObject, ISoftDeleteEntity, IWriteAccessEntity, IReadAccessEntity
{
    [MaxLength(500)]
    [Display(Name = "Remark", ResourceType = typeof(L))]
    public string? Remark { get; set; }

    [Required]
    [MaxLength(500)]
    [Display(Name = "Subject", ResourceType = typeof(L))]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    [Unicode(false)]
    [Display(Name = "ToList", ResourceType = typeof(L))]
    public string ToList { get; set; } = string.Empty;

    [Column("CCList")]
    [MaxLength(1000)]
    [Unicode(false)]
    [Display(Name = "Cclist", ResourceType = typeof(L))]
    public string? Cclist { get; set; }

    [Required]
    [MaxLength(-1)]
    [Display(Name = "TemplateHtml", ResourceType = typeof(L))]
    public string? TemplateHtml { get; set; }
}