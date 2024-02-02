using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

public static class PropertyExtentions
{
    static PropertyExtentions()
    {
        LicenceHelper.CheckLicense();
    }

    public static object GetPropertyValueByExpression<Tobj>(this Tobj self, string propertyName) where Tobj : class
    {
        var param = Expression.Parameter(typeof(Tobj), "value");
        var getter = Expression.Property(param, propertyName);
        var boxer = Expression.TypeAs(getter, typeof(object));
        var getPropValue = Expression.Lambda<Func<Tobj, object>>(boxer, param).Compile();
        return getPropValue(self);
    }

    public static object? GetPropertyValueByReflection<Tobj>(this Tobj self, string propertyName) where Tobj : class
    {
        return self.GetType().GetProperty(propertyName)?.GetValue(self);
    }

    public static void SetPropertyValue<Tobj>(this Tobj self, string propertyName, object? value)
    {
        if (self == null) return;
        if (value == default || string.IsNullOrEmpty(value.ToString()))
        {
            self.GetType().GetProperty(propertyName)?.SetValue(self, default);
        }
        else
        {
            var convertValue = TypeExtensions.ChangeType(value, self.GetType().GetProperty(propertyName)?.PropertyType!);
            self.GetType().GetProperty(propertyName)?.SetValue(self, convertValue);
        }
    }

    public static void SetPropertyValues<Tobj>(this Tobj self, Tobj sourceObj)
    {
        var properties = typeof(Tobj).GetProperties();
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                self.SetPropertyValue(property.Name, property.GetValue(sourceObj));
            }
        }
    }

    public static Attribute? GetAttributeIncludingMetadata(this PropertyInfo propertyInfo, Type attributeType)
    {
        // First, check for the attribute on the property itself
        if (propertyInfo == null || attributeType == null)
        {
            return null;
        }
        var attribute = Attribute.GetCustomAttribute(propertyInfo, attributeType);
        if (attribute != null)
        {
            return attribute;
        }

        // If the attribute is not found on the property, check for a MetadataTypeAttribute on the class
        if (propertyInfo.DeclaringType == null)
        {
            return null;
        }
        var metadataTypeAttribute = Attribute.GetCustomAttribute(propertyInfo.DeclaringType, typeof(MetadataTypeAttribute)) as MetadataTypeAttribute;
        if (metadataTypeAttribute != null)
        {
            // If a MetadataTypeAttribute is found, use reflection to get the property from the
            // metadata class
            var metadataProperty = metadataTypeAttribute.MetadataClassType.GetProperty(propertyInfo.Name);
            if (metadataProperty != null)
            {
                // If the property is found in the metadata class, check for the attribute on the
                // metadata property
                attribute = Attribute.GetCustomAttribute(metadataProperty, attributeType);
                if (attribute != null)
                {
                    return attribute;
                }
            }
        }
        return null;
    }

    public static string? GetDisplayLabel(Expression<Func<object>> expression)
    {
        var property = GetPropertyInfo(expression.Body);
        return property.GetDisplayLabel();
    }

    public static string? GetDisplayLabel<T>(Expression<Func<T, object>> expression)
    {
        var property = GetPropertyInfo(expression.Body);
        return property.GetDisplayLabel();
    }

    public static string? GetDisplayLabel(this PropertyInfo property)
    {
        if (property == null)
        {
            return default;
        }
        if (property.GetAttributeIncludingMetadata(typeof(DisplayAttribute)) is DisplayAttribute displayAttr
            && !string.IsNullOrEmpty(displayAttr.Name))
        {
            var resourceManager = displayAttr.ResourceType != null ? new ResourceManager(displayAttr.ResourceType) : null;
            var culture = CultureInfo.CurrentCulture;
            return resourceManager?.GetString(displayAttr.Name, culture) ?? displayAttr.Name;
        }
        else
        {
            return property.Name;
        }
    }

    /// <summary>
    /// get the property info by input express
    /// </summary>
    /// <param name="expression">
    /// The body of express
    /// </param>
    /// <returns>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// </exception>
    public static PropertyInfo GetPropertyInfo(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Member is PropertyInfo property)
                return property;

            throw new ArgumentException("Expression is not a property access");
        }

        if (expression is UnaryExpression unaryExpression)
            return GetPropertyInfo(unaryExpression.Operand);

        throw new ArgumentException("Invalid expression");
    }

    /// <summary>
    /// Get the full path of the property Property.SubProperty
    /// </summary>
    /// <param name="expression">
    /// The body of property
    /// </param>
    /// <returns>
    /// </returns>
    public static string GetFullPropertyName(Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            string prefix = GetFullPropertyName(memberExpression.Expression!);
            return string.IsNullOrEmpty(prefix) ? memberExpression.Member.Name : prefix + "." + memberExpression.Member.Name;
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            return GetFullPropertyName(unaryExpression.Operand);
        }
        return string.Empty;
    }

    public static bool IsNumericOrNullableNumeric(this PropertyInfo property)
    {
        var type = property.PropertyType;
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsPrimitive)
        {
            return type != typeof(bool) &&
                   type != typeof(char) &&
                   type != typeof(nint) &&
                   type != typeof(nuint);
        }
        return type == typeof(decimal);
    }

    public static bool IsTypeOrNullableType<T>(this PropertyInfo property)
    {
        var type = property.PropertyType;
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(T);
    }

    public static bool IsHumanReadable(this PropertyInfo property)
    {
        var p = property;
        return (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
            && (!p.PropertyType.IsGenericType || !typeof(IEnumerable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
            && (!p.PropertyType.IsGenericType || !typeof(ICollection<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));
    }

    public static IEnumerable<PropertyInfo> GetExcludedProperties(this Type thisType, params Type[] excludePropertyTypes)
    {
        var properties = thisType.GetProperties();
        foreach (var t in excludePropertyTypes)
        {
            bool isImplement = t.IsAssignableFrom(thisType);
            if (isImplement)
            {
                var tProperties = t.GetProperties();
                properties = properties.Where(p => !tProperties.Any(tp => tp.Name == p.Name)).ToArray();
            }
        }
        return properties;
    }
}