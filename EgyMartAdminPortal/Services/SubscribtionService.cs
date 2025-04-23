using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class SubscribtionService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        private const string ApiUrl = "cms/api/v2/SubscribtionList";

        public async Task<List<Subscribtion>> GetSubscriptionsAsync(DateTime startDate, DateTime endDate)
        {
            var requestPayload = new
            {
                startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/GetList", requestPayload);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<Subscribtion>>();
                return result ?? [];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching subscriptions: {ex.Message}");
                return [];
            }
        }
    }
}
