using Kimi.NetExtensions.Extensions;
using Kimi.NetExtensions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Newtonsoft.Json;

public static class ObjectExtensions
{
    public static JsonSerializerSettings jsonSetting = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects,
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
        return DisableLazyLoading<T>(obj, () => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, jsonSetting), jsonSetting));
    }

    public static T? JsonCopy<T>(this T? obj, int detpt)
    {
        return DisableLazyLoading<T>(obj, () => JsonConvert.DeserializeObject<T>(obj.ToJson(detpt), jsonSetting));
    }

    public static object? JsonCopy(this object? obj, Type type)
    {
        return DisableLazyLoading<object>(obj, () => JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, jsonSetting), type, jsonSetting));
    }

    public static object? JsonCopy(this object obj)
    {
        return DisableLazyLoading<object>(obj, () => JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, jsonSetting), obj.GetType()));
    }

    public static string ToJson(this object obj)
    {
        return DisableLazyLoading<string>(obj, () => JsonConvert.SerializeObject(obj, jsonSetting));
    }

    public static string ToJson(this object obj, JsonSerializerSettings serializerSettings)
    {
        return DisableLazyLoading<string>(obj, () => JsonConvert.SerializeObject(obj, serializerSettings));
    }

    public static string ToJson(this object obj, int maxDepth)
    {
        return DisableLazyLoading<string>(obj, () => ToJsonWithMaxDepth(obj, maxDepth));
    }

    private static string ToJsonWithMaxDepth(this object obj, int maxDepth)
    {
        using (var strWriter = new StringWriter())
        {
            using (var jsonWriter = new CustomJsonTextWriter(strWriter))
            {
                Func<bool> include = () => jsonWriter.CurrentDepth <= maxDepth;
                var resolver = new CustomContractResolver(include);
                var serializer = new JsonSerializer
                {
                    ContractResolver = resolver,
                    TypeNameHandling = TypeNameHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                serializer.Serialize(jsonWriter, obj);
            }
            return strWriter.ToString();
        }
    }

    public static T? FromJson<T>(this string jsonString)
    {
        return JsonConvert.DeserializeObject<T>(jsonString, jsonSetting);
    }

    public static T? JsonCopy<T>(this object? obj)
    {
        return DisableLazyLoading<T>(obj, () => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, jsonSetting), jsonSetting));
    }

    public static object GetPkValue(this object? obj, IUser? user = null)
    {
        var dbContext = obj.GetType().GetDbContextFromTableClassType(user);
        var pkName = dbContext!.GetKeyName(obj);
        var pkValue = obj.GetType().GetProperty(pkName!)?.GetValue(obj);
        return pkValue;
    }

    public static T DisableLazyLoading<T>(this object obj, Func<T> operation)
    {
        LazyLoader lazyLoader;
        var lazyLoaderProperty = obj?.GetPropertyValueByReflection("LazyLoader");
        if (lazyLoaderProperty != null)
        {
            lazyLoader = lazyLoaderProperty as LazyLoader;
            var context = lazyLoader?.GetPropertyValueByExpression("Context") as DbContext;
            bool originalLazyLoadingEnabled = false;
            if (context != null)
            {
                originalLazyLoadingEnabled = context.ChangeTracker.LazyLoadingEnabled;
            }
            context.ChangeTracker.LazyLoadingEnabled = false;
            try
            {
                return operation.Invoke();
            }
            finally
            {
                if (context != null)
                {
                    context.ChangeTracker.LazyLoadingEnabled = originalLazyLoadingEnabled;
                }
            }
        }
        else
        {
            return operation.Invoke();
        }
    }
}