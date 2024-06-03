public static class DateTimeExtensions
{
    static DateTimeExtensions()
    {
        LicenceHelper.CheckLicense();
    }

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime ToChinaDateTime(this DateTime utcDateTime)
    {
        return utcDateTime.AddHours(8);
    }

    /// <summary>
    /// 这个C#函数是一个扩展方法，将给定的DateTime对象转换为ISO 8601日期时间格式，并返回转换后的字符串。它使用DateTime的ToString方法以"O"格式表示日期时间。
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static string ToIsoDateTime(this DateTime dateTime)
    {
        return dateTime.ToString("O");
    }

    /// <summary>
    /// 该函数将传入的DateTime对象转换为Unix时间戳（以毫秒为单位）。它首先将日期转换为协调世界时（UTC），然后减去Unix纪元的时间差，最后返回转换后的毫秒数。
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static long ToUnixTimeMilliseconds(this DateTime date)
    {
        return (long)(date.ToUniversalTime() - Epoch).TotalMilliseconds;
    }

    public static DateTime ReplaceDate(this DateTime? input, DateTime? newDay)
    {
        if (input.HasValue && newDay.HasValue)
        {
            return new DateTime(newDay.Value.Year, newDay.Value.Month, newDay.Value.Day, input.Value.Hour, input.Value.Minute, input.Value.Second, input.Value.Millisecond);
        }
        else
        {
            return input ?? DateTime.MinValue;
        }
    }

    public static DateTime ReplaceTime(this DateTime? input, DateTime? newTime)
    {
        if (input.HasValue && newTime.HasValue)
        {
            return new DateTime(input.Value.Year, input.Value.Month, input.Value.Day, newTime.Value.Hour, newTime.Value.Minute, newTime.Value.Second, newTime.Value.Millisecond);
        }
        else
        {
            return input ?? DateTime.MinValue;
        }
    }
}