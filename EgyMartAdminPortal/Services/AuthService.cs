using EgyMartAdminPortal.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalStorageService _localStorage;
        private readonly NavigationManager _navigation;

        private const string ApiUrl = "auth/api/v2/Auth";
        private const string TokenKey = "authToken";
        private const string UserKey = "userData";

        public Person User { get; private set; } = new Person();

        public AuthService(IHttpClientFactory httpClientFactory, LocalStorageService localStorageService, NavigationManager navigation)
        {
            _httpClientFactory = httpClientFactory;
            _localStorage = localStorageService;
            _navigation = navigation;
        }

        public void SetUser(Person user) => User = user;

        public async Task<Person> GetUserAsync()
        {
            var user = await _localStorage.GetItemAsync<Person>(UserKey);
            User = user ?? new Person();

            if (string.IsNullOrEmpty(User.DisplayName))
            {
                var token = await GetAuthTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _navigation.NavigateTo("/login");
                }
            }

            return User;
        }

        public async Task<string?> GetAuthTokenAsync()
        {
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            return tokens?.Jwt;
        }

        public async Task<bool> HasValidSessionAsync()
        {
            var user = await _localStorage.GetItemAsync<Person>(UserKey);
            User = user ?? new Person();
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

                if (response == null)
                    return FailureResponse<LoginData>("No response from server", "لم يتم استلام رد من الخادم");

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginData>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    await SaveLoginDataAsync(apiResponse.Data);
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
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            if (tokens == null || string.IsNullOrEmpty(tokens.Jwt) || string.IsNullOrEmpty(tokens.RefreshToken))
                return false;

            var refreshUri = $"{ApiUrl}/RefreshToken";

            try
            {
                var client = _httpClientFactory.CreateClient("PlainClient");
                var response = await client.PostAsJsonAsync(refreshUri, new
                {
                    ExpiredToken = tokens.Jwt,
                    RefreshToken = tokens.RefreshToken
                });
                if (!response.IsSuccessStatusCode)
                    return false;
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<NewToken>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || result.Data == null)
                    return false;

                var nextJwt = string.IsNullOrEmpty(result.Data.Jwt) ? result.Data.NewJWT : result.Data.Jwt;
                if (string.IsNullOrEmpty(nextJwt))
                    return false;

                var updatedTokens = new Tokens
                {
                    Jwt = nextJwt,
                    RefreshToken = string.IsNullOrEmpty(result.Data.RefreshToken)
                        ? tokens.RefreshToken
                        : result.Data.RefreshToken
                };

                await _localStorage.SetItemAsync(TokenKey, updatedTokens);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var token = await GetAuthTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var client = _httpClientFactory.CreateClient("SignedClient");
                    await client.PostAsync($"{ApiUrl}/Logout", null);
                }
            }
            catch
            {
                // Best-effort server revocation; always clear local session
            }

            var relative = "/" + _navigation.ToBaseRelativePath(_navigation.Uri).TrimStart('/');
            if (!relative.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
            {
                await _localStorage.SetItemAsync("lastVisitedUrl", relative);
            }
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            await _localStorage.RemoveItemAsync("cachedLanguages");
            User = new Person();
            _navigation.NavigateTo("/login", forceLoad: true);
        }

        private async Task SaveLoginDataAsync(LoginData data)
        {
            var user = data.User;
            User = user;

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

            var tokens = new Tokens
            {
                Jwt = data.Tokens.Jwt,
                RefreshToken = data.Tokens.RefreshToken
            };

            await _localStorage.SetItemAsync(TokenKey, tokens);
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
