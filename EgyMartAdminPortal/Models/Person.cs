using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models
{
    public class Person
    {
        public long UserID { get; set; }
        public short UserTypeID { get; set; }

        [Required(ErrorMessage = "Please Enter Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please Enter Email")]
        [EmailAddress(ErrorMessage = "Email must be like this : ex12@example.com")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please Enter Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ProfileImage { get; set; } = string.Empty;
        public bool IsVerfied { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public bool FirstLogin { get; set; } = false;
        public int FailLoginCount { get; set; } = 0;
        public bool IsDeleted { get; set; }
        public bool IsDeletable { get; set; }
        public DateTime RegisterdSince { get; set; }
        public DateTime RCDate { get; set; }
        public long? RCBy { get; set; }
        public DateTime? LADate { get; set; }
        public long? LABy { get; set; }
        public int? VersionNo { get; set; }
        public long? VerfiedBy { get; set; }
        public DateTime? VerficationDate { get; set; }
        public int? VerificationStatus { get; set; } = 0; // 0: Not Verified, 1: Verified, 2: Rejected
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
