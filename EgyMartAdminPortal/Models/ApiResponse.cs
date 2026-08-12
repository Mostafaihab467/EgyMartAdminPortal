#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string ResponseArMsg { get; set; }
        public string ResponseEngMsg { get; set; }
        public T Data { get; set; }
        public int ResponseId { get; set; }
    }
}
