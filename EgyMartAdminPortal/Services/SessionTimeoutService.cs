using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace EgyMartAdminPortal.Services
{
    public class SessionTimeoutService(IJSRuntime jsRuntime, AuthService authService) : IDisposable
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly AuthService _authService = authService;
        private readonly LocalStorageService _localStorageService = new(jsRuntime);
        private DotNetObjectReference<SessionTimeoutService> _dotNetRef;

        private Timer _inactivityTimer;
        private Timer _heartbeatTimer;

        private DateTime? _tabHiddenTime;

        private const int InactivityLimitMinutes = 15;
        private const int HeartbeatIntervalMinutes = 10;

        private const string HeartbeatKey = "heartbeatTimestamp";
        private const string LastUrlKey = "lastVisitedUrl";

        public async Task InitializeAsync()
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            // Register JS events for user activity and visibility
            await _jsRuntime.InvokeVoidAsync("sessionTimeout.registerActivity", _dotNetRef);

            // Check last heartbeat (optional)
            var lastHeartbeatStr = await _localStorageService.GetItemAsync<string>(HeartbeatKey);
            if (DateTime.TryParse(lastHeartbeatStr, out var lastHeartbeat))
            {
                var diff = DateTime.UtcNow - lastHeartbeat;
                if (diff.TotalMinutes > InactivityLimitMinutes)
                {
                    await LogoutUserAsync();
                    return;
                }
            }

            StartHeartbeatTimer();
            StartInactivityTimer();
        }

        private void StartHeartbeatTimer()
        {
            _heartbeatTimer = new Timer(HeartbeatIntervalMinutes * 60 * 1000);
            _heartbeatTimer.Elapsed += async (s, e) => await UpdateHeartbeatAsync();
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();
        }

        private void StartInactivityTimer()
        {
            _inactivityTimer = new Timer(InactivityLimitMinutes * 60 * 1000);
            _inactivityTimer.Elapsed += async (s, e) => await LogoutUserAsync();
            _inactivityTimer.AutoReset = false;
            _inactivityTimer.Start();
        }

        [JSInvokable("ResetInactivityTimer")]
        public void ResetInactivityTimer()
        {
            _inactivityTimer?.Stop();
            _inactivityTimer?.Start();
        }

        [JSInvokable("OnVisibilityChange")]
        public async void OnVisibilityChange(string visibilityState)
        {
            if (visibilityState == "hidden")
            {
                _tabHiddenTime = DateTime.UtcNow;
            }
            else if (visibilityState == "visible")
            {
                if (_tabHiddenTime.HasValue)
                {
                    var hiddenDuration = DateTime.UtcNow - _tabHiddenTime.Value;
                    _tabHiddenTime = null;

                    if (hiddenDuration.TotalMinutes >= InactivityLimitMinutes)
                    {
                        await LogoutUserAsync();
                        return;
                    }
                }

                ResetInactivityTimer();
            }
        }

        private async Task UpdateHeartbeatAsync()
        {
            await _localStorageService.SetItemAsync(HeartbeatKey, DateTime.UtcNow);
        }

        private async Task LogoutUserAsync()
        {
            var currentUrl = await _jsRuntime.InvokeAsync<string>("sessionTimeout.getCurrentUrl");
            if (!currentUrl.Contains("login"))
            {
                await _localStorageService.SetItemAsync(LastUrlKey, currentUrl);
            }
            await _authService.LogoutAsync();
        }

        public void Dispose()
        {
            _heartbeatTimer?.Dispose();
            _inactivityTimer?.Dispose();
            _dotNetRef?.Dispose();
        }
    }
}
