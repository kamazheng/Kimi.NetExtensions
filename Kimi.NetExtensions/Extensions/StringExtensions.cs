using Kimi.NetExtensions.Services;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

public static class StringExtentions
{
    static StringExtentions()
    {
        LicenceHelper.CheckLicense();
    }

    public static bool ContainDigit(this string inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            return false;
        }
        if (inputString.Any(c => char.IsDigit(c)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsInteger(this string theValue)
    {
        long retNum;
        return long.TryParse(theValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
    }

    public static bool IsDouble(this string theValue)
    {
        double retNum;
        return double.TryParse(theValue, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out retNum);
    }

    public static bool IsFloat(this string theValue)
    {
        float retNum;
        return float.TryParse(theValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out retNum);
    }

    /// <summary>
    /// 函数使用Replace方法将theValue中的所有removeStr替换为空字符串，并返回替换后的结果。
    /// </summary>
    /// <param name="theValue">
    /// </param>
    /// <param name="removeStr">
    /// </param>
    /// <returns>
    /// </returns>
    public static string Remove(this string theValue, string removeStr)
    {
        return theValue.Replace(removeStr, "");
    }

    public static string Right(this string sValue, int iMaxLength)
    {
        //Check if the value is valid
        if (string.IsNullOrEmpty(sValue))
        {
            //Set valid empty string as string could be null
            sValue = string.Empty;
        }
        else if (sValue.Length > iMaxLength)
        {
            //Make the string no longer than the max length
            sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
        }
        else
        {
            return sValue;
        }

        //Return the string
        return sValue;
    }

    public static string Left(this string sValue, int iMaxLength)
    {
        //Check if the value is valid
        if (string.IsNullOrEmpty(sValue))
        {
            //Set valid empty string as string could be null
            sValue = string.Empty;
        }
        else if (sValue.Length > iMaxLength)
        {
            //Make the string no longer than the max length
            sValue = sValue.Substring(0, iMaxLength);
        }
        else
        {
            return sValue;
        }

        //Return the string
        return sValue;
    }

    /// <summary>
    /// return string ellipsis with "..." appendix
    /// </summary>
    /// <param name="sValue">
    /// </param>
    /// <param name="iMaxLength">
    /// </param>
    /// <returns>
    /// </returns>
    public static string Ellipsis(this string sValue, int iMaxLength)
    {
        return sValue.Length > iMaxLength ? sValue.Left(iMaxLength - 3) + "..." : sValue;
    }

    public static string Bg(this string selfString, string bgLanguage)
    {
        if (string.IsNullOrEmpty(bgLanguage))
        {
            return "";
        }
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var cn = selfString.HasChineseChar() ? selfString : bgLanguage;
        var en = selfString.HasChineseChar() ? bgLanguage : selfString;
        return currentCulture == "zh-CN" ? cn : en;
    }

    public static bool HasChineseChar(this string inChar)
    {
        if (string.IsNullOrEmpty(inChar))
        {
            return false;
        }
        for (int i = inChar.Length - 1; i >= 0; i--)
        {
            Regex rx = new Regex("^[\u4e00-\u9fa5]$");
            if (rx.IsMatch(inChar[i].ToString())) return true;
        }
        return false;
    }

    public static bool HasSqlCompareOperators(this string inString)
    {
        if (string.IsNullOrEmpty(inString))
        {
            return false;
        }
        Regex rx = new Regex("[,<>=]");
        if (rx.IsMatch(inString) || inString.ToLower().Contains(" like ") || inString.ToLower().Contains(" in ") || inString.ToLower().Contains(" is "))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static object? CreateClassInstance(this string className)
    {
        Type? classType = className.GetClassType();
        if (classType == null) return null;
        return Activator.CreateInstance(classType);
    }

    public static Type? GetClassType(this string classFullName)
    {
        Type? classType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName == classFullName);
        return classType;
    }

    public static int? ToNullableInt(this string? s)
    {
        int i;
        if (int.TryParse(s, out i)) return i;
        return null;
    }

    /// <summary> Replace words in a sentence by dictionary<target_word_string, replace_string>
    /// Replaces words enclosed in square brackets, keep brackets. </summary> <param
    /// name="sourceString"></param> <param name="args"></param> <returns></returns>
    public static string ReplaceWordsInSquareBracket(this string sourceString, Dictionary<string, string> args)
    {
        Regex re = new Regex(@"\[(\w+)\]", RegexOptions.Compiled);
        string output = re.Replace(sourceString,
            match => args.TryGetValue(match.Groups[1].Value, out string? value)
                ? "[" + args[match.Groups[1].Value] + "]" : match.Value
        );
        return output;
    }

    /// <summary>
    /// 这个C#函数用于将输入的字符串转换为整数。如果输入为空或 null 或包含非数字字符，则返回默认值。
    /// 函数使用正则表达式删除字符串中的非数字字符，并将其转换为整数。如果转换后的字符串长度小于等于一个整数的最大值，则返回转换后的整数； 否则，返回最大长度减1的整数。
    /// </summary>
    /// <param name="input">
    /// </param>
    /// <returns>
    /// </returns>
    public static int GetNumbersAsInteger(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return default;
        }
        var numStr = Regex.Replace(input, @"[^0-9]", "");
        var maxIntStr = int.MaxValue.ToString();
        if (maxIntStr.Length > numStr.Length)
        {
            return Convert.ToInt32(numStr);
        }
        else
        {
            return Convert.ToInt32(numStr.Right(maxIntStr.Length - 1));
        }
    }

    public static string? MidStrEx(string sourse, string startstr, string endstr)
    {
        string result = string.Empty;
        int startindex, endindex;
        try
        {
            startindex = sourse.IndexOf(startstr);
            if (startindex == -1)
                return result;
            string tmpstr = sourse.Substring(startindex + startstr.Length);
            endindex = tmpstr.IndexOf(endstr);
            if (endindex == -1)
                return result;
            result = tmpstr.Remove(endindex);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            return null;
        }
        return result;
    }

    public static string TryFormatJson(this string json)
    {
        if (json.IsValidJson())
        {
            dynamic? parsedJson = JsonConvert.DeserializeObject(json!);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
        else
        {
            return json;
        }
    }

    public static bool IsValidJson(this string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput)) { return false; }
        strInput = strInput.Trim();
        if (strInput.StartsWith("{") && strInput.EndsWith("}") || //For object
            strInput.StartsWith("[") && strInput.EndsWith("]")) //For array
        {
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (Exception ex) when (ex is JsonReaderException or Exception)
            {
                //Exception in parsing json
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static T? ToType<T>(this string jsonString)
    {
        if (jsonString.IsValidJson())
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        else
        {
            return default(T);
        }
    }

    public static object? ToType(this string jsonString, Type type)
    {
        return JsonConvert.DeserializeObject(jsonString, type);
    }

    public static double? TryToDoubleElseZero(this string? inputString)
    {
        Double.TryParse(inputString, out var doubleResult);
        return doubleResult;
    }

    public static bool IsTrustedConnection(this string? connectionString)
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

        return builder.IntegratedSecurity;
    }

    public static string KeepAlphaNumeric(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 使用正则表达式匹配并保留字母和数字
        string cleaned = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9]", "");
        return cleaned;
    }

    public static string KeepAlpha(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 使用正则表达式匹配并保留字母
        string cleaned = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z]", "");
        return cleaned;
    }

    public static string ToMD5(this string input)
    {
        return SHA.SHAmd5Encrypt(input);
    }

    public static string ToWord(this IEnumerable<char> input)
    {
        return new string(input.ToArray());
    }
}