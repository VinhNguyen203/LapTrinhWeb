using System.ComponentModel.DataAnnotations;

namespace LaptopShop.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(
            @"^\+?[0-9]{9,15}$",
            ErrorMessage = "Số điện thoại cần có 9–15 chữ số.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;
        [Required]
        [RegularExpression(
        "^(COD|QR)$",
         ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        public string PaymentMethod { get; set; } = "COD";

        public string? CouponCode { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        [StringLength(500)]
        public string? OrderNotes { get; set; }
    }
}
