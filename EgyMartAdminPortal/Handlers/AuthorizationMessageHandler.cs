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
                    await _authService.LogoutAsync();
                    return response;
                }
            }

            if (response.Headers.TryGetValues("Token-Expired", out var tokenExpiredValues) &&
                tokenExpiredValues.Any(value => value.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                if (await _authService.TryRefreshTokenAsync())
                {
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

            if (await IsJwtExpiredAsync())
            {
                if (await _authService.TryRefreshTokenAsync())
                {
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    await AttachAccessTokenAsync(clonedRequest);
                    return await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    await _authService.LogoutAsync();
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
    }
}