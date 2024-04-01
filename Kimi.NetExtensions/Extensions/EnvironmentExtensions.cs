using Microsoft.Extensions.Hosting;
using System.Security.Claims;

public static class EnvironmentExtension
{
    /// <summary>
    /// To check enviroments is debug or not. Need to SetEnvironmentVariable first.
    /// </summary>
    public static bool IsDebug
    {
        get
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
        }
    }

    public static void SetEnvironmentVariable(bool isDebug)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", isDebug ? Environments.Development : Environments.Production);
    }

    public static void SetEnvironment(string environmentName)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentName);
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