using System.Security.Cryptography;
using System.Text;

namespace EgyMartAdminPortal.Handlers
{
    public class SignatureHandler : DelegatingHandler
    {
        private readonly string _secretKey = "YourSecretKey";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var message = $"{request.Method}:{request.RequestUri}:{timestamp}";
            var signature = GenerateSignature(_secretKey, message);

            request.Headers.Add("X-Timestamp", timestamp);
            request.Headers.Add("X-Signature", signature);

            return await base.SendAsync(request, cancellationToken);
        }

        private string GenerateSignature(string secretKey, string message)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash);
        }
    }

}
