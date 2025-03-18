#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class FooterMenu
    {
        public int FooterItemID { get; set; }

        [Required(ErrorMessage = "Footer Item Title is required.")]
        public string FooterItemTitle { get; set; }
        public int LangID { get; set; }

        [JsonPropertyName("basID")]
        public int BaseID { get; set; }
        public int ParentID { get; set; }

        [Required(ErrorMessage = "Target URL is required.")]
        public string TargetUrl { get; set; }
        public bool IsActive { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Display Order is required.")]
        public int DisplayOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Column Index is required.")]
        public int ColumnIndex { get; set; }
    }
}
