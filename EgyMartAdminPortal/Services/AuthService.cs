using EgyMartAdminPortal.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
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
                _navigation.NavigateTo("/login");

            return User;
        }

        public async Task<string?> GetAuthTokenAsync()
        {
            var tokens = await _localStorage.GetItemAsync<Tokens>(TokenKey);
            return tokens?.Jwt;
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
                var client = _httpClientFactory.CreateClient("PlainClient");
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

            var refreshUri = $"{ApiUrl}/RefreshToken?ExpiredToken={Uri.EscapeDataString(tokens.Jwt)}&RefreshToken={Uri.EscapeDataString(tokens.RefreshToken)}";

            try
            {
                var client = _httpClientFactory.CreateClient("PlainClient");
                var response = await client.PostAsync(refreshUri, null);
                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync();
                var newTokens = JsonSerializer.Deserialize<Tokens>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (newTokens == null || string.IsNullOrEmpty(newTokens.Jwt))
                    return false;

                await _localStorage.SetItemAsync(TokenKey, newTokens);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            //await _localStorage.ClearAsync();
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            await _localStorage.RemoveItemAsync("cachedLanguages");
            User = new Person();
            _navigation.NavigateTo("/login");
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
