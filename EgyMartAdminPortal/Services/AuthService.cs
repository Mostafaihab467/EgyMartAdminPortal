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

        public async Task<ApiResponse<Person>> LoginAsync(string userName, string password)
        {
            try
            {
                var requestBody = new { UserName = userName, Password = password };

                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/SupplierLogin", requestBody);

                if (response == null)
                {
                    await _jsRuntime.InvokeVoidAsync("console.error", "No response from server");
                    return new ApiResponse<Person> { Success = false, ResponseEngMsg = "No response from server", ResponseArMsg = "لم يتم استلام رد من الخادم" };
                }

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    await _jsRuntime.InvokeVoidAsync("console.warn", $"Login failed: {response.StatusCode} - {errorMsg}");

                    return new ApiResponse<Person>
                    {
                        Success = false,
                        ResponseEngMsg = "Login failed, Invalid Username or Password",
                        ResponseArMsg = "فشل التسجيل, اسم المستخدم او كلمة المرور خطأ"
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<Person>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse is not null && apiResponse.Success && apiResponse.Data is not null)
                {
                    User = apiResponse.Data;

                    var minimalUser = new
                    {
                        User.UserID,
                        User.DisplayName,
                        User.UserName,
                        User.ProfileImage,
                        User.IsActive,
                        User.FirstLogin
                    };

                    var userDataJson = JsonSerializer.Serialize(minimalUser);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userData", userDataJson);

                    // Save JWT token (replace IsVerfied with actual JWT property if needed)
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", apiResponse.Data.IsVerfied);
                }

                return apiResponse ?? new ApiResponse<Person>
                {
                    Success = false,
                    ResponseEngMsg = "Invalid response from server",
                    ResponseArMsg = "استجابة غير صالحة من الخادم"
                };
            }
            catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("SSL") == true)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", "SSL Error: Invalid certificate or SSL misconfiguration.");
                return new ApiResponse<Person>
                {
                    Success = false,
                    ResponseEngMsg = "SSL error: Invalid certificate",
                    ResponseArMsg = "خطأ في شهادة الأمان SSL"
                };
            }
            catch (HttpRequestException ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"HTTP Request Error: {ex.Message}");
                return new ApiResponse<Person>
                {
                    Success = false,
                    ResponseEngMsg = "Network error, please try again",
                    ResponseArMsg = "خطأ في الشبكة، الرجاء المحاولة مرة أخرى"
                };
            }
            catch (TaskCanceledException)
            {
                await _jsRuntime.InvokeVoidAsync("console.warn", "Request timed out.");
                return new ApiResponse<Person>
                {
                    Success = false,
                    ResponseEngMsg = "Request timed out",
                    ResponseArMsg = "انتهت مهلة الطلب"
                };
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Unexpected error: {ex.Message}");
                return new ApiResponse<Person>
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
