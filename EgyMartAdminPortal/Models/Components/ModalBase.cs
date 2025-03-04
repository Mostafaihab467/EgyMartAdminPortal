using Microsoft.AspNetCore.Components;

namespace EgyMartAdminPortal.Models.Components
{
    public class ModalBase : ComponentBase
    {
        protected bool IsVisible { get; set; }

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
    }
}
