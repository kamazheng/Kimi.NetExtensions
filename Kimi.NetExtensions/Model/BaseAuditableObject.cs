using Kimi.NetExtensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kimi.NetExtensions.Model;

public abstract class BaseAuditableObject : BaseObject, IAuditableEntity
{
    [Column("CREATEDBY", Order = int.MaxValue - 4)]
    [MaxLength(50)]
    [Display(Name = nameof(CreatedBy), ResourceType = typeof(L))]
    public string CreatedBy { get; set; } = string.Empty;

    [Column("CREATEDON", Order = int.MaxValue - 3)]
    [Precision(3)]
    [Display(Name = nameof(CreatedOn), ResourceType = typeof(L))]
    public DateTime CreatedOn { get; set; }
}