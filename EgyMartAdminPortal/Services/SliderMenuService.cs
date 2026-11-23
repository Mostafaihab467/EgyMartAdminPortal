using EgyMartAdminPortal.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace EgyMartAdminPortal.Services
{
    public class SliderMenuService(HttpClient httpClient, ILogger<SliderMenuService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<SliderMenuService> _logger = logger;
        protected string ApiUrl = "cms/api/jpt/v2/WidgetsSliders";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<SliderMenu>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SliderMenu>>>($"{ApiUrl}/GetList");

                if (response == null || response.Data == null)
                    return [];

                return response.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Slider menu endpoint returned 404");
                return []; // Return an empty list instead of throwing an error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sliders");
                return []; // Handle other errors gracefully
            }
        }
        
        public async Task<List<SliderMenu>> GetByLangAsync(int langID,long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<SliderMenu>>>($"{ApiUrl}/GetByLang?BaseID={baseID}&LangID={langID}"))!;
            return response.Data;
        }

        public async Task<HttpResponseMessage> CreateAsync(SliderMenu item)
        {
            var request = new
            {
                item.ShortTitle,
                item.MainTitle,
                item.Call2ActionMsg,
                item.Call2ActionURL,
                item.DisplayOrder,
                ImageBase64 = item.ImageURL
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
                ImageURL = item.ImageURL,
                Call2ActionURL = item.Call2ActionURL,
                DisplayOrder = item.DisplayOrder
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit/{item.SliderID}", request);
            return response;
        }

        public async Task<bool> EditSliderImageAsync(int sliderId, string base64Image)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(base64Image);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{ApiUrl}/EditImage/{sliderId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("EditSliderImage failed with HTTP {StatusCode} for slider {SliderId}", response.StatusCode, sliderId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slider image {SliderId}", sliderId);
                return false;
            }
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
