using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Models.Coupons;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class CouponService(HttpClient httpClient, ILogger<CouponService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<CouponService> _logger = logger;
        private const string BaseApi = "orders/api/Coupons/";

        public async Task<ApiResponse<List<CouponItem>>> GetAllCouponsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CouponItem>>>($"{BaseApi}GetAll");
                return response ?? new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = "No data returned." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coupons");
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<CouponItem>> GetCouponByIdAsync(int couponId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CouponItem>>($"{BaseApi}GetById/{couponId}");
                return response ?? new ApiResponse<CouponItem> { Success = false, ResponseEngMsg = "No data returned." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coupon {CouponId}", couponId);
                return new ApiResponse<CouponItem> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<CouponItem>>> CreateCouponAsync(CreateCouponModel model)
        {
            try
            {
                var payload = new
                {
                    code = model.Code.Trim(),
                    discountType = model.DiscountType,
                    discountValue = model.DiscountValue,
                    minOrderTotal = model.MinOrderTotal,
                    maxUses = model.MaxUses,
                    expireDate = model.ExpireDate,
                    type = model.SelectedVendorId.HasValue ? model.SelectedVendorId.Value.ToString() : (string.IsNullOrWhiteSpace(model.Type) ? "All" : model.Type.Trim())
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseApi}Create", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CouponItem>>>();
                    return result ?? new ApiResponse<List<CouponItem>> { Success = true };
                }

                return new ApiResponse<List<CouponItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to create coupon.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<CouponItem>>> EditCouponAsync(EditCouponModel model)
        {
            try
            {
                var payload = new
                {
                    couponId = model.CouponId,
                    code = model.Code.Trim(),
                    discountType = model.DiscountType,
                    discountValue = model.DiscountValue,
                    minOrderTotal = model.MinOrderTotal,
                    maxUses = model.MaxUses,
                    expireDate = model.ExpireDate,
                    isActive = model.IsActive
                };

                var response = await _httpClient.PutAsJsonAsync($"{BaseApi}Edit", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CouponItem>>>();
                    return result ?? new ApiResponse<List<CouponItem>> { Success = true };
                }

                return new ApiResponse<List<CouponItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to update coupon.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon {CouponId}", model.CouponId);
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
            }
        }

        public async Task<ApiResponse<List<CouponItem>>> DeleteCouponAsync(int couponId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseApi}Delete/{couponId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CouponItem>>>();
                    return result ?? new ApiResponse<List<CouponItem>> { Success = true };
                }

                return new ApiResponse<List<CouponItem>>
                {
                    Success = false,
                    ResponseEngMsg = await SafeErrorMessageAsync(response, "Unable to delete coupon.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon {CouponId}", couponId);
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = "An unexpected error occurred. Please try again." };
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
