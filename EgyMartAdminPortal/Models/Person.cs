namespace EgyMartAdminPortal.Models
{
    public class Person
    {
        public long UserID { get; set; }
        public short UserTypeID { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public DateTime RegisterdSince { get; set; }
        public bool IsVerfied { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool FirstLogin { get; set; } = false;
        public int FailLoginCount { get; set; } = 0;
        public bool IsDeleted { get; set; }
        public bool IsDeletable { get; set; }
        public DateTime RCDate { get; set; }
        public short RCBy { get; set; }
        public DateTime LADate { get; set; }
        public short LABy { get; set; }
        public long VerfiedBy { get; set; }
        public DateTime VerficationDate { get; set; }
        public short VerificationStatus { get; set; }
    }

    public class Supplier : Person
    {
        public long SupplierID
        {
            get => UserID;
            set => UserID = value;
        }
    }

    public class Customer : Person
    {
        public long CustomerID
        {
            get => UserID;
            set => UserID = value;
        }
    }
}
