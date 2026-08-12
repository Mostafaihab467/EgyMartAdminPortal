#nullable disable
using EgyMartAdminPortal;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class HeaderMenu
    {
        public long MenuItemID { get; set; }

        [Required(ErrorMessage = "Menu Item Title is required.")]
        public string MenuItemTitle { get; set; }
        public long LangID { get; set; }

        [JsonPropertyName("basID")]
        public long BaseID { get; set; }
        public long ParentID { get; set; }

        [Required(ErrorMessage = "Target URL is required.")]
        public string TargetUrl { get; set; }
        public bool IsActive { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Display Order is required.")]
        public int DisplayOrder { get; set; }
    }
}
