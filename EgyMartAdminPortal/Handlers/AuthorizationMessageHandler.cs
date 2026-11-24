using EgyMartAdminPortal.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace EgyMartAdminPortal.Handlers
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);

        private readonly AuthSession _session;
        private readonly AuthService _authService;
        private readonly Uri? _apiBase;

        public JwtAuthorizationMessageHandler(AuthSession session, AuthService authService, IConfiguration configuration)
        {
            _session = session;
            _authService = authService;
            var apiUrl = configuration["ApiUrl"];
            _apiBase = string.IsNullOrWhiteSpace(apiUrl) ? null : new Uri(apiUrl);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!IsApiRequest(request))
                return await base.SendAsync(request, cancellationToken);

            if (request.Content != null)
                await request.Content.LoadIntoBufferAsync();

            var sentJwt = await AttachAccessTokenAsync(request);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                response.Headers.TryGetValues("Token-Expired", out var tokenExpiredValues) &&
                tokenExpiredValues.Contains("true", StringComparer.OrdinalIgnoreCase))
            {
                bool refreshed;
                await RefreshLock.WaitAsync(cancellationToken);
                try
                {
                    refreshed = (!string.IsNullOrEmpty(_session.Jwt) && _session.Jwt != sentJwt)
                                || await _authService.TryRefreshTokenAsync();
                }
                finally
                {
                    RefreshLock.Release();
                }

                if (refreshed)
                {
                    response.Dispose();
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    await AttachAccessTokenAsync(clonedRequest);
                    return await base.SendAsync(clonedRequest, cancellationToken);
                }

                await _authService.LogoutAsync();
                return response;
            }

            return response;
        }

        private bool IsApiRequest(HttpRequestMessage request) =>
            _apiBase is null ||
            (request.RequestUri is { } uri &&
             Uri.Compare(uri, _apiBase, UriComponents.SchemeAndServer, UriFormat.Unescaped,
                         StringComparison.OrdinalIgnoreCase) == 0);

        private Task<string?> AttachAccessTokenAsync(HttpRequestMessage request)
        {
            request.Headers.Authorization = null;
            if (!string.IsNullOrEmpty(_session.Jwt))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Jwt);
            return Task.FromResult(_session.Jwt);
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
