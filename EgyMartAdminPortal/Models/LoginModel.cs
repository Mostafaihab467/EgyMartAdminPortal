using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password required.")]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
    public class LoginData
    {
        public Person User { get; set; } = default!;
        public Tokens Tokens { get; set; } = default!;
    }

    public class Tokens
    {
        public string Jwt { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }

    public class NewToken
    {
        public string NewJWT { get; set; } = string.Empty;
    }
}
