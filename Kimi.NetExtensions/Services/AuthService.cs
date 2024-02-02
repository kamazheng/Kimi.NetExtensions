using Kimi.NetExtensions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class AuthService
{
    static AuthService() => LicenceHelper.CheckLicense();

    /// <summary>
    /// To Add JwtBearer and Windows Negotiation Authetication for Server side project. Client side
    /// need to use the customized authentication provider
    /// </summary>
    /// <param name="services">
    /// </param>
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication((options) =>
        {
            options.DefaultAuthenticateScheme = "JwtBearer";
            options.DefaultChallengeScheme = "JwtBearer";
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = GetTokenValidationParameters();
        }).AddNegotiate();

        services.AddAuthorization(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        }); //jwt default for [authorize] attrubute

        services.AddScoped<IAccessTokenProvider, JwtByIUserAccessTokenProvider>();
        services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
    }

    /// <summary>
    /// Get the JWT token setting, JWT:Issuer for ValidIssuer and ValidAudience and JWT:Key requrired.
    /// </summary>
    /// <returns>
    /// </returns>
    public static TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = ConfigReader.Configuration["JWT:Issuer"],
            ValidAudience = ConfigReader.Configuration["JWT:Issuer"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ConfigReader.Configuration["JWT:Key"]!)),
            LifetimeValidator = CustomLifetimeValidator
        };
    }

    private static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
    {
        if (expires != null)
        {
            return expires > DateTime.UtcNow;
        }
        return false;
    }
}