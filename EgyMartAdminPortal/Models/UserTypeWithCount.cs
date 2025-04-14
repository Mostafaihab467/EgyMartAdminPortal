namespace EgyMartAdminPortal.Models
{
    public class UserTypeWithCount
    {
        public short UserTypeID { get; set; }
        public string UserTypeTitle { get; set; } = string.Empty;
        public int UsersCount { get; set; }
    }
}
