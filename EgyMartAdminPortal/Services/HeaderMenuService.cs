using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class HeaderMenuService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/jpt/v2/WidgetsHeaderMenu";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<HeaderMenu>> GetTopLevelAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<HeaderMenu>>($"{ApiUrl}/GetTopLevel/1/1");

                if (response == null || response == null)
                    return [];

                return response;
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
        
        public async Task<List<HeaderMenu>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<HeaderMenu>>>($"{ApiUrl}/GetByLang/{baseID}/{langID}"))!;
            return response.Data;
        }

        public async Task<List<HeaderMenu>> GetChildAsync(long ID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/GetChild/1/1/{ID}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return [];
            }

            var data = await response.Content.ReadFromJsonAsync<List<HeaderMenu>>();

            if (data == null)
            {
                return [];
            }

            return data;
        }

        public async Task<HttpResponseMessage> CreateAsync(HeaderMenu header)
        {
            var headerRequest = new HeaderMenu
            {
                MenuItemTitle = header.MenuItemTitle,
                LangID = header.LangID,
                BaseID = header.BaseID,
                ParentID = header.ParentID,
                DisplayOrder = header.DisplayOrder,
                TargetUrl = header.TargetUrl
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", headerRequest);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(HeaderMenu header)
        {
            var headerRequest = new HeaderMenu
            {
                MenuItemID = header.MenuItemID,
                MenuItemTitle = header.MenuItemTitle,
                DisplayOrder = header.DisplayOrder,
                TargetUrl = header.TargetUrl,
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", headerRequest);
            return response;
        }

        public async Task<HttpResponseMessage> ChangeStatusAsync(long menuItemID, bool isActive)
        {
            var requestPayload = new
            {
                MenuItemID = menuItemID,
                NewStatus = isActive
            };

            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/ChangeStatus", requestPayload);
            return response;
        }
    }
}
