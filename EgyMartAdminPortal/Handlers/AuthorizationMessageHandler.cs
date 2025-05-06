using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EgyMartAdminPortal.Handlers
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly LocalStorageService _localStorage;
        private readonly AuthService _authService;
        private readonly NavigationManager _navigation;
        private const string TokenKey = "authToken";
        private const string LastVisitedUrlKey = "lastVisitedUrl"; // Key to store last visited URL

        public JwtAuthorizationMessageHandler(LocalStorageService localStorageService, AuthService authService, NavigationManager navigationManager)
        {
            _localStorage = localStorageService;
            _authService = authService;
            _navigation = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AttachAccessTokenAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // 401 detected
                await HandleUnauthorizedAsync();
                return response; // or throw exception if you want
            }

            if (await IsJwtExpiredAsync())
            {
                if (await _authService.TryRefreshTokenAsync())
                {
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    await AttachAccessTokenAsync(clonedRequest);

                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    await HandleUnauthorizedAsync();
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

        private async Task<bool> IsJwtExpiredAsync()
        {
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            return tokens == null || string.IsNullOrEmpty(tokens.Jwt) || IsJwtExpired(tokens.Jwt);
        }

        private bool IsJwtExpired(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
                return true;

            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return true;

            try
            {
                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("exp", out var expProperty))
                    return true;

                var expSeconds = expProperty.GetInt64();
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

                return expDateTime <= DateTime.UtcNow;
            }
            catch
            {
                // any error => consider expired
                return true;
            }
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
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

        private async Task HandleUnauthorizedAsync()
        {
            // Save the current URL before redirecting to the login page
            var currentUrl = _navigation.Uri;
            if (!currentUrl.Contains("login"))
            {
                await _localStorage.SetItemAsync(LastVisitedUrlKey, currentUrl);
            }

            // Clear tokens and logout
            await _authService.LogoutAsync();

            // Redirect to login page
            _navigation.NavigateTo("/login", true);
        }
    }
}
