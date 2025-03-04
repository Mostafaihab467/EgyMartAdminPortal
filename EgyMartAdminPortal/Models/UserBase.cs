namespace EgyMartAdminPortal.Models
{
    public abstract class UserBase
    {
        public long ID { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RegisterdSince { get; set; }
        public bool IsVerified { get; set; }
        public bool IsDeleting { get; set; }
    }

    public class Supplier : UserBase
    {
        public long SupplierID
        {
            get => ID;
            set => ID = value;
        }
    }

    public class Customer : UserBase
    {
        public long CustomerID
        {
            get => ID;
            set => ID = value;
        }
    }
}
