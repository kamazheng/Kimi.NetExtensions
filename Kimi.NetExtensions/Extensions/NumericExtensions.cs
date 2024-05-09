/// <summary>
/// 将双精度数字转换为 humanized 的形式，即以中文或英文表示的可读形式。函数参数 digits 是可选的，表示保留的小数位数。
/// 如果数字为默认值，则返回默认值。函数通过计算数字的绝对值和使用数学函数来确定数字的量级，并将其转换为 shortNumber。
/// 然后，根据量级的值返回不同的后缀。如果量级小于0，则返回 "MIN"，如果量级大于12，则返回 "MAX"。最后，将 shortNumber 和后缀连接起来，并返回结果。
/// 量级定义：string[] suffix = { "f", "a", "p", "n", "μ", "m", string.Empty, "k", "M", "G", "T", "P", "E" };
/// </summary>
public static class NumericExtensions
{
    public static string Humanize(this double? number, int digits = 1)
    {
        if (number == default)
        {
            return "0";
        }
        string[] suffix = { "f", "a", "p", "n", "μ", "m", string.Empty, "k", "M", "G", "T", "P", "E" };

        var absnum = Math.Abs(number!.Value);

        int mag;
        if (absnum < 1)
        {
            mag = (int)Math.Floor(Math.Floor(Math.Log10(absnum)) / 3);
        }
        else
        {
            mag = (int)(Math.Floor(Math.Log10(absnum)) / 3);
        }

        var shortNumber = number!.Value / Math.Pow(10, mag * 3);
        shortNumber = Math.Round(shortNumber, digits);

        if ((mag + 6) < 0) return "MIN";
        if ((mag + 6) > 12) return "MAX";
        return $"{shortNumber}{suffix[mag + 6]}";
    }

    public static string Humanize(this float? number, int digits = 1)
    {
        if (number == default) return "NULL";
        return Humanize((double)number!.Value, digits);
    }

    /// <summary>
    /// 这个C#函数用于判断一个double类型的值是否在指定的范围内。函数接受四个参数：value（待判断的值）、min（范围的下界）、max（范围的上界）和decimalPlaces（小数点后的位数）。
    /// 函数通过计算一个epsilon值来确定比较时的精度，然后使用双等号（>=和<=）判断value是否在[min-epsilon,max+epsilon]范围内。如果在范围内，则返回true；
    /// 否则返回false。
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static bool IsInRange(this double value, double min, double max, int decimalPlaces)
    {
        double epsilon = Math.Pow(10, -decimalPlaces);
        return (value >= (min - epsilon) && value <= (max + epsilon));
    }

    public static bool IsInRange(this double value, decimal min, decimal max, int decimalPlaces)
    {
        return IsInRange((double)value, (double)min, (double)max, decimalPlaces);
    }

    public static T RoundToPlaces<T>(this T input, int places, MidpointRounding mode = MidpointRounding.AwayFromZero) where T : IConvertible
    {
        if (places < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(places), "Decimal places must be non-negative.");
        }

        double result = Math.Round(Convert.ToDouble(input), places, mode);
        return (T)Convert.ChangeType(result, typeof(T));
    }
}