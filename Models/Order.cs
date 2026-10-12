using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopShop.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? CustomerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ShippingAddress { get; set; }

        public DateTime? OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        public string? Status { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}