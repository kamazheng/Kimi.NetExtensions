using Newtonsoft.Json;

namespace Kimi.NetExtensions.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T?> ToNullableObjecct<T>(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var stringContent = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)stringContent;
                }
                return JsonConvert.DeserializeObject<T>(stringContent);
            }
            catch (JsonException)
            {
                // Handle JSON deserialization error
                throw new HttpRequestException($"The response content could not be deserialized into {typeof(T)}.");
            }
        }
        else
        {
            // Handle non-success status code
            throw new HttpRequestException(stringContent);
        }
    }
    public static async Task<T> ToObject<T>(this HttpResponseMessage response)
    {
        var result = await response.ToNullableObjecct<T>();
        if (result == null)
        {
            throw new HttpRequestException($"The response content could not be deserialized into {typeof(T)}.");
        }
        else
        {
            return result;
        }
    }
}