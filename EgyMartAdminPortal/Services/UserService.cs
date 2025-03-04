using EgyMartAdminPortal.Models;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class UserService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "auth/api/v1/Auth";
        private async Task<ApiResponse<List<T>>> FetchPendingUsersAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<T>>>($"{ApiUrl}/{endpoint}");
            //Console.WriteLine(response);
            return response!;
        }
        private async Task<bool> VerifyUserAsync(string userType, long userId)
        {
            var response = await _httpClient.PutAsync($"{ApiUrl}/{userType}/Verify?userID={userId}&verifiedID=2", null);
            //Console.WriteLine(response);
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiResponse<List<Supplier>>> GetPendingVerifySuppliersAsync() => await FetchPendingUsersAsync<Supplier>("PendingVerifySuppliers/Get");
        public async Task<ApiResponse<List<Customer>>> GetPendingVerifyCustomersAsync() => await FetchPendingUsersAsync<Customer>("PendingVerifyCustomer/Get");

        public async Task<bool> VerifySupplierAsync(long supplierId) => await VerifyUserAsync("Supplier", supplierId);
        public async Task<bool> VerifyCustomerAsync(long customerId) => await VerifyUserAsync("Customer", customerId);


    }
}
