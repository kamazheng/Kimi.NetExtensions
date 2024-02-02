using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

/// <summary>
/// Implememtn
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAccessTokenProvider _tokenProvider;

    public JwtAuthStateProvider(IAccessTokenProvider tokenProvider)
    {
        LicenceHelper.CheckLicense();
        _tokenProvider = tokenProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenResult = await _tokenProvider.RequestAccessToken();
            if (tokenResult != null
                && tokenResult.TryGetToken(out AccessToken accessToken)
                && (!string.IsNullOrEmpty(accessToken.Value) && handler.CanReadToken(accessToken.Value))
                )
            {
                var token = handler.ReadJwtToken(accessToken.Value);
                var expiration = token.ValidTo;
                if (DateTime.UtcNow < expiration)
                {
                    identity = new ClaimsIdentity(token.Claims, "Server authentication");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request failed:" + ex.ToString());
        }
        var state = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return state;
    }
}