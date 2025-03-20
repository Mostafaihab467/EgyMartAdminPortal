#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class Attributes
    {
        public long AttributeID { get; set; }
        public long BaseAttributeID { get; set; }
        public string AttributeTitle { get; set; }
        public int AttributeTypeID { get; set; }
        public bool ShowAcrossAllCategory { get; set; }
        public int LangID { get; set; }
        public long RcBy { get; set; }
        public bool IsActive { get; set; }
    }
}
