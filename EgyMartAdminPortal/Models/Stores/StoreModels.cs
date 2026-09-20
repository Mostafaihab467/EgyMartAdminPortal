using System;
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models.Stores
{
    public class StoreItem
    {
        public long StoreId { get; set; }
        public long VendorUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; }
        public long? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int VerifiedProductCount { get; set; }
        public string? VendorName { get; set; }
    }

    public class CreateStoreModel
    {
        [Required(ErrorMessage = "Store name is required")]
        [StringLength(150, ErrorMessage = "Store name cannot exceed 150 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line is required")]
        [StringLength(300, ErrorMessage = "Address line cannot exceed 300 characters")]
        public string AddressLine { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Latitude cannot exceed 100 characters")]
        public string? Latitude { get; set; }

        [StringLength(100, ErrorMessage = "Longitude cannot exceed 100 characters")]
        public string? Longitude { get; set; }

        [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters")]
        public string? Phone { get; set; }

        public long? VendorUserId { get; set; }
    }

    public class EditStoreModel : CreateStoreModel
    {
        public long StoreId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class StoreProductItem
    {
        public long StoreProductId { get; set; }
        public long StoreId { get; set; }
        public long ProductId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public long? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public long VendorUserId { get; set; }
        public string? ProductTitle { get; set; }
        public decimal CostAfter { get; set; }
    }
}
