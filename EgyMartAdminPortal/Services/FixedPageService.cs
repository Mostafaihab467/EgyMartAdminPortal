using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class FixedPageService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/v1/Widgets_FixedPages";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<FixedPage>> GetAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<FixedPage>>>($"{ApiUrl}/GetList"))!;
            return response.Data;
        }
        
        public async Task<List<FixedPage>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<FixedPage>>>($"{ApiUrl}/GetByLang/{baseID}/{langID}"))!;
            return response.Data;
        }

        public async Task<HttpResponseMessage> CreateAsync(FixedPage item)
        {
            var request = new FixedPage
            {
                PageTitle = item.PageTitle,
                PageBody = item.PageBody,
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", request);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(FixedPage item)
        {
            var request = new FixedPage
            {
                PageID = item.PageID,
                PageTitle = item.PageTitle,
                PageBody = item.PageBody
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", request);
            return response;
        }

        public async Task<ApiResponse<int>> ChangeStatusAsync(int ID, bool newState)
        {
            var requestPayload = new
            {
                PageID = ID,
                IsActive = newState
            };

            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/ChangeStatus", requestPayload);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int>() { Success = false, Data = 0 };
        }
    }
}
