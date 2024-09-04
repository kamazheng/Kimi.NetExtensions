using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Kimi.NetExtensions.Services;

public class OpenIdAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public OpenIdAuthStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_httpContextAccessor?.HttpContext == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        var user = _httpContextAccessor.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            await _httpContextAccessor.HttpContext!.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        else
        {
            var state = new AuthenticationState(new ClaimsPrincipal(user));
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }
    }
}