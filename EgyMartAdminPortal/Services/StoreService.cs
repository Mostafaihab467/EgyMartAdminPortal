using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Models.Stores;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class StoreService(HttpClient httpClient, ILogger<StoreService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<StoreService> _logger = logger;
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
                _logger.LogError(ex, "Error loading stores");
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> GetStoreByIdAsync(long storeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseApi}GetById/{storeId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "No data returned." };
                }

                return new ApiResponse<List<StoreItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Failed to load store details.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading store {StoreId}", storeId);
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
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

                return new ApiResponse<List<StoreItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to create store.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating store");
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
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

                return new ApiResponse<List<StoreItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to update store.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating store {StoreId}", model.StoreId);
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
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

                return new ApiResponse<List<StoreItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to delete store.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting store {StoreId}", storeId);
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<StoreItem>>> VerifyStoreAsync(long storeId, bool isVerified)
        {
            try
            {
                var url = $"{BaseApi}Verify/{storeId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                var response = await _httpClient.PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreItem>>>();
                    return result ?? new ApiResponse<List<StoreItem>> { Success = true };
                }

                return new ApiResponse<List<StoreItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to update store verification.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying store {StoreId}", storeId);
                return new ApiResponse<List<StoreItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<StoreProductItem>>> GetStoreProductsAsync(long storeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseApi}GetProducts/{storeId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreProductItem>>>();
                    return result ?? new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = "No data returned." };
                }

                return new ApiResponse<List<StoreProductItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Failed to load store products.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products for store {StoreId}", storeId);
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<StoreProductItem>>> VerifyStoreProductAsync(long storeProductId, bool isVerified)
        {
            try
            {
                var url = $"{BaseApi}VerifyProduct/{storeProductId}?isVerified={isVerified.ToString().ToLowerInvariant()}";
                var response = await _httpClient.PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreProductItem>>>();
                    return result ?? new ApiResponse<List<StoreProductItem>> { Success = true };
                }

                return new ApiResponse<List<StoreProductItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to update store product verification.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying store product {StoreProductId}", storeProductId);
                return new ApiResponse<List<StoreProductItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        private static async Task<string> SafeErrorMessageAsync(HttpResponseMessage response, string fallback)
        {
            if ((int)response.StatusCode >= 500)
                return "A server error occurred. Please try again later.";
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("responseEngMsg", out var msg) && msg.ValueKind == JsonValueKind.String)
                {
                    var text = msg.GetString();
                    if (!string.IsNullOrWhiteSpace(text) && text.Length <= 200)
                        return text;
                }
            }
            catch
            {
                // Ignore non-JSON bodies
            }
            return fallback;
        }
    }
}
