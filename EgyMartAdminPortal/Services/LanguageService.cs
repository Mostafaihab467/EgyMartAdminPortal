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
            // Check in-memory cache
            if (_cachedLanguages != null)
                return _cachedLanguages
                    .Select(lang => new Language { LangID = lang.Key, LangTitle = lang.Value })
                    .ToList();

            // Check localStorage
            var json = await _jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _cachedLanguages = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                if (_cachedLanguages != null)
                {
                    return _cachedLanguages
                        .Select(lang => new Language { LangID = lang.Key, LangTitle = lang.Value })
                        .ToList();
                }
            }

            // If not found, fetch from API
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Language>>>($"{ApiUrl}");

            _cachedLanguages = response!.Data
                .ToDictionary(lang => lang.LangID, lang => lang.LangTitle);

            // Store in localStorage
            var serialized = JsonSerializer.Serialize(_cachedLanguages);
            await _jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", StorageKey, serialized);

            return response?.Data ?? [];
        }

        public async Task<Dictionary<int, string>> GetLanguagesAsync()
        {
            var allowedLanguageIds = new HashSet<int> { 2, 3 }; // Only Arabic and French

            if (_cachedLanguages != null)
                return _cachedLanguages
                    .Where(lang => allowedLanguageIds.Contains(lang.Key))
                    .ToDictionary(lang => lang.Key, lang => lang.Value);

            // Try to load from localStorage
            var json = await _jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                _cachedLanguages = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                if (_cachedLanguages != null)
                {
                    return _cachedLanguages
                        .Where(lang => allowedLanguageIds.Contains(lang.Key))
                        .ToDictionary(lang => lang.Key, lang => lang.Value);
                }
            }

            // Fetch from API
            var languages = await GetAsync();

            _cachedLanguages = languages
                .ToDictionary(lang => lang.LangID, lang => lang.LangTitle);

            return _cachedLanguages
                .Where(lang => allowedLanguageIds.Contains(lang.Key))
                .ToDictionary(lang => lang.Key, lang => lang.Value);
        }

        public async Task ClearCache()
        {
            _cachedLanguages = null;
            await _jsRuntime.InvokeVoidAsync("localStorageHelper.removeItem", StorageKey);
        }
    }
}
