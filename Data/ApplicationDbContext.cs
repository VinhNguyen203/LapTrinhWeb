using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        //
        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    }
}