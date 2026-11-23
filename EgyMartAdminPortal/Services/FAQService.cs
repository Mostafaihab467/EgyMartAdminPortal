using EgyMartAdminPortal.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class FAQService(HttpClient httpClient, ILogger<FAQService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<FAQService> _logger = logger;
        protected string ApiUrl = "cms/api/jpt/v2/WidgetsFAQ";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<FAQ>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<FAQ>>>($"{ApiUrl}/GetList");

                if (response == null || response.Data == null)
                    return [];

                return response.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "FAQ endpoint returned 404");
                return []; // Return an empty list instead of throwing an error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FAQs");
                return []; // Handle other errors gracefully
            }
        }
        
        public async Task<List<FAQ>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<FAQ>>>($"{ApiUrl}/GetByLang?BaseID={baseID}&LangID={langID}"))!;
            return response.Data;
        }

        public async Task<HttpResponseMessage> CreateAsync(FAQ item)
        {
            var request = new FAQ
            {
                QTitle = item.QTitle,
                QAnswer = item.QAnswer,
                DisplayOrder = item.DisplayOrder
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", request);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(FAQ item)
        {
            var request = new FAQ
            {
                QTitle = item.QTitle,
                QAnswer = item.QAnswer,
                DisplayOrder = item.DisplayOrder
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit/{item.Qid}", request);
            return response;
        }

        public async Task<ApiResponse<int>> ChangeStatusAsync(int ID, bool newState)
        {
            var response = await _httpClient.PutAsync($"{ApiUrl}/ChangeStatus/{ID}?IsActive={newState}", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int>() { Success = false, Data = 0 };
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            return await _httpClient.DeleteAsync($"{ApiUrl}/Delete/{id}");
        }
    }
}
