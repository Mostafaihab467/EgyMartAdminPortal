#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class UserData
    {
        public long userID { get; set; }
        public string displayName { get; set; }
        public string profileImage { get; set; }
        public bool IsVerfied { get; set; } = false;
        public bool firstLogin { get; set; } = true;
        public int failLoginCount { get; set; } = 0;
    }
}
