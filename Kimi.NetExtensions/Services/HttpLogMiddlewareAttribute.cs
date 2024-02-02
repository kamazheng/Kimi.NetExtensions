using Microsoft.AspNetCore.Builder;

/// <summary>
/// Use as Middleware attribute on controller:
/// [MiddlewareFilter(typeof(HttpLogMiddlewareAttribute))]
/// </summary>
public class HttpLogMiddlewareAttribute
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<HttpLogHandlerMiddleware>();
    }
}