using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class FAQService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/v1/WidgetsFAQ";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<FAQ>> GetAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<FAQ>>>($"{ApiUrl}/GetList"))!;
            return response.Data;
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
    }
}
