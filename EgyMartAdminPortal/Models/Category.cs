#nullable disable
using EgyMartAdminPortal.Handlers;
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class Category
    {
        public long CategoryID { get; set; }

        [Required(ErrorMessage = "Category Title is required.")]
        public string CategoryTitle { get; set; }
        public long ParentID { get; set; }
        public int LangID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Display Order is required.")]
        public int DisplayOrder { get; set; }
        public string CategoryImageURL { get; set; }
        public bool IsActive { get; set; }
    }
}
