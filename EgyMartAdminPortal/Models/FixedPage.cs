#nullable disable
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class FixedPage
    {
        public int PageID { get; set; }

        [Required(ErrorMessage = "Page Title is required.")]
        public string PageTitle { get; set; }

        [Required(ErrorMessage = "Page Body is required.")]
        public string PageBody { get; set; }
        public long BaseID { get; set; }
        public long LangID { get; set; }
        public bool IsActive { get; set; }
    }
}
