using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http;

/// <summary> 该函数是一个中间件类，用于在特定URL路径下进行身份验证。
/// 如果用户未经过身份验证或未经授权，则使用Negotiate认证方案进行身份验证。如果验证失败，则要求用户重新进行身份验证。通过后继续执行后续的中间件或请求处理程序。 Server side:
/// This is middleware to challenge Negotiate authentication, for example, for the hangfire
/// dashboard. app.UseMiddleware<ChallengeNegotiateUser>($"/{AppConstant.HangfirePath}"); </summary>
public class ChallengeNegotiateUser
{
    private readonly RequestDelegate _next;
    private readonly string _url;

    public ChallengeNegotiateUser(RequestDelegate next, string url = "")
    {
        _next = next;
        _url = url;
        LicenceHelper.CheckLicense();
    }

    public async Task Invoke(HttpContext context)
    {
        var url = context.Request.Path;
        if (url.StartsWithSegments(_url) || string.IsNullOrEmpty(url))
        {
            var user = context.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                var result = await context.AuthenticateAsync(NegotiateDefaults.AuthenticationScheme);
                if (result.Succeeded)
                {
                    context.User = result.Principal;
                }
                else
                {
                    await context.ChallengeAsync(NegotiateDefaults.AuthenticationScheme);
                    return;
                }
            }
        }
        await _next(context);
    }
}