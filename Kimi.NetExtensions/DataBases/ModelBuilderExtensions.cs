using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kimi.NetExtensions.Extensions;

public static class ModelBuilderExtensions
{
#pragma warning disable EF1001

    public static void SetMaxLength(this ModelBuilder modelBuilder, int stringLength)
    {
        // https://stackoverflow.com/a/50852517/909237
        // Use explicit setting instead of pre convention configuration, since the latter breaks [MaxLength] attributes
        // https://docs.microsoft.com/en-us/ef/core/modeling/bulk-configuration#pre-convention-configuration

        var stringProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(string))
            .OfType<Property>();

        foreach (var property in stringProperties)
            property.Builder.HasMaxLength(stringLength, ConfigurationSource.Convention);
    }

    public static void SetColumnType(this ModelBuilder modelBuilder, Type type, string columnType)
    {
        // https://stackoverflow.com/a/50852517/909237
        // Use explicit setting instead of pre convention configuration, since the latter breaks [MaxLength] attributes
        // https://docs.microsoft.com/en-us/ef/core/modeling/bulk-configuration#pre-convention-configuration

        var stringProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == type)
            .OfType<Property>();

        foreach (var property in stringProperties)
            property.Builder.HasColumnType(columnType);
    }
}