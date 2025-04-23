using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class LanguageService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        private Dictionary<int, string>? _cachedLanguages;

        protected string ApiUrl = "cms/api/v2/Lubs/LangList";

        public async Task<List<Language>> GetAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>($"{ApiUrl}");
            return response?.Data ?? [];
        }

        public async Task<Dictionary<int, string>> GetLanguagesAsync()
        {
            // ✅ Return cached languages if already loaded
            if (_cachedLanguages != null)
                return _cachedLanguages;

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>(ApiUrl);
            if (response?.Data == null)
                return new Dictionary<int, string>();

            var allowedLanguageIds = new HashSet<int> { 2, 3 }; // Arabic and French IDs

            _cachedLanguages = response.Data
                .Where(lang => allowedLanguageIds.Contains(lang.LangID))
                .ToDictionary(lang => lang.LangID, lang => lang.LangTitle);

            return _cachedLanguages;
        }

        // Optional: To force refresh in future if needed
        public void ClearCache()
        {
            _cachedLanguages = null;
        }
    }
}
