using EgyMartAdminPortal.Models;
using System;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class AttributeService(HttpClient httpClient, AuthService authService)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly AuthService authService = authService;
        protected string ApiUrl = "products/api/Attribute";
        public async Task<List<Attributes>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Attributes>>>($"{ApiUrl}/GetList");

                if (response == null || response.Data == null)
                    return [];

                return response.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("API returned 404 - Resource not found.");
                return []; // Return an empty list instead of throwing an error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return []; // Handle other errors gracefully
            }
        }

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }
        
        public async Task<ApiResponse<int>> CreateAsync(Attributes newAttribute)
        {
            var user = await authService.GetUser();
            newAttribute.RcBy = user.UserID;

            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", newAttribute);

            if (!response.IsSuccessStatusCode)
                return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }
        
        public async Task<ApiResponse<int>> EditAsync(Attributes updatedAttribute)
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", updatedAttribute);

            if (!response.IsSuccessStatusCode)
                return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }
        
        public async Task<ApiResponse<int>> ChangeStatusAsync(long AttributeID, bool IsActive)
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/ChangeStatus/{AttributeID}/{IsActive}", new { });

            if (!response.IsSuccessStatusCode)
                return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }

        public async Task<List<LinksAC>> GetLinkMatrixAsync(long attributeID)
        {
            try
            {
                string url = $"{ApiUrl}/GetLinkMatrix?AttributID={attributeID}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<LinksAC>>>();
                    return result?.Data ?? [];
                }
                else
                {
                    // Handle the case where the response is not successful
                    return [];
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (network issues, etc.)
                Console.WriteLine($"An error occurred: {ex.Message}");
                return [];
            }
        }

        public async Task<ApiResponse<int>> SetLinkAsync(long AttributeID, long CategoryID)
        {
            var response = await _httpClient.PostAsync($"{ApiUrl}/SetLink/{AttributeID}/{CategoryID}", null);

            if (!response.IsSuccessStatusCode)
                return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }
        public async Task<ApiResponse<int>> DeleteLinkAsync(long? id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiUrl}/BreakLink/{id}");

            if (!response.IsSuccessStatusCode)
                return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }
        public async Task<List<Attributes>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Attributes>>>($"{ApiUrl}/GetTranslateByID/{baseID}/{langID}"))!;
            return response.Data;
        }
    }
}
