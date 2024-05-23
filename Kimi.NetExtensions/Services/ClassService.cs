namespace Kimi.NetExtensions.Services;

public static class ClassService
{
    /// <summary>
    /// Give class one identifier, even you changed the name of class, the class can still be found by the identifier.
    /// Use case: persist the class identifier in database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassIdAttribute : Attribute
    {
        public string Identifier { get; private set; }

        public ClassIdAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }

    public static string? GetClassId(Type type)
    {
        var attribute = (ClassIdAttribute?)Attribute.GetCustomAttribute(type, typeof(ClassIdAttribute));
        return attribute?.Identifier;
    }
}