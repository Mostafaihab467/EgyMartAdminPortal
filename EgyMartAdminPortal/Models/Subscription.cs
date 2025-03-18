#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EgyMartAdminPortal.Models
{
    public class Subscription
    {
        public long AdsPlanID { get; set; }

        [Required(ErrorMessage = "Plan Title is required.")]
        public string PlanTitle { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Plan Location.")]
        public int LocationID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration Days must be greater than 0.")]
        public int DurationDays { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost Before must be non-negative.")]
        public double CostBefore { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative.")]
        [Compare(nameof(CostBefore), ErrorMessage = "Cost must be less than Cost Before.")]
        public double Cost { get; set; }
        public string PlanDescription { get; set; }
        public int LangID { get; set; }

        [JsonPropertyName("basePlanID")]
        public long BaseID { get; set; }
        public bool IsActive { get; set; }
        public string AdsLocationTitle { get; set; }
    }
}
