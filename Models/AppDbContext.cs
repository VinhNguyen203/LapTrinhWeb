using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Models
{
    public class AppDbContext : DbContext
    {
        // Constructor bắt buộc để nhận cấu hình kết nối từ Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng sẽ có trong CSDL (Khớp với SQL Server)
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        // Phần bảng của Hưng phụ trách
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}