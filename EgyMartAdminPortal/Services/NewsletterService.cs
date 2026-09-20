using EgyMartAdminPortal.Models.Newsletter;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EgyMartAdminPortal.Services
{
    public class NewsletterService : IAsyncDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<NewsletterService> _logger;
        private const string BaseApi = "cms/api/v1/Newsletter/";

        private HubConnection? _hubConnection;

        public event Action<NotificationPayload>? OnNotificationReceived;
        public event Action<bool>? OnConnectionStatusChanged;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public string ConnectionState => _hubConnection?.State.ToString() ?? "Disconnected";

        public NewsletterService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILogger<NewsletterService> logger)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        #region SignalR Hub Connection

        public async Task StartSignalRAsync()
        {
            if (_hubConnection != null) return;

            try
            {
                // Resolve Hub URL using ApiUrl base address (e.g. http://localhost:70/cms/hubs/notifications)
                string hubUrl = new Uri(_httpClient.BaseAddress ?? new Uri("http://localhost:70"), "cms/hubs/notifications").ToString();

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<NotificationPayload>("ReceiveNotification", (payload) =>
                {
                    _logger.LogInformation("SignalR notification received: {Title}", payload?.Title);
                    if (payload != null)
                    {
                        OnNotificationReceived?.Invoke(payload);
                    }
                });

                _hubConnection.Reconnecting += (error) =>
                {
                    OnConnectionStatusChanged?.Invoke(false);
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += (connectionId) =>
                {
                    OnConnectionStatusChanged?.Invoke(true);
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += (error) =>
                {
                    OnConnectionStatusChanged?.Invoke(false);
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                OnConnectionStatusChanged?.Invoke(true);
                _logger.LogInformation("SignalR connected successfully to {HubUrl}", hubUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not start SignalR hub connection to /cms/hubs/notifications");
                OnConnectionStatusChanged?.Invoke(false);
            }
        }

        #endregion

        #region HTTP Endpoints

        public async Task<NewsletterStats?> GetStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<StatsResponse>($"{BaseApi}stats");
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting newsletter stats");
                return null;
            }
        }

        public async Task<SubscribersResponse?> GetSubscribersAsync(int pageNumber = 1, int pageSize = 10, string? search = null, bool? isActive = null, string? userType = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };
                if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");
                if (isActive.HasValue) queryParams.Add($"isActive={isActive.Value.ToString().ToLower()}");
                if (!string.IsNullOrWhiteSpace(userType)) queryParams.Add($"userType={Uri.EscapeDataString(userType.Trim())}");

                string url = $"{BaseApi}subscribers?{string.Join("&", queryParams)}";
                return await _httpClient.GetFromJsonAsync<SubscribersResponse>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscribers");
                return null;
            }
        }

        public async Task<BroadcastsResponse?> GetBroadcastsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                string url = $"{BaseApi}broadcasts?pageNumber={pageNumber}&pageSize={pageSize}";
                return await _httpClient.GetFromJsonAsync<BroadcastsResponse>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting broadcasts");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> SendBroadcastAsync(SendBroadcastModel model, IBrowserFile? file = null)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.Title.Trim()), "Title");
                content.Add(new StringContent(model.Message.Trim()), "Message");
                content.Add(new StringContent(model.TargetAudience.Trim()), "TargetAudience");

                if (file != null)
                {
                    // Allow up to 30MB file size
                    long maxAllowedSize = 30 * 1024 * 1024;
                    var stream = file.OpenReadStream(maxAllowedSize);
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
                    content.Add(fileContent, "Attachment", file.Name);
                }

                var response = await _httpClient.PostAsync($"{BaseApi}send", content);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Broadcast sent successfully!");
                }

                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Failed to send broadcast." : err);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending broadcast");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> SubscribeAsync(string email, string userType = "Customer")
        {
            try
            {
                var payload = new { Email = email.Trim(), UserType = userType };
                var response = await _httpClient.PostAsJsonAsync($"{BaseApi}subscribe", payload);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Subscribed successfully!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UnsubscribeAsync(string email)
        {
            try
            {
                var payload = new { Email = email.Trim() };
                var response = await _httpClient.PostAsJsonAsync($"{BaseApi}unsubscribe", payload);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Unsubscribed successfully!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteSubscriberAsync(long subscriberId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseApi}subscribers/{subscriberId}");
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Subscriber deleted successfully!");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}
