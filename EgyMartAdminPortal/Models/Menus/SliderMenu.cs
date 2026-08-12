#nullable disable
using EgyMartAdminPortal;
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class SliderMenu
    {
        public int SliderID { get; set; }

        [Required(ErrorMessage = "ShortTitle is required.")]
        public string ShortTitle { get; set; }
        public string MainTitle { get; set; }

        [Required(ErrorMessage = "Call2ActionMsg is required.")]
        public string Call2ActionMsg { get; set; }

        [Required(ErrorMessage = "Image is required.")]
        public string ImageURL { get; set; }

        [Required(ErrorMessage = "Call2ActionURL is required.")]
        public string Call2ActionURL { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Display Order is required.")]
        public int DisplayOrder { get; set; }
        public long BaseID { get; set; }
        public long LangID { get; set; }
        public bool IsActive { get; set; }
    }
}
