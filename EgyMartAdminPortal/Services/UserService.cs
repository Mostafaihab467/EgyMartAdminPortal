using EgyMartAdminPortal.Models;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace EgyMartAdminPortal.Services
{
    public class UserService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;
        protected string ApiUrl = "auth/api/v1/";
        private async Task<ApiResponse<List<T>>> FetchPendingUsersAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<T>>>($"{ApiUrl}Auth/{endpoint}");
            return response!;
        }
        private async Task<bool> VerifyUserAsync(string userType, long userId, int status)
        {
            var userParam = (userType == "Supplier") ? "userID" : "customerID";
            var response =await _httpClient.PutAsync($"{ApiUrl}Auth/{userType}/Verify?{userParam}={userId}&verifiedID={status}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiResponse<List<Supplier>>> GetPendingVerifySuppliersAsync() => await FetchPendingUsersAsync<Supplier>("PendingVerifySuppliers/Get");
        public async Task<ApiResponse<List<Customer>>> GetPendingVerifyCustomersAsync() => await FetchPendingUsersAsync<Customer>("PendingVerifyCustomer/Get");

        public async Task<bool> VerifySupplierAsync(long supplierId, int status) => await VerifyUserAsync("Supplier", supplierId, status);
        public async Task<bool> VerifyCustomerAsync(long customerId, int status) => await VerifyUserAsync("Customer", customerId, status);

        public async Task<ApiResponse<CreateUserResult>> CreateUserAsync(Person request)
        {
            try
            {
                var bayload = new
                {
                    request.DisplayName,
                    request.UserName,
                    request.Password,
                    request.UserTypeID,
                    request.FirstLogin,
                };
                var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}UsersManagment/Create", bayload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateUserResult>>();
                    Console.WriteLine(result!.Data.UserId);
                    return result ?? new ApiResponse<CreateUserResult> { Success = false, ResponseEngMsg = "Empty response from server." };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<CreateUserResult>
                    {
                        Success = false,
                        ResponseEngMsg = $"Server error: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<CreateUserResult>
                {
                    Success = false,
                    ResponseEngMsg = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<UserTypeWithCount>>> GetUsersTypesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserTypeWithCount>>>($"{ApiUrl}UsersManagment/Types/GetListWithCount");
                return response ?? new ApiResponse<List<UserTypeWithCount>> { Success = false, ResponseEngMsg = "No data returned." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserTypeWithCount>>
                {
                    Success = false,
                    ResponseEngMsg = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<Person>>> GetListByTypeAsync(int typeID, int pageNumber = 1, int sizePerPage = 50)
        {
            try
            {
                string url = $"{ApiUrl}UsersManagment/GetListByType?TypeID={typeID}&PageNumber={pageNumber}&SizePerPage={sizePerPage}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Person>>>();
                    if (result?.Data != null)
                    {
                        foreach (var person in result.Data)
                        {
                            if (person.VerificationStatus == null)
                            {
                                person.VerificationStatus = 0; // Set default value if null
                            }
                            if(person.ProfileImage == null)
                            {
                                person.ProfileImage = $"images/avatars/user.jpg";
                            }
                            //if (!person.ProfileImage!.StartsWith("https://"))
                            //{
                            //    person.ProfileImage = $"https://api.egyptbigmart.com:5051/Uploads/ProfileImgs/{person.ProfileImage}";
                            //}
                        }
                    }
                    return result ?? new ApiResponse<List<Person>> { Success = false, ResponseEngMsg = "Empty response from server." };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<Person>>
                    {
                        Success = false,
                        ResponseEngMsg = $"Server error: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<Person>>
                {
                    Success = false,
                    ResponseEngMsg = $"Exception: {ex.Message}"
                };
            }
        }

        public async Task<bool> StateUserAsync(long userId, bool active)
        {
            try
            {
                // Construct the URL for blocking/unblocking the user
                var url = $"{ApiUrl}UsersManagment/BlockOrUnBlock?UserID={userId}&Active={active}";

                // Send the PUT request to the API
                var response = await _httpClient.PutAsync(url, null);

                // Check if the request was successful
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Handle exceptions (network issues, etc.)
                Console.WriteLine($"Exception occurred while updating user status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(long userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiUrl}UsersManagment/ResetPassword?UserID={userId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]> DownloadSupplierAttachmentAsync(long OwnerID)
        {
            var response = await _httpClient.GetAsync($"{ApiUrl}/download_verficationFilePDf/{OwnerID}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new Exception("Failed to download attachment");
        }

    }
}
