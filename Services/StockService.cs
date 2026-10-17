
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Services
{
    public interface IStockService
    {
        Task<bool> DecreaseStockAsync(int productId, int quantity);
        Task<bool> IncreaseStockAsync(int productId, int quantity);
        Task<int> GetStockAsync(int productId);
        Task<bool> IsLowStockAsync(int productId, int threshold = 5);
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 5);
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
            if (quantity <= 0) return false;

            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity < quantity) return false;

            product.StockQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        // Tăng tồn kho (dùng khi nhập thêm hàng hoặc hủy đơn hàng)
        public async Task<bool> IncreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0) return false;

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.StockQuantity += quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy số lượng tồn kho của một sản phẩm
        public async Task<int> GetStockAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            return product?.StockQuantity ?? 0;
        }

        // Kiểm tra xem sản phẩm có sắp hết hàng hay không
        public async Task<bool> IsLowStockAsync(int productId, int threshold = 5)
        {
            var stock = await GetStockAsync(productId);
            return stock < threshold;
        }

        // Lấy danh sách toàn bộ sản phẩm sắp hết hàng
        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 5)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.StockQuantity < threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }
    }
}