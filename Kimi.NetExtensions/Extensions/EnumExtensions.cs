namespace Kimi.NetExtensions.Extensions;

public static class EnumExtensions
{
    public static T ParseEnum<T>(string? value, T defaultValue) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("T must be an enumerated type");

        if (string.IsNullOrEmpty(value))
            return defaultValue;

        foreach (T item in Enum.GetValues(typeof(T)))
        {
            if (item.ToString().Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                return item;
        }

        return defaultValue;
    }
}