namespace EgyMartAdminPortal.Models.Dashoard
{
    public class PlanData
    {
        public long PlanId { get; set; }
        public string? PlanTitle { get; set; } = "Plan";
        public long PlansCount { get; set; }
        public decimal TotalCost { get; set; }
    }
}
