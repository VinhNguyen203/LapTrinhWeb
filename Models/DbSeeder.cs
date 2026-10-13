using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // Đảm bảo database đã được tạo hoặc cập nhật mới nhất
            // context.Database.EnsureCreated();

            // Bạn có thể thêm dữ liệu mẫu (seed data) cho Brand, Category, Product hoặc Admin tại đây nếu cần.
            await Task.CompletedTask;
        }
    }
}