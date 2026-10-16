using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? AdminReply { get; set; }     // Nội dung phản hồi của Admin
        public DateTime? RepliedAt { get; set; }    // Ngày Admin phản hồi
    }
}