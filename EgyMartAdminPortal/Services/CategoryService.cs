using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class CategoryService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "products/api/v1/CategoryList";
        public async Task<ApiResponse<List<Category>>> GetAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>($"{ApiUrl}/GetTopLevel?LangID=1&RepType=0"))!;
            return response;
        }

        public async Task<ApiResponse<List<Category>>> GetChildAsync(long categoryID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/GetChild/{categoryID}/1");

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
    }
}
