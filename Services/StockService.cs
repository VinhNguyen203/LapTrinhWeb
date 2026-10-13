
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Services
{
    public interface IStockService
    {
        Task<bool> DecreaseStockAsync(int productId, int quantity);
        Task<bool> IncreaseStockAsync(int productId, int quantity);
    }

    public class StockService : IStockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        // Giảm tồn kho (dùng khi khách đặt hàng thành công)
        public async Task<bool> DecreaseStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity < quantity) return false;

            product.StockQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        // Tăng tồn kho (dùng khi nhập thêm hàng hoặc hủy đơn hàng)
        public async Task<bool> IncreaseStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantity;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}