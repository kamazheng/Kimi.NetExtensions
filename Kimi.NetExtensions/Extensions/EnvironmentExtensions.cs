using Microsoft.Extensions.Hosting;
using System.Security.Claims;

public static class EnvironmentExtension
{
    public static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }

    public static void SetEnvironmentVariable()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
#else
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Production);
#endif
    }

    /// <summary>
    /// 这个C#函数获取当前用户的ClaimsPrincipal对象。它首先尝试从AppServicesHelper的HttpContextAccessor属性中获取HttpContext对象，然后从该对象中获取User属性（即当前用户）。 如果无法获取到当前用户，则创建一个新的ClaimsPrincipal对象，其中包含一个表示当前线程用户Environment.UserName的ClaimsIdentity对象，该对象包含一个表示用户名的Claim对象。
    /// </summary>
    /// <returns>
    /// </returns>
    public static ClaimsPrincipal CurrentUser =>
        AppServicesHelper.HttpContextAccessor?.HttpContext?.User ??
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, Environment.UserName) }));

    public static string LoginUserName =>
         CurrentUser.Identity!.Name?.Split("\\")?.Last() ?? "Anonymous";

    public static string CompanyDomain =>
     CurrentUser.Identity!.Name?.Split("\\")?.First() ?? "Anonymous";

    public static string? ClientIp =>
        AppServicesHelper.HttpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "NA";

    public static string? ClientHost =>
        AppServicesHelper.HttpContextAccessor?.HttpContext?.Request?.Host.Host ?? "NA";
}