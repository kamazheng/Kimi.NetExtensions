using Kimi.NetExtensions.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Kimi.NetExtensions.Services;

public static class OpenId
{
    /// <summary>
    /// Adds OpenID authentication to the web application. Will scope inject the
    /// OpenIdAuthStateProvider as AuthenticationStateProvider.
    /// Configration: OIDC:ClientId, OIDC:ClientSecret, OIDC:Authority, OIDC:MetadataAddress,
    /// OIDC:CallbackPath - /signin-oidc
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="scopes">"openid", "profile", etc.</param>
    public static void AddOpenIdAuthentication(this WebApplicationBuilder builder, params string[] scopes)
    {
        AddOpenIdService(builder, scopes);

        builder.Services.AddScoped<AuthenticationStateProvider, OpenIdAuthStateProvider>();
    }

    /// <summary>
    /// Adds OpenID authentication to the web application. NOT inject the OpenIdAuthStateProvider as AuthenticationStateProvider.
    /// Configration: OIDC:ClientId, OIDC:ClientSecret, OIDC:Authority, OIDC:MetadataAddress,
    /// OIDC:CallbackPath - /signin-oidc
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="scopes">"openid", "profile", etc.</param>
    private static void AddOpenIdService(this WebApplicationBuilder builder, string[] scopes)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect(options =>
        {
            options.ClientId = builder.Configuration["OIDC:ClientId"];
            options.ClientSecret = builder.Configuration["OIDC:ClientSecret"];
            options.Authority = builder.Configuration["OIDC:Authority"];
            options.MetadataAddress = builder.Configuration["OIDC:MetadataAddress"];
            options.CallbackPath = builder.Configuration["OIDC:CallbackPath"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            foreach (var scope in scopes.OrEmptyIfNull())
            {
                options.Scope.Add(scope);
            }
        });
    }
}