using Kimi.NetExtensions.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kimi.NetExtensions.Services;

public class JwtByIUserAccessTokenProvider : IAccessTokenProvider
{
    private IHttpContextAccessor _httpContext;
    private IUser _user;

    public JwtByIUserAccessTokenProvider(IHttpContextAccessor httpContext, IUser user)
    {
        _httpContext = httpContext;
        _user = user;
    }

    /// <summary>
    /// </summary>
    /// <returns>
    /// </returns>
    public async ValueTask<AccessTokenResult> RequestAccessToken()
    {
        if (!string.IsNullOrEmpty(_user.UserName))
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, _user.UserName),
            };
            //Todo: Add roles to claims.
            var roles = _user.Roles ?? new List<string>();
            var permissions = _user.Permissions ?? new List<string>();
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ResourcePermission.PolicyTypeName, permission));
            }

            var jwtPara = AuthService.GetTokenValidationParameters();
            var jwtToken = new JwtSecurityToken(
                issuer: jwtPara.ValidIssuer,
                audience: jwtPara.ValidIssuer,
                claims: claims,
                signingCredentials: new SigningCredentials(jwtPara.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
                expires: DateTime.UtcNow.AddMinutes(int.Parse(ConfigReader.Configuration["JWT:TimeValid"]!))
            );
            var token = await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwtToken));
            _user.JWT = token;
            var accessTokenResult = new AccessTokenResult(AccessTokenResultStatus.Success, new AccessToken { Value = token }, "", null);
            return accessTokenResult;
        }
        else
        {
            return default!;
        }
    }

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        return RequestAccessToken();
    }
}