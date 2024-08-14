using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Kimi.NetExtensions.Services;

/// <summary>
/// Middleware for URL rewriting. param name="fileExtensions" The list of file extensions to check.
/// param name="newParaUrl" The new URL format with a placeholder for the request path.
/// </summary>
public class UrlRewriteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _fileExtensions;
    private readonly string _newParaUrl;
    private readonly string _logFilePath = "urlLog.txt"; // Path to the log file

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRewriteMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="fileExtensions">The list of file extensions to check.</param>
    /// <param name="newParaUrl">The new URL format with a placeholder for the request path.</param>
    public UrlRewriteMiddleware(RequestDelegate next, List<string> fileExtensions, string newParaUrl)
    {
        _next = next;
        _fileExtensions = fileExtensions;
        _newParaUrl = newParaUrl;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var fullUrl = context.Request.GetDisplayUrl();
        LogToFile($"Full URL: {fullUrl}");

        if (IsFileExtensionInList(fullUrl))
        {
            var newUrl = string.Format(_newParaUrl, fullUrl);
            LogToFile($"New URL: {newUrl}");
            context.Response.Redirect(newUrl, true);
            return;
        }
        await _next(context);
    }

    private bool IsFileExtensionInList(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        foreach (var extension in _fileExtensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private void LogToFile(string message)
    {
        using (var writer = new StreamWriter(_logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}