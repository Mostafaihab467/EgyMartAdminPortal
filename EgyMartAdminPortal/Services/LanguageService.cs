using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class LanguageService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/v1/Lubs/LangList";
        public async Task<List<Language>> GetAsync()
        {
            var response = (await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>($"{ApiUrl}"))!;
            return response.Data;
        }

        public async Task<Dictionary<int, string>> GetLanguagesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>(ApiUrl);
            if (response?.Data == null)
                return new Dictionary<int, string>();

            var allowedLanguageIds = new HashSet<int> { 2, 3 }; // Arabic and French IDs

            return response.Data
                .Where(lang => allowedLanguageIds.Contains(lang.LangID))
                .ToDictionary(lang => lang.LangID, lang => lang.LangTitle);
        }

    }
}
