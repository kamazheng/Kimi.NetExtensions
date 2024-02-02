using System.Linq.Dynamic.Core;
using System.Reflection;

public static class DynamicClassExtensions
{
    static DynamicClassExtensions()
    {
        LicenceHelper.CheckLicense();
    }

    /// <summary>
    /// 这个C#函数将一个List&lt;DynamicClass&gt;对象转换为一个List&lt;Dictionary&lt;string, object?&gt;&gt;对象。
    /// 它遍历DynamicClass对象列表并将每个对象转换为一个Dictionary&lt;string, object?&gt;对象，然后将其添加到新的列表中。最后，它返回新列表。
    /// </summary>
    /// <param name="dynamicClasses">
    /// </param>
    /// <returns>
    /// </returns>
    public static List<Dictionary<string, object?>> ToListDictionary(this List<DynamicClass> dynamicClasses)
    {
        List<Dictionary<string, object?>> dictionaries = new List<Dictionary<string, object?>>();
        foreach (var dynamicClass in dynamicClasses)
        {
            var dictionary = dynamicClass.ToDictionary();
            dictionaries.Add(dictionary);
        }
        return dictionaries;
    }

    /// <summary>
    /// 这个C#函数将一个DynamicClass对象转换为一个Dictionary对象，其中键是属性的名称，值是属性的值。 它使用反射来获取对象的公共实例属性，并使用GetValue方法获取属性的值。如果Dictionary为空，返回一个空的Dictionary。
    /// </summary>
    /// <param name="dynamicClass">
    /// </param>
    /// <returns>
    /// </returns>
    public static Dictionary<string, object?> ToDictionary(this DynamicClass dynamicClass)
    {
        var dictionary = dynamicClass.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(dynamicClass, null));
        return dictionary ?? new Dictionary<string, object?>();
    }
}