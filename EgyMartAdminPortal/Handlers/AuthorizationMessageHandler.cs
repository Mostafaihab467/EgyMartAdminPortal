using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EgyMartAdminPortal.Handlers
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        // Only one token refresh at a time across parallel requests
        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        private readonly LocalStorageService _localStorage;
        private readonly AuthService _authService;
        private readonly Uri? _apiBase;
        private const string TokenKey = "authToken";

        public JwtAuthorizationMessageHandler(LocalStorageService localStorageService, AuthService authService, IConfiguration configuration)
        {
            _localStorage = localStorageService;
            _authService = authService;
            var apiUrl = configuration["ApiUrl"];
            _apiBase = string.IsNullOrWhiteSpace(apiUrl) ? null : new Uri(apiUrl);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Never send the bearer token to hosts other than the API
            if (!IsApiRequest(request))
                return await base.SendAsync(request, cancellationToken);

            // Buffer the body so the request can be re-sent after a token refresh
            if (request.Content != null)
                await request.Content.LoadIntoBufferAsync();

            var sentJwt = await AttachAccessTokenAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
    response.Headers.TryGetValues("Token-Expired", out var tokenExpiredValues) &&
    tokenExpiredValues.Contains("true", StringComparer.OrdinalIgnoreCase))
            {
                // 401 + Token-Expired = true → try to refresh token (one refresh at a time)
                bool refreshed;
                await _refreshLock.WaitAsync(cancellationToken);
                try
                {
                    // If another request already refreshed the token, just reuse it
                    var current = await _localStorage.GetItemAsync<Tokens>(TokenKey);
                    refreshed = (!string.IsNullOrEmpty(current?.Jwt) && current.Jwt != sentJwt)
                                || await _authService.TryRefreshTokenAsync();
                }
                finally
                {
                    _refreshLock.Release();
                }

                if (refreshed)
                {
                    response.Dispose();
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    await AttachAccessTokenAsync(clonedRequest);
                    return await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    await _authService.LogoutAsync();
                    return response;
                }
            }


            return response;
        }

        private bool IsApiRequest(HttpRequestMessage request) =>
            _apiBase is null ||
            (request.RequestUri is { } uri &&
             Uri.Compare(uri, _apiBase, UriComponents.SchemeAndServer, UriFormat.Unescaped,
                         StringComparison.OrdinalIgnoreCase) == 0);

        private async Task<string?> AttachAccessTokenAsync(HttpRequestMessage request)
        {
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            if (!string.IsNullOrEmpty(tokens?.Jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Jwt);
            }
            return tokens?.Jwt;
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}