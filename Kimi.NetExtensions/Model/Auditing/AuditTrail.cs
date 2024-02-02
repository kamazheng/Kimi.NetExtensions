using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Kimi.NetExtensions.Model.Auditing;

public class AuditTrail
{
    public AuditTrail(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string UserId { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();
    public TrailType TrailType { get; set; }
    public List<string> ChangedColumns { get; } = new();
    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public Trail ToAuditTrail() =>
        new()
        {
            UserId = UserId,
            Type = TrailType.ToString(),
            TableName = TableName,
            AuditOn = DateTime.UtcNow,
            PrimaryKey = JsonSerializer.Serialize(KeyValues),
            OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
            AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns),
            Updated = DateTime.UtcNow,
            Updatedby = UserId,
            Active = true
        };
}