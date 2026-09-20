using EgyMartAdminPortal.Models;
using EgyMartAdminPortal.Models.Coupons;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class CouponService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
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
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = ex.Message };
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
                return new ApiResponse<CouponItem> { Success = false, ResponseEngMsg = ex.Message };
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

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = error };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = ex.Message };
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

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = error };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = ex.Message };
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

                var error = await response.Content.ReadAsStringAsync();
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = error };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<CouponItem>> { Success = false, ResponseEngMsg = ex.Message };
            }
        }
    }
}
