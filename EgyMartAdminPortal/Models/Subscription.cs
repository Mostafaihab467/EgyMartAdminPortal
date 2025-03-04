#nullable disable
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class Subscription
    {
        public long AdsPlanID { get; set; }
        public string PlanTitle { get; set; }
        public int LocationID { get; set; }
        public int DurationDays { get; set; }
        public double CostBefore { get; set; }
        public double Cost { get; set; }
        public string PlanDescription { get; set; }
        public int LangID { get; set; }

        [JsonPropertyName("basePlanID")]
        public long BaseID { get; set; }
        public bool IsActive { get; set; }
        public string AdsLocationTitle { get; set; }
    }
}
