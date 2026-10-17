using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Models
{
    /// <summary>
    /// DbContext chính của ứng dụng LaptopShop
    /// Quản lý kết nối với cơ sở dữ liệu SQL Server
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ========================
        // Các bảng dữ liệu chính
        // ========================

        /// <summary>Bảng thương hiệu sản phẩm</summary>
        public DbSet<Brand> Brands { get; set; }

        /// <summary>Bảng danh mục sản phẩm</summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>Bảng sản phẩm</summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>Bảng đơn hàng</summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>Bảng chi tiết đơn hàng</summary>
        public DbSet<OrderDetail> OrderDetails { get; set; }

        /// <summary>Bảng đánh giá sản phẩm</summary>
        public DbSet<Review> Reviews { get; set; }

        /// <summary>Bảng người dùng hệ thống</summary>
        public DbSet<AppUser> Users { get; set; }

        /// <summary>Bảng mã giảm giá & khuyến mãi</summary>
        public DbSet<Coupon> Coupons { get; set; }

        /// <summary>Bảng giỏ hàng của khách hàng</summary>
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Users
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                // Email phải là duy nhất
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("Customer");
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
