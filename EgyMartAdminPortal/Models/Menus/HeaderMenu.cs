#nullable disable
using EgyMartAdminPortal;
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class HeaderMenu
    {
        public long MenuItemID { get; set; }
        public string MenuItemTitle { get; set; }
        public long LangID { get; set; }

        [JsonPropertyName("basID")]
        public long BaseID { get; set; }
        public long ParentID { get; set; }
        public string TargetUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
