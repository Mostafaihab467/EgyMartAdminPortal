using EgyMartAdminPortal.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class AuthService(HttpClient httpClient, IJSRuntime jsRuntime, NavigationManager navigation)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly NavigationManager Navigation = navigation;
        private const string ApiUrl = "auth/api/v2/Auth";

        public Person User { get; private set; } = new Person();

        public void SetUser(Person user)
        {
            User = user;
        }

        public async Task<Person> GetUser()
        {
            var userDataJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userData");
            if (!string.IsNullOrEmpty(userDataJson))
            {
                User = JsonSerializer.Deserialize<Person>(userDataJson) ?? new Person();
            }
            if (User.DisplayName == null)
            {
                // Redirect to login page
                Navigation.NavigateTo("/login");
            }
            return User;
        }

        public async Task<string> GetToken()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ["authToken"]);
        }

        public async Task<ApiResponse<LoginData>> LoginAsync(string userName, string password)
        {
            try
            {
                var requestBody = new { UserName = userName, Password = password };

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/SupplierLogin", requestBody);

                if (response == null)
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", "No response from server");
                    return new ApiResponse<LoginData> { Success = false, ResponseEngMsg = "No response from server", ResponseArMsg = "لم يتم استلام رد من الخادم" };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginData>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse is not null && apiResponse.Success && apiResponse.Data is not null)
                {
                    var user = apiResponse.Data.User;
                    var jwt = apiResponse.Data.Tokens.Jwt;

                    User = user;

                    // Store selected user fields
                    var minimalUser = new
                    {
                        user.UserID,
                        user.DisplayName,
                        user.UserName,
                        user.ProfileImage,
                        user.IsActive,
                        user.FirstLogin
                    };

                    var userDataJson = JsonSerializer.Serialize(minimalUser);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userData", userDataJson);

                    // Store JWT token
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", jwt);
                }

                return apiResponse ?? new ApiResponse<LoginData>
                {
                    Success = false,
                    ResponseEngMsg = "Invalid response from server",
                    ResponseArMsg = "استجابة غير صالحة من الخادم"
                };
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Unexpected error: {ex.Message}");
                return new ApiResponse<LoginData>
                {
                    Success = false,
                    ResponseEngMsg = "An unexpected error occurred",
                    ResponseArMsg = "حدث خطأ غير متوقع"
                };
            }
        }

        public async Task<bool> ChangePasswordAsync(long userId, string newPassword, string confirmPassword)
        {
            var url = $"{ApiUrl}/AdminPassword/ChangePassword?UserID={userId}";

            var changePasswordRequest = new
            {
                newPassword = newPassword,
                confirmPassword = confirmPassword
            };

            var response = await _httpClient.PutAsJsonAsync(url, changePasswordRequest);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Handle error response or return false
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userData");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

            User = new Person();

            // Redirect to login page
            Navigation.NavigateTo("/login");
        }
    }
}
