using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class CategoryService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "products/api/v2/CategoryList";
        public async Task<List<Category>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>($"{ApiUrl}/GetTopLevel?LangID=1&RepType=1");

                if (response == null || response.Data == null)
                    return [];

                return response.Data;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("API returned 404 - Resource not found.");
                return [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return [];
            }
        }
        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }
        public async Task<ApiResponse<List<Category>>> GetChildAsync(long categoryID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/GetChild/{categoryID}/0");

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
        public async Task<List<Category>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>($"{ApiUrl}/GetByLang/{baseID}?LangID={langID}"))!;
            return response.Data;
        }
    }
}
