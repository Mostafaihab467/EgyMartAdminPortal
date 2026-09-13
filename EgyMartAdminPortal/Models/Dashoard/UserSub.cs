namespace EgyMartAdminPortal.Models.Dashoard
{
    public class UserSub
    {
        public long UserID { get; set; }
        public string? DisplayName { get; set; }
        public long PlanID { get; set; }
        public string? PlanTitle { get; set; } = "Plan";
        public long? SubscriptionID { get; set; }
        public DateTime EndDate { get; set; }
    }
}
