using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace EgyMartAdminPortal.Services
{
    public class JwtAuthStateProvider(AuthSession session) : AuthenticationStateProvider
    {
        private readonly AuthSession _session = session;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrWhiteSpace(_session.Jwt) || IsExpired(_session.Jwt))
                return Task.FromResult(Anonymous());

            var identity = new ClaimsIdentity(ReadClaims(_session.Jwt), "jwt");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        public void Notify() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        private static AuthenticationState Anonymous() =>
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        private static bool IsExpired(string jwt)
        {
            var exp = ReadClaims(jwt).FirstOrDefault(c => c.Type == "exp")?.Value;
            if (!long.TryParse(exp, out var seconds))
                return true;
            var expiry = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return expiry <= DateTimeOffset.UtcNow.AddSeconds(30);
        }

        private static IEnumerable<Claim> ReadClaims(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return [];

            try
            {
                var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.EnumerateObject()
                    .SelectMany(p => ClaimsFor(p.Name, p.Value))
                    .ToList();
            }
            catch (FormatException)
            {
                return [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static IEnumerable<Claim> ClaimsFor(string name, JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    yield return new Claim(name, item.ToString());
                yield break;
            }

            yield return new Claim(name, value.ToString());
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
