using System;
using System.ComponentModel.DataAnnotations;

namespace EgyMartAdminPortal.Models.Coupons
{
    public class CouponItem
    {
        public int CouponId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Percent";
        public decimal DiscountValue { get; set; }
        public decimal MinOrderTotal { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; } = "All";
        public DateTime CreatedAt { get; set; }

        public bool IsExpired => ExpireDate.HasValue && ExpireDate.Value < DateTime.UtcNow;
        public bool IsExhausted => MaxUses.HasValue && UsedCount >= MaxUses.Value;
        public bool IsGeneral => string.Equals(Type, "All", StringComparison.OrdinalIgnoreCase);

        public string StatusText
        {
            get
            {
                if (!IsActive) return "Inactive";
                if (IsExpired) return "Expired";
                if (IsExhausted) return "Exhausted";
                return "Active";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (!IsActive) return "bg-secondary";
                if (IsExpired) return "bg-danger";
                if (IsExhausted) return "bg-warning text-dark";
                return "bg-success";
            }
        }
    }

    public class CreateCouponModel
    {
        [Required(ErrorMessage = "Coupon code is required")]
        [StringLength(50, ErrorMessage = "Coupon code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required")]
        public string DiscountType { get; set; } = "Percent";

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, 1000000, ErrorMessage = "Discount value must be greater than zero")]
        public decimal DiscountValue { get; set; } = 10;

        [Range(0, 1000000, ErrorMessage = "Minimum order total cannot be negative")]
        public decimal MinOrderTotal { get; set; } = 0;

        public int? MaxUses { get; set; }

        public DateTime? ExpireDate { get; set; }

        // "All" or a vendor's UserId
        public string Type { get; set; } = "All";
        public long? SelectedVendorId { get; set; }
    }

    public class EditCouponModel
    {
        public int CouponId { get; set; }

        [Required(ErrorMessage = "Coupon code is required")]
        [StringLength(50, ErrorMessage = "Coupon code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required")]
        public string DiscountType { get; set; } = "Percent";

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, 1000000, ErrorMessage = "Discount value must be greater than zero")]
        public decimal DiscountValue { get; set; }

        [Range(0, 1000000, ErrorMessage = "Minimum order total cannot be negative")]
        public decimal MinOrderTotal { get; set; } = 0;

        public int? MaxUses { get; set; }

        public DateTime? ExpireDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
