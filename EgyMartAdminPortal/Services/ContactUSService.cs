using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class ContactUsService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "cms/api/jpt/v2/Widget_ContactUs/";

        public string GetTranslateApiUrl()
        {
            return $"{ApiUrl}";
        }

        public async Task<List<ContactUs>> GetByLangAsync(int langID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}GetList?LangID={langID}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch contact us data.");

            var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<ContactUs>>>();
            return content?.Data ?? [];
        }

        public async Task<bool> CreateContactUsAsync(ContactUs contact)
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}Create", contact);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
                return result?.Success == true;
            }

            return false;
        }

        public async Task<bool> UpdateContactUsAsync(ContactUs model)
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}Update/{model.ContactUsID}", model);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result?.Success == true;
        }

        public async Task<bool> TranslateContactUsAsync(object model)
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}Translate", model);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return result?.Success == true;
        }

    }
}
