using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EgyMartAdminPortal.Handlers
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly LocalStorageService _localStorage;
        private readonly AuthService _authService;
        private const string TokenKey = "authToken";

        public JwtAuthorizationMessageHandler(LocalStorageService localStorageService, AuthService authService)
        {
            _localStorage = localStorageService;
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AttachAccessTokenAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // 401 detected, try to refresh token
                if (await _authService.TryRefreshTokenAsync())
                {
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    await AttachAccessTokenAsync(clonedRequest);
                    return await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    // Refresh failed, proceed with logout
                    Console.WriteLine("Refresh token failed,m proceeding with logout.", request);
                    await _authService.LogoutAsync();
                    return response;
                }
            }

            return response;
        }

        private async Task AttachAccessTokenAsync(HttpRequestMessage request)
        {
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            if (!string.IsNullOrEmpty(tokens?.Jwt))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Jwt);
            }
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