using Kimi.NetExtensions.Interfaces;
using Newtonsoft.Json;

public static class ObjectExtensions
{
    public static JsonSerializerSettings jsonSetting = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None
    };

    static ObjectExtensions()
    {
        LicenceHelper.CheckLicense();
    }

    public static bool IsPrimaryKeyDefault(this object? obj)
    {
        if (obj == null) return true;
        if (obj == default) return true;
        if (obj?.ToString() == "") return true;
        if (obj?.ToString() == "0") return true;
        if (obj?.ToString() == "00000000-0000-0000-0000-000000000000") return true; //Guid
        return false;
    }

    public static bool IsJsonEqual(this object? obj1, object? obj2)
    {
        return JsonConvert.SerializeObject(obj1) == JsonConvert.SerializeObject(obj2, jsonSetting);
    }

    public static T? JsonCopy<T>(this T? obj)
    {
        var js = JsonConvert.SerializeObject(obj, jsonSetting);
        return JsonConvert.DeserializeObject<T>(js, jsonSetting);
    }

    public static object? JsonCopy(this object? obj, Type type)
    {
        return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, jsonSetting), type, jsonSetting);
    }

    public static object? JsonCopy(this object obj)
    {
        return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, jsonSetting), obj.GetType());
    }

    public static string ToJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, jsonSetting);
    }
    public static T? JsonCopy<T>(this object? obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, jsonSetting), jsonSetting);
    }

    public static object GetPkValue(this object? obj, IUser? user = null)
    {
        var dbContext = obj.GetType().GetDbContextFromTableClassType(user);
        var pkName = dbContext!.GetKeyName(obj);
        var pkValue = obj.GetType().GetProperty(pkName!)?.GetValue(obj);
        return pkValue;
    }
}