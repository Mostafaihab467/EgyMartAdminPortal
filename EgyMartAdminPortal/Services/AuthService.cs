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

        public async Task<UserData> GetUser()
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
            return User;
        }

        public async Task<string> GetToken()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ["authToken"]);
        }

        public async Task<ApiResponse<UserData>> LoginAsync(string userName, string password)
        {
            try
            {
                var requestBody = new { UserName = userName, Password = password };
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Timeout after 10 seconds

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/SupplierLogin", requestBody, cts.Token);

                if (response == null)
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", "No response from server");
                    return new ApiResponse<UserData> { Success = false, ResponseEngMsg = "No response from server", ResponseArMsg = "لم يتم استلام رد من الخادم" };
                }

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    await _jsRuntime.InvokeVoidAsync("console.warn", $"Login failed: {response.StatusCode} - {errorMsg}");

                    return new ApiResponse<UserData>
                    {
                        Success = false,
                        ResponseEngMsg = "Login failed, Invalid Username or Password",
                        ResponseArMsg = "فشل التسجيل, اسم المستخدم او كلمة المرور خطأ"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserData>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse is not null && apiResponse.Success && apiResponse.Data is not null)
                {
                    User = apiResponse.Data;

                    // Save user data and token
                    var userDataJson = JsonSerializer.Serialize(User);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userData", userDataJson);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", User.IsVerfied);
                }

                return apiResponse ?? new ApiResponse<UserData>
                {
                    Success = false,
                    ResponseEngMsg = "Invalid response from server",
                    ResponseArMsg = "استجابة غير صالحة من الخادم"
                };
            }
            catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("SSL") == true)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", "SSL Error: Invalid certificate or SSL misconfiguration.");
                return new ApiResponse<UserData>
                {
                    Success = false,
                    ResponseEngMsg = "SSL error: Invalid certificate",
                    ResponseArMsg = "خطأ في شهادة الأمان SSL"
                };
            }
            catch (HttpRequestException ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"HTTP Request Error: {ex.Message}");
                return new ApiResponse<UserData>
                {
                    Success = false,
                    ResponseEngMsg = "Network error, please try again",
                    ResponseArMsg = "خطأ في الشبكة، الرجاء المحاولة مرة أخرى"
                };
            }
            catch (TaskCanceledException)
            {
                await _jsRuntime.InvokeVoidAsync("console.warn", "Request timed out.");
                return new ApiResponse<UserData>
                {
                    Success = false,
                    ResponseEngMsg = "Request timed out",
                    ResponseArMsg = "انتهت مهلة الطلب"
                };
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Unexpected error: {ex.Message}");
                return new ApiResponse<UserData>
                {
                    Success = false,
                    ResponseEngMsg = "An unexpected error occurred",
                    ResponseArMsg = "حدث خطأ غير متوقع"
                };
            }
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
