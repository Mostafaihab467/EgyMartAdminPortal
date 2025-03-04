#nullable disable
using EgyMartAdminPortal;

namespace EgyMartAdminPortal.Models
{
    public class SliderMenu
    {
        public int SliderID { get; set; }
        public string ShortTitle { get; set; }
        public string MainTitle { get; set; }
        public string Call2ActionMsg { get; set; }
        public string ImageURL { get; set; }
        public string Call2ActionURL { get; set; }
        public int DisplayOrder { get; set; }
        public long BaseID { get; set; }
        public long LangID { get; set; }
        public bool IsActive { get; set; }
    }
}
