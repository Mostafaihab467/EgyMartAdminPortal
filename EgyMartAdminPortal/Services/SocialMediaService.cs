using EgyMartAdminPortal.Models;
using System.Net.Http.Json;
using System.Net.Http;
using EgyMartAdminPortal.Models.Components;

namespace EgyMartAdminPortal.Services
{
    public class SocialMediaService(HttpClient httpClient )
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/jpt/v2/WidgetsSocialMedia";
        public async Task<List<SocialMedia>> GetAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<SocialMedia>>>($"{ApiUrl}/Get?reptype=1");

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

        public async Task<HttpResponseMessage>  CreateAsync(SocialMedia social)
        {
            var socialRequest = new SocialMedia
            {
                LinkTitle = social.LinkTitle,
                LinkTarget = social.LinkTarget,
                DisplayOrder = social.DisplayOrder,
                LinkIcon = social.LinkIcon
            };
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", socialRequest);
            return response;
        }

        public async Task<HttpResponseMessage> EditAsync(SocialMedia social)
        {
            var socialRequest = new SocialMedia
            {
                LinkID = social.LinkID,
                LinkTitle = social.LinkTitle,
                LinkTarget = social.LinkTarget,
                DisplayOrder = social.DisplayOrder,
                LinkIcon = social.LinkIcon
            };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit", socialRequest);
            return response;
        }
        
        public async Task<ApiResponse<int>> ChangeStatusAsync(short ID, bool IsActive)
        {
            var requestPayload = new
            {
                LinkID = ID,
                NewStatus = IsActive
            };

            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/ChangeStatus", requestPayload);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int>() { Success = false, Data = 0};
        }
    }
}
