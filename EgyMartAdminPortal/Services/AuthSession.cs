using EgyMartAdminPortal.Models;

namespace EgyMartAdminPortal.Services
{
    /// <summary>
    /// Access and refresh tokens for this browser tab. They stay in memory so page scripts cannot read them from localStorage.
    /// </summary>
    public sealed class AuthSession
    {
        public string? Jwt { get; private set; }
        public string? RefreshToken { get; private set; }
        public Person User { get; private set; } = new();

        public void Set(string? jwt, string? refreshToken, Person? user)
        {
            Jwt = string.IsNullOrWhiteSpace(jwt) ? null : jwt;
            RefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken;
            if (user != null)
                User = user;
        }

        public void Clear()
        {
            Jwt = null;
            RefreshToken = null;
            User = new Person();
        }
    }
}
