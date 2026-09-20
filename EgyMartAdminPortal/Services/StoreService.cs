using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Models.Stores;
using System.Net;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class StoreService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        private const string BaseApi = "orders/api/Stores/";

        public async Task<ApiResponse<List<StoreItem>>> GetAllStoresAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<StoreItem>>>($"{BaseApi}GetAll");
                return response ?? new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "No data returned from server." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> GetStoreByIdAsync(long storeId)
        {
            try
            {
                var url = $"{BaseApi}GetById/{storeId}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                {
                    url = $"{BaseApi}GetById/GetById/{storeId}";
                    response = await _httpClient.GetAsync(url);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "No data returned." };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = error };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> CreateStoreAsync(CreateStoreModel model)
        {
            try
            {
                var payload = new
                {
                    name = model.Name,
                    addressLine = model.AddressLine,
                    city = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
                    latitude = string.IsNullOrWhiteSpace(model.Latitude) ? null : model.Latitude.Trim(),
                    longitude = string.IsNullOrWhiteSpace(model.Longitude) ? null : model.Longitude.Trim(),
                    phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                    vendorUserId = model.VendorUserId
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseApi}Create", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> EditStoreAsync(EditStoreModel model)
        {
            try
            {
                var payload = new
                {
                    storeId = model.StoreId,
                    name = model.Name,
                    addressLine = model.AddressLine,
                    city = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim(),
                    latitude = string.IsNullOrWhiteSpace(model.Latitude) ? null : model.Latitude.Trim(),
                    longitude = string.IsNullOrWhiteSpace(model.Longitude) ? null : model.Longitude.Trim(),
                    phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                    isActive = model.IsActive,
                    vendorUserId = model.VendorUserId
                };

                var response = await _httpClient.PutAsJsonAsync($"{BaseApi}Edit", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> DeleteStoreAsync(long storeId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseApi}Delete/{storeId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> VerifyStoreAsync(long storeId, bool isVerified)
        {
            try
            {
                var url = $"{BaseApi}Verify/{storeId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                var response = await _httpClient.PutAsync(url, null);
                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                {
                    url = $"{BaseApi}Verify/Verify/{storeId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                    response = await _httpClient.PutAsync(url, null);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreProductItem>>> GetStoreProductsAsync(long storeId)
        {
            try
            {
                var url = $"{BaseApi}GetProducts/{storeId}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                {
                    url = $"{BaseApi}GetProducts/GetProducts/{storeId}";
                    response = await _httpClient.GetAsync(url);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreProductItem>>>();
                    return result ?? new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = "No data returned." };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }

        public async Task<ApiResponse<List<StoreProductItem>>> VerifyStoreProductAsync(long storeProductId, bool isVerified)
        {
            try
            {
                var url = $"{BaseApi}VerifyProduct/{storeProductId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                var response = await _httpClient.PutAsync(url, null);
                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                {
                    url = $"{BaseApi}VerifyProduct/VerifyProduct/{storeProductId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                    response = await _httpClient.PutAsync(url, null);
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreProductItem>>>();
                    return result ?? new ApiResponse<List<StoreProductItem>> { Success = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = $"Server error ({response.StatusCode}): {error}" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }
    }
}
