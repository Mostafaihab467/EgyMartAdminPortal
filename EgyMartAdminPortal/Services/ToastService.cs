using Microsoft.JSInterop;
namespace EgyMartAdminPortal.Services
{
    public class ToastrService
    {
        private readonly IJSRuntime _jsRuntime;

        public ToastrService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ShowSuccessAsync(string message, string title = "")
        {
            await _jsRuntime.InvokeVoidAsync("toastr.success", message, title);
        }

        public async Task ShowErrorAsync(string message, string title = "")
        {
            await _jsRuntime.InvokeVoidAsync("toastr.error", message, title);
        }

        public async Task ShowInfoAsync(string message, string title = "")
        {
            await _jsRuntime.InvokeVoidAsync("toastr.info", message, title);
        }

        public async Task ShowWarningAsync(string message, string title = "")
        {
            await _jsRuntime.InvokeVoidAsync("toastr.warning", message, title);
        }
    }
}
