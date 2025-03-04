using EgyMartAdminPortal.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<object>?> TranslateAsync<T>(T item, string apiUrl) where T : class
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/Translate", item);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Raw API Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("API request failed!");
                    return new ApiResponse<object> { Success = false, ResponseEngMsg = "Translation API request failed." };
                }

                var result = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Translation Error: {ex.Message}");
                return new ApiResponse<object> { Success = false, ResponseEngMsg = "An error occurred during translation." };
            }
        }
    }

}
