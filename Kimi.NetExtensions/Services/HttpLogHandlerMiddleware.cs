using Microsoft.AspNetCore.Http;
using System.Text;

/// <summary> Middleware to log http request, HttpLog table must created. Not log the http file
/// form-data. Don't use HttpLogMiddlewareAttribute to add on to controllers for which need to log,
/// it will cause problem as the dbcontext is static inject. Need to double check. Usage:
/// services.AddScoped<HttpLogHandlerMiddleware>(); services.AddScoped<IHttpLog, HttpLog>();
/// services.AddScoped<IHttpLogSetting, ?>(); To filter only some of the request, test if you can
/// use parameterless of middleware. app.MapWhen(context =>
/// context.Request.Headers.ContainsKey("X-HttpLog"), appBuilder => { app.UseMiddleware<HttpLogHandlerMiddleware>();
///
/// </summary>

//https://stackoverflow.com/questions/43403941/how-to-read-asp-net-core-response-body
public class HttpLogHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private IHttpLog? _httpLog;

    public HttpLogHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IHttpLog httpLog)
    {
        _httpLog = httpLog;
        try
        {
            //Cache the request body
            if (context?.Request?.ContentType?.StartsWith("multipart/form-data") != true)
            {
                // Enable request body buffering
                context!.Request.EnableBuffering();

                // Read the request body and store it in a variable
                var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);

                // Store the request body in the HttpContext.Items dictionary
                context.Items["RequestBody"] = requestBody;

                // Rewind the request body stream
                context.Request.Body.Position = 0;
            }
            else
            {
                context.Items["RequestBody"] = "File not cached";
            }

            Stream originalBody = context.Response.Body;

            try
            {
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;
                    await _next(context);
                    memStream.Position = 0;
                    string responseBody = new StreamReader(memStream).ReadToEnd();
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                    await LogHttp(context, responseBody);
                }
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
        catch (Exception error) //Record the exception for _next calling.
        {
            // Get the stored request body from the HttpContext.Items dictionary
            _httpLog.RequestPath = context.Request.Path;
            _httpLog.ErrorMessage = error.ToString();
            _httpLog.ExceptionType = error.GetType()?.FullName ?? error.GetType().Name;
            _httpLog.User = context?.User?.Identity?.Name;
            _httpLog.Updated = DateTime.UtcNow;
            _httpLog.Updatedby = nameof(HttpLogHandlerMiddleware);
            _httpLog.Active = true;
            if (context?.Items?.ContainsKey("RequestBody") == true)
            {
                _httpLog.RequestBody = context.Items["RequestBody"] as string;
            }
            await _httpLog.SaveAsync();

            // Rethrow the exception to propagate it up the call stack
            throw;
        }
    }

    private async Task LogHttp(HttpContext context, string responseBody)
    {
        if (await _httpLog.IsLogOnAsync())
        {
            _httpLog.RequestPath = context.Request.Path;
            _httpLog.User = context?.User?.Identity?.Name;
            _httpLog.ResponseBody = responseBody;
            _httpLog.Updated = DateTime.UtcNow;
            _httpLog.Updatedby = nameof(HttpLogHandlerMiddleware);
            _httpLog.Active = true;
            if (context?.Items?.ContainsKey("RequestBody") == true)
            {
                _httpLog.RequestBody = context.Items["RequestBody"] as string;
            }
            await _httpLog.SaveAsync();
        }
    }
}