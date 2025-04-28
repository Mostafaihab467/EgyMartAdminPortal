using Microsoft.JSInterop;
using System.Timers;
using Timer = System.Timers.Timer;

namespace EgyMartAdminPortal.Services
{
    public class SessionTimeoutService(IJSRuntime jsRuntime, AuthService authService) : IDisposable
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        private readonly AuthService _authService = authService;
        private readonly LocalStorageService _localStorageService = new LocalStorageService(jsRuntime);
        private DotNetObjectReference<SessionTimeoutService> _dotNetRef;

        private Timer _inactivityTimer;
        private Timer _heartbeatTimer;

        private const int InactivityLimitMinutes = 10;
        private const int HeartbeatIntervalMinutes = 1;

        private const string HeartbeatKey = "heartbeatTimestamp";

        public async Task InitializeAsync()
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            // Register JS events for user activity
            await _jsRuntime.InvokeVoidAsync("sessionTimeout.registerActivity", _dotNetRef);

            // Check last heartbeat
            var lastHeartbeatStr = await _localStorageService.GetItemAsync<string>(HeartbeatKey);
            if (DateTime.TryParse(lastHeartbeatStr, out var lastHeartbeat))
            {
                var diff = DateTime.UtcNow - lastHeartbeat;
                if (diff.TotalMinutes > InactivityLimitMinutes)
                {
                    // Last heartbeat older than 10 min = logout immediately
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
        public void OnVisibilityChange(string visibilityState)
        {
            if (visibilityState == "visible")
            {
                // Reset inactivity timer if user came back
                ResetInactivityTimer();
            }
        }

        private async Task UpdateHeartbeatAsync()
        {
            await _localStorageService.SetItemAsync(HeartbeatKey, DateTime.UtcNow);
        }

        private async Task LogoutUserAsync()
        {
            await _authService.LogoutAsync();
            // Optionally redirect to login page (use NavigationManager or event)
        }

        public void Dispose()
        {
            _heartbeatTimer?.Dispose();
            _inactivityTimer?.Dispose();
            _dotNetRef?.Dispose();
        }
    }
}
