#nullable disable
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class FooterMenu
    {
        public int FooterItemID { get; set; }
        public string FooterItemTitle { get; set; }
        public int LangID { get; set; }

        [JsonPropertyName("basID")]
        public int BaseID { get; set; }
        public int ParentID { get; set; }
        public string TargetUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ColumnIndex { get; set; }
    }
}
