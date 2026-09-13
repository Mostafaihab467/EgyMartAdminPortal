using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class FooterMenuService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/jpt/v2/WidgetsFooterLinks";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<FooterMenu>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<FooterMenu>>>($"{ApiUrl}/GetList");

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
        
        public async Task<List<FooterMenu>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<FooterMenu>>>($"{ApiUrl}/GetChild/{baseID}/{langID}"))!;
            return response.Data;
        }

        public async Task<List<FooterMenu>> GetChildAsync(int ID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/GetChild/1/1/{ID}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return [];
            }

            var data = await response.Content.ReadFromJsonAsync<List<FooterMenu>>();

            if (data == null)
            {
                return [];
            }

            return data;
        }

        public async Task<HttpResponseMessage> CreateAsync(FooterMenu item)
        {
            var request = new FooterMenu
            {
                FooterItemTitle = item.FooterItemTitle,
                TargetUrl = item.TargetUrl,
                DisplayOrder = item.DisplayOrder,
                ColumnIndex = item.ColumnIndex
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", request);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(FooterMenu item)
        {
            var request = new FooterMenu
            {
                FooterItemID = item.FooterItemID,
                FooterItemTitle = item.FooterItemTitle,
                TargetUrl = item.TargetUrl,
                DisplayOrder = item.DisplayOrder,
                ColumnIndex = item.ColumnIndex
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", request);
            return response;
        }

        public async Task<ApiResponse<int>> ChangeStatusAsync(int ID, bool IsActive)
        {
            var requestPayload = new
            {
                FooterItemID = ID,
                IsActive
            };

            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/ChangeStatus", requestPayload);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int>() { Success = false, Data = 0 };
        }
    }
}
