using EgyMartAdminPortal.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class LanguageService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private Dictionary<int, string>? _cachedLanguages;
        private const string StorageKey = "cachedLanguages";

        protected string ApiUrl = "cms/api/jpt/v2/Lubs/LangList";

        public async Task<List<Language>> GetAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>($"{ApiUrl}");
            return response?.Data ?? [];
        }

        public async Task<Dictionary<int, string>> GetLanguagesAsync()
        {
            if (_cachedLanguages != null)
                return _cachedLanguages;

            // Try to load from localStorage
            var json = await _jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _cachedLanguages = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                if (_cachedLanguages != null)
                    return _cachedLanguages;
            }

            // Fetch from API
            var languages = await GetAsync();

            var allowedLanguageIds = new HashSet<int> { 2, 3 };

            _cachedLanguages = languages
                .Where(lang => allowedLanguageIds.Contains(lang.LangID))
                .ToDictionary(lang => lang.LangID, lang => lang.LangTitle);

            // Store in localStorage
            var serialized = JsonSerializer.Serialize(_cachedLanguages);
            await _jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", StorageKey, serialized);

            return _cachedLanguages;
        }

        public async Task ClearCache()
        {
            _cachedLanguages = null;
            await _jsRuntime.InvokeVoidAsync("localStorageHelper.removeItem", StorageKey);
        }
    }
}
