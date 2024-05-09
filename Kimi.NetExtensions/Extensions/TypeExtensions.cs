using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

public static class TypeExtensions
{
    static TypeExtensions()
    {
        LicenceHelper.CheckLicense();
    }

    public static Assembly[] NotSystemAssemblies { get; set; }
        = AppDomain.CurrentDomain.GetAssemblies()
        .Where(x => !string.IsNullOrEmpty(x.FullName) && !x.FullName.StartsWith("System") && !x.FullName.StartsWith("Microsoft")).ToArray();

    private static Func<object, object> MakeCastDelegate(Type from, Type to)
    {
        var p = Expression.Parameter(typeof(object)); //do not inline
        return Expression.Lambda<Func<object, object>>(
            Expression.Convert(Expression.ConvertChecked(Expression.Convert(p, from), to), typeof(object)),
            p).Compile();
    }

    private static readonly Dictionary<Tuple<Type, Type>, Func<object, object>> CastCache
    = new Dictionary<Tuple<Type, Type>, Func<object, object>>();

    public static Func<object, object> GetCastDelegate(Type from, Type to)
    {
        lock (CastCache)
        {
            var key = new Tuple<Type, Type>(from, to);
            Func<object, object>? cast_delegate;
            if (!CastCache.TryGetValue(key, out cast_delegate))
            {
                cast_delegate = MakeCastDelegate(from, to);
                CastCache.Add(key, cast_delegate);
            }
            return cast_delegate;
        }
    }

    public static object Cast(Type t, object o)
    {
        return GetCastDelegate(o.GetType(), t).Invoke(o);
    }

    public static object CastToType(this object o, Type t)
    {
        return GetCastDelegate(o.GetType(), t).Invoke(o);
    }

    public static T? ChangeType<T>(object value)
    {
        var t = typeof(T);

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
            {
                return default;
            }

            t = Nullable.GetUnderlyingType(t);
        }

