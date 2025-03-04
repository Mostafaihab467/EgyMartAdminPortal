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
        private const string ApiUrl = "auth/api/v1/Auth";

        public UserData User { get; private set; } = new UserData();

        public void SetUser(UserData user)
        {
            User = user;
        }

        public async Task GetUser()
        {
            var userDataJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userData");
            if (!string.IsNullOrEmpty(userDataJson))
            {
                User = JsonSerializer.Deserialize<UserData>(userDataJson) ?? new UserData();
            }
            if (User.displayName == null)
            {
                // Redirect to login page
                Navigation.NavigateTo("/login");
            }
        }

        public async Task<string> GetToken()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ["authToken"]);
        }

        public async Task<ApiResponse<UserData>> LoginAsync(string userName, string password)
        {
            var requestBody = new { UserName = userName, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/SupplierLogin", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<UserData> { Success = false, ResponseEngMsg = "Login failed", ResponseArMsg = "فشل التسجيل" };
            }

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserData>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse is not null && apiResponse.Success && apiResponse.Data is not null)
            {
                User = apiResponse.Data;

                // Save user data and token in session storage
                var userDataJson = JsonSerializer.Serialize(User);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userData", userDataJson);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", User.IsVerfied);
            }

            return apiResponse!;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userData");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

            User = new UserData();

            // Redirect to login page
            Navigation.NavigateTo("/login");
        }
    }
}
