using System;
using System.Linq;
using System.Threading.Tasks;
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LaptopShop.Data
{
    /// <summary>
    /// Khởi tạo dữ liệu mẫu ban đầu cho hệ thống (Admin & Khách hàng mẫu)
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DbSeeder");

            try
            {
                // Kiểm tra xem bảng Users đã có dữ liệu chưa
                if (!await context.Users.AnyAsync())
                {
                    logger?.LogInformation("Bắt đầu khởi tạo tài khoản mặc định...");

                    var adminUser = new AppUser
                    {
                        FullName = "Quản trị viên Hệ thống",
                        Email = "admin@laptopshop.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        PhoneNumber = "0987654321",
                        Address = "Trụ sở chính LaptopShop, Hà Nội",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    var customerUser = new AppUser
                    {
                        FullName = "Nguyễn Văn Khách",
                        Email = "customer@laptopshop.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                        PhoneNumber = "0912345678",
                        Address = "123 Cầu Giấy, Hà Nội",
                        Role = "Customer",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    await context.Users.AddRangeAsync(adminUser, customerUser);
                    await context.SaveChangesAsync();

                    logger?.LogInformation("Đã khởi tạo thành công 2 tài khoản mặc định (Admin & Customer).");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Lỗi xảy ra trong quá trình seed dữ liệu.");
            }
        }
    }
}
