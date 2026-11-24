using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EgyMartAdminPortal.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace EgyMartAdminPortal.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalStorageService _localStorage;
        private readonly NavigationManager _navigation;
        private readonly AuthSession _session;
        private readonly JwtAuthStateProvider _authState;

        private const string ApiUrl = "auth/api/v2/Auth";
        private const string TokenKey = "authToken";
        private const string UserKey = "userData";

        public Person User => _session.User;

        public AuthService(
            IHttpClientFactory httpClientFactory,
            LocalStorageService localStorageService,
            NavigationManager navigation,
            AuthSession session,
            AuthenticationStateProvider authState)
        {
            _httpClientFactory = httpClientFactory;
            _localStorage = localStorageService;
            _navigation = navigation;
            _session = session;
            _authState = (JwtAuthStateProvider)authState;
        }

        public void SetUser(Person user) => _session.Set(_session.Jwt, _session.RefreshToken, user);

        public async Task<Person> GetUserAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            if (string.IsNullOrEmpty(_session.User.DisplayName))
            {
                var stored = await _localStorage.GetItemAsync<Person>(UserKey);
                if (stored != null && !string.IsNullOrEmpty(_session.Jwt))
                    _session.Set(_session.Jwt, _session.RefreshToken, stored);
            }

            return _session.User;
        }

        public Task<string?> GetAuthTokenAsync() => Task.FromResult(_session.Jwt);

        public async Task<bool> HasValidSessionAsync()
        {
            if (string.IsNullOrEmpty(_session.User.DisplayName))
            {
                var stored = await _localStorage.GetItemAsync<Person>(UserKey);
                if (stored != null)
                    _session.Set(_session.Jwt, _session.RefreshToken, stored);
            }
            var token = await GetAuthTokenAsync();

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(User.DisplayName))
                return false;

            if (!IsTokenExpired(token))
                return true;

            if (await TryRefreshTokenAsync())
            {
                var refreshed = await GetAuthTokenAsync();
                return !string.IsNullOrWhiteSpace(refreshed) && !IsTokenExpired(refreshed);
            }

            return false;
        }

        public static bool IsTokenExpired(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt)) return true;
            var parts = jwt.Split('.');
            if (parts.Length < 2) return true;

            try
            {
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                var bytes = Convert.FromBase64String(payload);
                using var doc = JsonDocument.Parse(bytes);
                if (!doc.RootElement.TryGetProperty("exp", out var expEl) || !expEl.TryGetInt64(out var exp))
                    return true;
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp;
            }
            catch
            {
                return true;
            }
        }

        public async Task<ApiResponse<LoginData>> LoginAsync(string userName, string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PlainClient");
                var requestBody = new { UserName = userName, Password = password, PortalType = 3 };
                var response = await client.PostAsJsonAsync($"{ApiUrl}/SupplierLogin", requestBody);

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginData>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    await SaveLoginDataAsync(apiResponse.Data);
                    _authState.Notify();
                }

                return apiResponse ?? FailureResponse<LoginData>("Invalid response from server", "استجابة غير صالحة من الخادم");
            }
            catch
            {
                return FailureResponse<LoginData>("An unexpected error occurred", "حدث خطأ غير متوقع");
            }
        }

        public async Task<bool> ChangePasswordAsync(long userId, string newPassword, string confirmPassword)
        {
            try
            {
                // Authenticated client: sends the bearer token (and refreshes it on expiry)
                var client = _httpClientFactory.CreateClient("SignedClient");
                var url = $"{ApiUrl}/AdminPassword/ChangePassword?UserID={userId}";
                var request = new { newPassword, confirmPassword };

                var response = await client.PutAsJsonAsync(url, request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryRefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(_session.Jwt) || string.IsNullOrEmpty(_session.RefreshToken))
                return false;

            try
            {
                var client = _httpClientFactory.CreateClient("PlainClient");
                var response = await client.PostAsJsonAsync($"{ApiUrl}/RefreshToken", new
                {
                    ExpiredToken = _session.Jwt,
                    RefreshToken = _session.RefreshToken
                });
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<NewToken>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result?.Data == null || string.IsNullOrEmpty(result.Data.NewJWT))
                    return false;

                var nextJwt = string.IsNullOrEmpty(result.Data.Jwt) ? result.Data.NewJWT : result.Data.Jwt;
                if (string.IsNullOrEmpty(nextJwt))
                    return false;

                var rotated = string.IsNullOrEmpty(result.Data.RefreshToken)
                    ? _session.RefreshToken
                    : result.Data.RefreshToken;
                _session.Set(nextJwt, rotated, _session.User);
                _authState.Notify();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            var jwt = _session.Jwt;
            var relative = "/" + _navigation.ToBaseRelativePath(_navigation.Uri).TrimStart('/');
            _session.Clear();
            _authState.Notify();

            if (!string.IsNullOrEmpty(jwt))
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("PlainClient");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                    await client.PostAsync($"{ApiUrl}/Logout", null);
                }
                catch
                {
                    // The browser session is already cleared. A failed revoke must not keep the user on the page.
                }
            }

            if (!relative.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
                await _localStorage.SetItemAsync("lastVisitedUrl", relative);

            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            await _localStorage.RemoveItemAsync("cachedLanguages");
            _navigation.NavigateTo("/login", forceLoad: true);
        }

        private async Task SaveLoginDataAsync(LoginData data)
        {
            _session.Set(data.Tokens.Jwt, data.Tokens.RefreshToken, data.User);
            await _localStorage.RemoveItemAsync(TokenKey);

            var user = data.User;
            var minimalUser = new
            {
                user.UserID,
                user.DisplayName,
                user.UserName,
                user.ProfileImage,
                user.IsActive,
                user.FirstLogin
            };
            await _localStorage.SetItemAsync(UserKey, minimalUser);
        }

        private static ApiResponse<T> FailureResponse<T>(string engMessage, string arMessage) where T : class
        {
            return new ApiResponse<T>
            {
                Success = false,
                ResponseEngMsg = engMessage,
                ResponseArMsg = arMessage
            };
        }
    }
}
