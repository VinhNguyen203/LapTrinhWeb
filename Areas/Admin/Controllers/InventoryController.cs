
using LaptopShop.Controllers;
using LaptopShop.Models;
using LaptopShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private readonly IStockService _stockService;

        public InventoryController(AppDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        // GET: Admin/Inventory
        public async Task<IActionResult> Index(string searchString, int? brandId, string stockStatus = "all")
        {
            // 1. Thống kê KPI tồn kho
            var allProducts = await _context.Products.ToListAsync();
            ViewBag.TotalProducts = allProducts.Count;
            ViewBag.TotalStockUnits = allProducts.Sum(p => p.StockQuantity);
            ViewBag.LowStockCount = allProducts.Count(p => p.StockQuantity < 5 && p.StockQuantity > 0);
            ViewBag.OutOfStockCount = allProducts.Count(p => p.StockQuantity == 0);

            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.Trim();
                query = query.Where(p => p.Name.Contains(term));
            }

            // Lọc theo hãng sản xuất
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Lọc theo trạng thái tồn kho
            switch (stockStatus?.ToLower())
            {
                case "low":
                    query = query.Where(p => p.StockQuantity < 5 && p.StockQuantity > 0);
                    break;
                case "out":
                    query = query.Where(p => p.StockQuantity == 0);
                    break;
                case "available":
                    query = query.Where(p => p.StockQuantity >= 5);
                    break;
                default:
                    stockStatus = "all";
                    break;
            }

            ViewBag.Brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentStockStatus = stockStatus;
            ViewBag.LowStockThreshold = 5; // Ngưỡng cảnh báo sắp hết hàng

            var products = await query.OrderBy(p => p.StockQuantity).ToListAsync();
            return View(products);
        }

        // POST: Admin/Inventory/QuickImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickImport(int productId, int addQuantity)
        {
            if (productId <= 0 || addQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng nhập thêm phải lớn hơn 0!";
                return RedirectToAction(nameof(Index));
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần nhập kho!";
                return RedirectToAction(nameof(Index));
            }

            var success = await _stockService.IncreaseStockAsync(productId, addQuantity);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã nhập thêm thành công {addQuantity} máy cho '{product.Name}'. Tồn kho hiện tại: {product.StockQuantity}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình cập nhật tồn kho!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}