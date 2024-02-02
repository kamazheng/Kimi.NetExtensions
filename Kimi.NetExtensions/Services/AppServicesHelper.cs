using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// AppServicesHelper, Save the Service as static object. in Program.cs: AppServicesHelper.Services
/// = host.Services;
/// </summary>
public class AppServicesHelper
{
    private IHttpContextAccessor ContextAccessor { get; }

    public AppServicesHelper(IHttpContextAccessor httpContextAccessor)
    {
        ContextAccessor = httpContextAccessor;
    }

    private static IServiceProvider? services;

    /// <summary>
    /// Provides static access to the framework's services provider
    /// </summary>
    public static IServiceProvider? Services
    {
        get { return services; }
        set
        {
            if (services != null)
            {
                throw new Exception("Can't set once a value has already been set.");
            }
            services = value;
        }
    }

    public static AuthenticationStateProvider? AuthenticationStateProvider => services?.GetService(typeof(AuthenticationStateProvider)) as AuthenticationStateProvider;
    public static IConfiguration? Configuration => services?.GetService(typeof(IConfiguration)) as IConfiguration;
    public static IHttpContextAccessor? HttpContextAccessor => services?.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;

    public static T? GetService<T>() where T : class => services?.GetService(typeof(T)) as T;

    public static T? GetRequiredService<T>() where T : class => services?.GetRequiredService(typeof(T)) as T;

    public static T? GetNewScopeService<T>() where T : class
    {
        using (var serviceScope = services!.CreateScope())
        {
            var getService = serviceScope.ServiceProvider.GetService<T>();
            return getService;
        }
    }
}