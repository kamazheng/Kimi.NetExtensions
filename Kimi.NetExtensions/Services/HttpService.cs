using Newtonsoft.Json;
using System.Text;

public interface IHttpService
{
    Task<T> Get<T>(string uri, CancellationToken token = default);

    Task<T> Post<T>(string uri, object value, CancellationToken token = default);

    Task<object> Post(string uri, object value, Type? returnType = default, CancellationToken token = default);

    Task<T> Put<T>(string uri, object value, CancellationToken token = default);

    Task<object> Put(string uri, object value, Type returnType, CancellationToken token = default);

    Task<T> Delete<T>(string uri, object value, CancellationToken token = default);

    Task<object> Delete(string uri, object value, Type returnType, CancellationToken token = default);

    Task<T> Post<T>(string uri, HttpContent content, CancellationToken token = default);

    Task<object> Post(string uri, HttpContent content, CancellationToken token = default);
}

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// httpclient can inject with authentication header
    /// </summary>
    /// <param name="httpClient">
    /// </param>
    public HttpService(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        
    }

    public async Task<T> Get<T>(string uri, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        return await sendRequest<T>(request, token);
    }

    public async Task<T> Post<T>(string uri, HttpContent content, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = content;
        return await sendRequest<T>(request, token);
    }

    public async Task<object> Post(string uri, HttpContent content, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = content;
        return await sendRequest(request, null, token);
    }

    private StringContent CreateJsonContent(object value)
    {
        return new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
    }

    public async Task<T> Post<T>(string uri, object value, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = CreateJsonContent(value);
        return await sendRequest<T>(request, token);
    }

    public async Task<object> Post(string uri, object value, Type? returnType, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = CreateJsonContent(value);
        return await sendRequest(request, returnType, token);
    }

    public async Task<T> Put<T>(string uri, object value, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Content = CreateJsonContent(value);
        return await sendRequest<T>(request, token);
    }

    public async Task<object> Put(string uri, object value, Type returnType, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, uri);
        if (value.GetType() == typeof(MultipartFormDataContent))
        {
            request.Content = (MultipartFormDataContent)value;
        }
        else
        {
            request.Content = CreateJsonContent(value);
        }
        return await sendRequest(request, returnType, token);
    }

    public async Task<T> Delete<T>(string uri, object value, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        request.Content = CreateJsonContent(value);
        return await sendRequest<T>(request, token);
    }

    public async Task<object> Delete(string uri, object value, Type returnType, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        request.Content = CreateJsonContent(value);
        return await sendRequest(request, returnType, token);
    }

    // helper methods

    private async Task<T> sendRequest<T>(HttpRequestMessage request, CancellationToken token = default)
    {
        var result = await sendRequest(request, typeof(T), token);
        return (T)result;
    }

    /// <summary>
    /// </summary>
    /// <param name="request">
    /// </param>
    /// <param name="type">
    /// </param>
    /// <param name="token">
    /// </param>
    /// <returns>
    /// </returns>
    /// <exception cref="Exception">
    /// </exception>
    internal virtual async Task<object> sendRequest(HttpRequestMessage request, Type? type, CancellationToken token = default)
    {
        using var response = await _httpClient.SendAsync(request, token);
        var resString = await response.Content.ReadAsStringAsync(token);

        // throw exception on error response
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(JsonConvert.SerializeObject(new
            {
                ErrorCode = response.StatusCode,
                Reason = response.ReasonPhrase,
                Path = request.RequestUri,
                Message = resString,
            }));
        }

        if (type == typeof(File))
        {
            var file = await response.Content.ReadAsByteArrayAsync();
            return file;
        }
        else if (type == default || type == typeof(string))
        {
            return resString;
        }
        else
        {
            var ret = JsonConvert.DeserializeObject(resString, type);
            if (ret is not null) return ret;
            else return default!;
        }
    }
}