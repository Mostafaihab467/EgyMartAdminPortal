using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class SliderMenuService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/v1/WidgetsSliders";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<SliderMenu>> GetAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<SliderMenu>>>($"{ApiUrl}/GetList"))!;
            return response.Data ?? [];
        }
        
        public async Task<List<SliderMenu>> GetByLangAsync(int langID,long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<SliderMenu>>>($"{ApiUrl}/GetByLang?BaseID={baseID}&LangID={langID}"))!;
            return response.Data;
        }

        public async Task<HttpResponseMessage> CreateAsync(SliderMenu item)
        {
            var request = new SliderMenu
            {
                MainTitle = item.MainTitle,
                ShortTitle = item.ShortTitle,
                Call2ActionMsg = item.Call2ActionMsg,
                ImageURL = item.ImageURL,
                Call2ActionURL = item.Call2ActionURL,
                DisplayOrder = item.DisplayOrder
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", request);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(SliderMenu item)
        {
            var request = new SliderMenu
            {
                ShortTitle = item.ShortTitle,
                MainTitle = item.MainTitle,
                Call2ActionMsg = item.Call2ActionMsg,
                Call2ActionURL = item.Call2ActionURL,
                DisplayOrder = item.DisplayOrder
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit/{item.SliderID}", request);
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
