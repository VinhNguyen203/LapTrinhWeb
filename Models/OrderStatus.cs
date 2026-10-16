namespace LaptopShop.Models
{
    public enum OrderStatus
    {
        Pending = 0,     // Chờ xác nhận
        Confirmed = 1,   // Đã xác nhận
        Shipping = 2,    // Đang giao hàng
        Delivered = 3,   // Đã giao hàng
        Cancelled = 4    // Đã hủy
    }
}