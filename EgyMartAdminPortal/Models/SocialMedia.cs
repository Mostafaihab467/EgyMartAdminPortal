#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class SocialMedia
    {
        public short LinkID { get; set; }
        public string LinkTitle { get; set; }
        public string LinkTarget { get; set; }
        public int DisplayOrder { get; set; }
        public string LinkIcon { get; set; }
        public bool IsActive { get; set; }
    }
}
