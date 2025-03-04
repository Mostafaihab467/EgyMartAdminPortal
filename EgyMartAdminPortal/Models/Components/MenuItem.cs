namespace EgyMartAdminPortal.Models.Components
{
    public class MenuItem
    {
        public string Text { get; set; } = "";
        public string IconClass { get; set; } = "";
        public Action? OnClick { get; set; }
    }
}
