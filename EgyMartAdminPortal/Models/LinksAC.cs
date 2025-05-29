#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class LinksAC
    {
        public long CategoryID { get; set; }
        public string CategoryTitle { get; set; }
        public long ParentID { get; set; } = 0;
        public int LangID { get; set; }
        public int DisplayOrder { get; set; }
        public string CategoryImageURL { get; set; }
        public bool IsActive { get; set; }
        public long? LinkID { get; set; }
    }
}
