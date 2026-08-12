#nullable disable
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class Attributes
    {
        public long AttributeID { get; set; }
        public string AttributeTitle { get; set; }
        public int LangID { get; set; }
        public int AttributeTypeID { get; set; }

        [JsonPropertyName("BaseAttributeID")]
        public long BaseID { get; set; }
        public bool ShowAcrossAllCategory { get; set; }
        public bool IsActive { get; set; }
        public long? RcBy { get; set; } = 0;
    }
}
