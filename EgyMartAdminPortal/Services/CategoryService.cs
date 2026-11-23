using EgyMartAdminPortal.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class CategoryService(HttpClient httpClient, ILogger<CategoryService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<CategoryService> _logger = logger;
        protected string ApiUrl = "products/api/jpt/v2/CategoryList";
        public async Task<List<Category>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>($"{ApiUrl}/GetTopLevel?LangID=1&RepType=0");

                if (response == null || response.Data == null)
                    return [];

                return response.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Categories endpoint returned 404");
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                return [];
            }
        }
        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }
        public async Task<ApiResponse<List<Category>>> GetChildAsync(long categoryID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/GetChild/{categoryID}/0/1");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new();
            }

            var data = await response.Content.ReadFromJsonAsync<ApiResponse<List<Category>>>();

            return data ?? new();
        }
        public async Task<HttpResponseMessage> CreateAsync(Category category)
        {
            var Cattquest = new Category
            {
                CategoryTitle = category.CategoryTitle,
                ParentID = category.ParentID,
                LangID = category.LangID,
                DisplayOrder = category.DisplayOrder,
                CategoryImageURL = category.CategoryImageURL
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", Cattquest);
            return response;
        }
        public async Task<HttpResponseMessage> EditAsync(Category category)
        {
            var Cattquest = new Category
            {
                CategoryID = category.CategoryID,
                CategoryTitle = category.CategoryTitle,
                CategoryImageURL = category.CategoryImageURL,
                DisplayOrder = category.DisplayOrder,
            };

            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", Cattquest);
            return response;
        }
        public async Task<HttpResponseMessage> ChangeStatusAsync(long rID, bool isActive)
        {
            var requestPayload = new
            {
                Rid = rID,
                NewStatus = isActive
            };

            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/ChangeStatus", requestPayload);
            return response;
        }
        public async Task<HttpResponseMessage> DeleteAsync(long categoryID)
        {
            // Deletes the category and its whole sub tree (the API owns that cascade).
            return await _httpClient.DeleteAsync($"{ApiUrl}/Delete/{categoryID}");
        }
        public async Task<List<Category>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>($"{ApiUrl}/GetByLang/{baseID}?LangID={langID}"))!;
            return response.Data;
        }
    }
}
