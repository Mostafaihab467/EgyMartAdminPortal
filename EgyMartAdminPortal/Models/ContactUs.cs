#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class ContactUs
    {
        public int ContactUsID { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int LangID { get; set; }
        public int BaseID { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
