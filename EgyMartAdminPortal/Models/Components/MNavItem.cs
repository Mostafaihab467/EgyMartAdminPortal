#nullable disable
namespace EgyMartAdminPortal.Models.Components
{
    public class MNavItem
    {
        public int ID { get; set; }
        public int ParentID { get; set; }
        public string Href { get; set; }
        public string IconName { get; set; }
        public string Text { get; set; }
    }
}
