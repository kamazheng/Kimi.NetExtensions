namespace Kimi.NetExtensions.Model.Identities;

public static partial class ResourceAction
{
    public static string Read
    { get { return nameof(Read); } }

    public static string Write
    { get { return nameof(Write); } }

    public static string Delete
    { get { return nameof(Delete); } }

    public static string Export
    { get { return nameof(Export); } }

    public static string Import
    { get { return nameof(Import); } }
}