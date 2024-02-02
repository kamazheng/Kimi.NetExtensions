using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace Kimi.NetExtensions.Services
{
    public class JwtHttpService : HttpService
    {
        private IAccessTokenProvider _accessTokenProvider;

        public JwtHttpService(HttpClient httpClient, IAccessTokenProvider accessTokenProvider) : base(httpClient)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        internal override async Task<object> sendRequest(HttpRequestMessage request, Type? type, CancellationToken token = default)
        {
            var tokenResult = await _accessTokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var jwtToken))
            {
                if (!string.IsNullOrEmpty(jwtToken?.Value))
                {
                    string accessToken = jwtToken.Value;
                    // Use the access token...
                    request!.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            return base.sendRequest(request, type, token);
        }
    }
}