using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Kimi.NetExtensions.Services;

public static class LocalFileServer
{
    /// <summary>
    /// Creates a local file server for serving static files.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <param name="filePath">
    /// The relative path to the directory containing the files. Default is "Files".
    /// </param>
    public static void CreateLocalFileServer(this WebApplication app, string filePath = "Files")
    {
        var env = app.Services.GetRequiredService<IWebHostEnvironment>();
        var filesPath = Path.Combine(env.ContentRootPath, filePath);
        // Ensure the directory exists
        Directory.CreateDirectory(filesPath);

        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(filesPath),
            RequestPath = $"/{filePath.ToLower()}",
            EnableDirectoryBrowsing = true
        });
    }
}