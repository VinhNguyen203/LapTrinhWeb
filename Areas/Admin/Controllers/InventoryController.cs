
using LaptopShop.Models;
using LaptopShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IStockService _stockService;

        public InventoryController(AppDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        // GET: Admin/Inventory
        public async Task<IActionResult> Index(string searchString, int? brandId)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            // Lọc theo hãng sản xuất
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            ViewBag.Brands = await _context.Brands.ToListAsync();
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.LowStockThreshold = 5; // Ngưỡng cảnh báo sắp hết hàng

            var products = await query.OrderBy(p => p.StockQuantity).ToListAsync();
            return View(products);
        }

        // POST: Admin/Inventory/QuickImport
        [HttpPost]
        public async Task<IActionResult> QuickImport(int productId, int addQuantity)
        {
            if (addQuantity > 0)
            {
                await _stockService.IncreaseStockAsync(productId, addQuantity);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}