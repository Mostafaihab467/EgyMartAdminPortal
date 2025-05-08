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

        private const int InactivityLimitMinutes = 20;
        private const int HeartbeatIntervalMinutes = 3;

        private const string HeartbeatKey = "heartbeatTimestamp";

        public async Task InitializeAsync()
        {
            _dotNetRef = DotNetObjectReference.Create(this);

            await _jsRuntime.InvokeVoidAsync("sessionTimeout.registerActivity", _dotNetRef);

            var lastHeartbeatStr = await _localStorageService.GetItemAsync<string>(HeartbeatKey);
            if (DateTime.TryParse(lastHeartbeatStr, out var lastHeartbeat))
            {
                var diff = DateTime.UtcNow.Minute - lastHeartbeat.Minute;
                if (diff > InactivityLimitMinutes)
                {
                    await _authService.LogoutAsync();
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
            _inactivityTimer.Elapsed += async (s, e) => await _authService.LogoutAsync();
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
                        await _authService.LogoutAsync();
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

        public void Dispose()
        {
            _heartbeatTimer?.Dispose();
            _inactivityTimer?.Dispose();
            _dotNetRef?.Dispose();
        }
    }
}
