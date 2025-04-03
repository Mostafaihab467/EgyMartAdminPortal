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
            return response!;
        }
        private async Task<bool> VerifyUserAsync(string userType, long userId, int status)
        {
            var userParam = (userType == "Supplier") ? "userID" : "customerID";
            var response =await _httpClient.PutAsync($"{ApiUrl}/{userType}/Verify?{userParam}={userId}&verifiedID={status}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiResponse<List<Supplier>>> GetPendingVerifySuppliersAsync() => await FetchPendingUsersAsync<Supplier>("PendingVerifySuppliers/Get");
        public async Task<ApiResponse<List<Customer>>> GetPendingVerifyCustomersAsync() => await FetchPendingUsersAsync<Customer>("PendingVerifyCustomer/Get");

        public async Task<bool> VerifySupplierAsync(long supplierId, int status) => await VerifyUserAsync("Supplier", supplierId, status);
        public async Task<bool> VerifyCustomerAsync(long customerId, int status) => await VerifyUserAsync("Customer", customerId, status);

        public async Task<byte[]> DownloadSupplierAttachmentAsync(long OwnerID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/download_verficationFilePDf/{OwnerID}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new Exception("Failed to download attachment");
        }


        public async Task<ApiResponse<int>> CreateAsync(Person newPerson)
        {
            //var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}/Create", newPerson);

            //if (!response.IsSuccessStatusCode)
            //    return new ApiResponse<int> { Success = false, Data = 0, ResponseEngMsg = $"Error: {response.StatusCode}" };

            //var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            //return result ?? new ApiResponse<int> { Success = false, Data = 0 };

            return new ApiResponse<int>
            {
                Success = false,
                Data = 0,
                ResponseEngMsg = "CreateAsync method is not implemented yet."
            };
        }

    }
}
