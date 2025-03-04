using EgyMartAdminPortal.Models;
using System.Net.Http.Json;
namespace EgyMartAdminPortal.Services
{
    public class SubscriptionService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "ads/api/v1/Ads/ManageSubscriptions";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<ApiResponse<List<Subscription>>> GetSubscriptionsAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Subscription>>>($"{ApiUrl}/GetSubscriptionsList?repType=2"))!;
            return response;
        }

        public async Task<List<Subscription>> GetByLangAsync(int langID, long baseID)
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Subscription>>>($"{ApiUrl}/GetTranlsate?basePlanID={baseID}&langID={langID}"))!;
            return response.Data;
        }

        public async Task<ApiResponse<List<AdsLocation>>> GetLocationsAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<AdsLocation>>>($"{ApiUrl}/AdsLocationList"))!;
            return response;
        }

        public async Task<HttpResponseMessage> CreateAsync(Subscription subscription)
        {
            var requestBody = new { subscription.PlanTitle, subscription.LocationID, subscription.DurationDays, subscription.CostBefore, subscription.Cost, subscription.PlanDescription, subscription.LangID, subscription.BaseID };
            return await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", requestBody);
        }
        
        public async Task<HttpResponseMessage> EditAsync(Subscription subscription)
        {
            var requestBody = new { subscription.PlanTitle, subscription.LocationID, subscription.DurationDays, subscription.CostBefore, subscription.Cost, subscription.PlanDescription };
            return await _httpClient.PutAsJsonAsync($"{ApiUrl}/Edit/{subscription.AdsPlanID}", requestBody);
        }
        
        public async Task<ApiResponse<int>> ChangeStatusAsync(long AdsPlanID, bool IsActive)
        {
            var requestPayload = new { IsActive };
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/SetSubscriptionStatus/{AdsPlanID}", requestPayload);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result ?? new ApiResponse<int> { Success = false, Data = 0 };
        }
    }
}
