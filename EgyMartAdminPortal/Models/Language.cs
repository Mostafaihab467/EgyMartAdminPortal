#nullable disable
namespace EgyMartAdminPortal.Models
{
    public class Language
    {
        public int LangID { get; set; }
        public string LangTitle { get; set; }
        public string Direction { get; set; }
        //public string FlagClass { get; set; }
        public string FlagClass => GetFlagClass(LangTitle);

        private static string GetFlagClass(string code) => code switch
        {
            "English" => "flag-icon flag-icon-us", // Example: English (UK)
            "French" => "flag-icon flag-icon-fr", // Example: French
            "Arabic" => "flag-icon flag-icon-eg", // Example: Spanish
            _ => "flag-icon flag-icon-us"     // Default flag
        };
    }
}
