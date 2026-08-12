namespace EgyMartAdminPortal.Models.Dashoard
{
    public class AgeData
    {
        public int Age { get; set; }
        public long ViewsCount { get; set; }
    }
    public class ApiAgeResponse
    {
        public bool Success { get; set; }
        public string? ResponseArMsg { get; set; }
        public string? ResponseEngMsg { get; set; }
        public List<AgeData>? Data { get; set; }
        public int? ResponseId { get; set; }
    }
}
