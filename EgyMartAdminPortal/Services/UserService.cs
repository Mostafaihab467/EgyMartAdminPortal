using EgyMartAdminPortal.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EgyMartAdminPortal.Services
{
    public class UserService(HttpClient httpClient, LocalStorageService localStorageService)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly LocalStorageService _localStorageService = localStorageService;
        protected string ApiUrl = "auth/api/v2/";
        private async Task<ApiResponse<List<T>>> FetchPendingUsersAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<T>>>($"{ApiUrl}Auth/{endpoint}");
            return response!;
        }

        private async Task<bool> VerifyUserAsync(string userType, long userId, long verifiedID, bool? status = null)
        {
            var userParam = (userType == "Supplier") ? "userID" : "customerID";
            var statusParam = (status.HasValue) ? $"&status={(status.Value ? 1 : 2)}" : string.Empty;

            var url = $"{ApiUrl}Auth/{userType}/Verify?{userParam}={userId}&verifiedID={verifiedID}{statusParam}";

            var response = await _httpClient.PutAsync(url, null);
            return response.IsSuccessStatusCode;
        }

        public async Task<ApiResponse<List<Supplier>>> GetPendingVerifySuppliersAsync() => await FetchPendingUsersAsync<Supplier>("PendingVerifySuppliers/Get");
        public async Task<ApiResponse<List<Customer>>> GetPendingVerifyCustomersAsync() => await FetchPendingUsersAsync<Customer>("PendingVerifyCustomer/Get");

        public async Task<bool> VerifySupplierAsync(long supplierId, long verifiedID, bool status) =>
    await VerifyUserAsync("Supplier", supplierId, verifiedID, status);

        public async Task<bool> VerifyCustomerAsync(long customerId, long verifiedID) =>
            await VerifyUserAsync("Customer", customerId, verifiedID);

        public async Task<long> GetUserIdAsync()
        {
            return (await _localStorageService.GetItemAsync<Person>("userData"))!.UserID;
        }

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

        public async Task<List<UserTypeWithCount>> GetAdminTypesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserTypeWithCount>>>(
                    $"{ApiUrl}UsersManagment/Types/GetSome");

                if (response != null && response.Success)
                    return response.Data;

                return new List<UserTypeWithCount>(); // or throw error
            }
            catch (Exception ex)
            {
                // Optionally log or rethrow
                throw new ApplicationException("Error fetching user types", ex);
            }
        }

        public async Task<string> GetUserTypeTitleByIdAsync(int id)
        {
            var types = (await GetUsersTypesAsync()).Data;
            return types.FirstOrDefault(t => t.UserTypeID == id)?.UserTypeTitle ?? "Unknown";
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
                                person.VerificationStatus = 0;
                            }
                            if (person.ProfileImage == null)
                            {
                                person.ProfileImage = $"images/avatars/user.jpg";
                            }
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

        public async Task<bool> ResetLoginFailAsync(string userName)
        {
            try
            {
                var payload = new { userName };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync($"{ApiUrl}Auth/FailCounter/Reset", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ResetLoginFailAsync] HTTP error: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();

                if (result == null)
                {
                    Console.WriteLine("[ResetLoginFailAsync] Failed to deserialize response.");
                    return false;
                }

                if (!result.Success)
                {
                    Console.WriteLine($"[ResetLoginFailAsync] API responded with failure: {result.ResponseEngMsg}");
                }

                return result.Data == 0 ? false : true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ResetLoginFailAsync] Exception: {ex.Message}");
                return false;
            }
        }


        public async Task<string?> DownloadSupplierAttachmentAsync(long OwnerID)
        {
            var response = await _httpClient.GetAsync($"cms/api/jpt/v2/CompanyProfile/download_verficationFilePDf/{OwnerID}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("fileBase64", out var fileBase64Element))
                {
                    return fileBase64Element.GetString();
                }

                return null;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new Exception("Failed to download attachment");
        }

    }
}
