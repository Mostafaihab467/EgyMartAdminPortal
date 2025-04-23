using EgyMartAdminPortal.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace EgyMartAdminPortal.Handlers
{
    public class SignatureHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly string _ivBase64 = "UTlGSkt1eUZESk96aVhhbw==";

        public SignatureHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userData");
            string email = "";
            string userID = "";

            if (string.IsNullOrEmpty(userJson) && request.Content != null)
            {
                var bodyContent = await request.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(bodyContent);
                if (doc.RootElement.TryGetProperty("userName", out var emailElement))
                {
                    email = emailElement.GetString() ?? "";
                }
                if (doc.RootElement.TryGetProperty("userID", out var userIDElement))
                {
                    userID = userIDElement.GetString() ?? "";
                }
            }
            else if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    var user = JsonSerializer.Deserialize<Person>(userJson);
                    email = user?.UserName ?? "";
                    userID = user?.UserID.ToString() ?? "";
                }
                catch
                {
                    email = "Anonymous";
                }
            }

            var endpoint = request.RequestUri?.AbsolutePath ?? "/";
            var body = $"{userID}@{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}";
            var signature = await _jsRuntime.InvokeAsync<string>(
                "cryptoHelper.signRequest",
                email,
                endpoint,
                body,
                _ivBase64
            );

            request.Headers.Add("sign", signature);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