        return (T)Convert.ChangeType(value, t!);
    }

    public static object? ChangeType(object value, Type conversion)
    {
        var t = conversion;

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value is null)
            {
                return null;
            }
            t = Nullable.GetUnderlyingType(t);
        }
        object result;
        try
        {
            result = Convert.ChangeType(value, t!);
            return result;
        }
        catch (InvalidCastException)
        {
            if (t == typeof(DateTime) && value.ToString()!.IsDouble())
            {
                return DateTime.FromOADate((double)value);
            }
            return TypeDescriptor.GetConverter(t!).ConvertFromInvariantString(value.ToString()!);
        }
    }

    /// <summary>
    /// Get all prooerties of Type, including on MetadataTypeAttribute
    /// </summary>
    /// <param name="type">
    /// </param>
    /// <returns>
    /// </returns>
    public static IDictionary<string, IEnumerable<Attribute>> GetAllPropertyAttributes(this Type type)
    {
        var metadataTypes = type.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                               .OfType<MetadataTypeAttribute>();
        var properties = type.GetProperties();
        var result = new Dictionary<string, IEnumerable<Attribute>>();

        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(true).OfType<Attribute>();
            foreach (var metadataType in metadataTypes)
            {
                var metadataProperty = metadataType.MetadataClassType.GetProperty(property.Name);
                if (metadataProperty != null)
                {
                    attributes = attributes.Concat(metadataProperty.GetCustomAttributes(true).OfType<Attribute>());
                }
            }
            result.Add(property.Name, attributes);
        }

        return result;
    }

    /// <summary>
    /// Get all prooerties of Type, including on MetadataTypeAttribute
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    /// </returns>
    public static IDictionary<string, IEnumerable<Attribute>> GetAllPropertyAttributes<T>()
    {
        var type = typeof(T);
        return type.GetAllPropertyAttributes();
    }

    public static bool IsNumeric(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    public static bool IsInteger(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return true;

            default:
                return false;
        }
    }

    public static bool IsDecimal(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    public static bool IsBool(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(bool);
    }

    public static bool IsDateTime(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(DateTime);
    }

    public static bool IsString(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(string);
    }

    /// <summary>
    /// Get all types for which implement interface T
    /// </summary>
    /// <typeparam name="T">
    /// Interface
    /// </typeparam>
    /// <returns>
    /// </returns>
    public static IEnumerable<Type> GetTypesForImplementInterface<T>()
    {
        return NotSystemAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
    }

    public static IEnumerable<Type> GetAllSubClassOf<T>()
    {
        return NotSystemAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(T)));
    }

    /// <summary>
    /// Get the type by its class name which inherit from type T
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <param name="className">
    /// </param>
    /// <returns>
    /// </returns>
    public static Type? GetTypeByNameInheritFrom<T>(this string className) where T : class
    {
        var baseType = typeof(T);
        var assembly = baseType.Assembly;

        foreach (var type in NotSystemAssemblies
            .SelectMany(a => a.GetTypes()))
        {
            if (type.Name == className && (baseType.IsAssignableFrom(type) || type.IsSubclassOf(baseType)))
            {
                return type;
            }
        }

        return null;
    }

    /// <summary> Get the first type which implement Interface<T> </summary> <param
    /// name="interfaceType"> typeof(Interface<>) make generic type empty </param> <param
    /// name="genericType"> typeof(generic type) </param> <returns></returns> <exception
    /// cref="ArgumentNullException"></exception> <exception cref="ArgumentException"></exception>
    public static Type? GetFirstTypeImplementGenericInterface(this Type interfaceType, Type genericType)
    {
        if (interfaceType == null || genericType == null)
        {
            throw new ArgumentNullException(nameof(interfaceType), nameof(genericType));
        }
        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException("InterfaceType must be an interface type.", nameof(interfaceType));
        }
        var types = NotSystemAssemblies.SelectMany(a => a.GetTypes());
        return types.FirstOrDefault(t => t.GetInterfaces()?.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType.MakeGenericType(genericType)) == true);
    }

    /// <summary>
    /// Return the first baseClass T
    /// </summary>
    /// <param name="baseClass">
    /// </param>
    /// <param name="genericType">
    /// </param>
    /// <returns>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public static Type? GetFirstGenericType(this Type baseClass, Type genericType)
    {
        if (baseClass == null || genericType == null)
        {
            throw new ArgumentNullException(nameof(baseClass), nameof(genericType));
        }
        var types = NotSystemAssemblies.SelectMany(a => a.GetTypes());
        var baseClassGeneric = baseClass.MakeGenericType(genericType);
        return types.FirstOrDefault(t => t.IsClass && baseClassGeneric.IsAssignableFrom(t));
    }

    public static string GetKeyName(this Type type)
    {
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var keyAttr = (KeyAttribute?)property.GetAttributeIncludingMetadata(typeof(KeyAttribute));
            if (keyAttr != null)
            {
                return property.Name;
            }
        }
        return "";
    }

    public static FieldInfo? GetPrivateField(this Type? t, string name)
    {
        const BindingFlags bf = BindingFlags.Instance |
                                BindingFlags.NonPublic |
                                BindingFlags.DeclaredOnly;
        FieldInfo? fi = null;
        while ((fi == null) && (t != null))
        {
            fi = t.GetField(name, bf);
            t = t.BaseType;
        }
        return fi;
    }

    public static bool IsNullable(this Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) != null;
    }

    public static string GetDisplayLabel(this Type type)
    {
        DisplayAttribute? displayAttr = (DisplayAttribute?)type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(DisplayAttribute));
        if (displayAttr != null)
        {
            var resourceManager = displayAttr.ResourceType != null ? new ResourceManager(displayAttr.ResourceType) : null;
            var culture = CultureInfo.CurrentUICulture;
            return resourceManager?.GetString(displayAttr.Name!, culture) ?? displayAttr.Name!;
        }
        else
        {
            return type.Name;
        }
    }
}