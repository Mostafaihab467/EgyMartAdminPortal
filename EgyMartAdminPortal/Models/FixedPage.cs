#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class FixedPage
    {
        public int PageID { get; set; }
        public string PageTitle { get; set; }
        public string PageBody { get; set; }
        public long BaseID { get; set; }
        public long LangID { get; set; }
        public bool IsActive { get; set; }
    }
}
