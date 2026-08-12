#nullable disable
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class FAQ
    {
        public int Qid { get; set; }

        [Required(ErrorMessage = "Question Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string QTitle { get; set; }

        [Required(ErrorMessage = "Answer is required.")]
        public string QAnswer { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Display Order is required.")]
        public int DisplayOrder { get; set; }
        public long BaseID { get; set; }
        public int LangID { get; set; }
        public bool IsActive { get; set; }
    }
}
