// Controllers/HomeController.cs
// Phần của Khoa: Trang chủ Client - load sản phẩm thật từ DB
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

namespace LaptopShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy 8 sản phẩm mới nhất để hiện trên trang chủ
            var featuredProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            // Lấy danh sách hãng hàng đầu cho trang chủ (sắp xếp an toàn trong bộ nhớ)
            var allBrands = await _context.Brands
                .Include(b => b.Products)
                .ToListAsync();
            ViewBag.TopBrands = allBrands
                .OrderByDescending(b => b.Products?.Count ?? 0)
                .Take(6)
                .ToList();

            // Lấy danh mục hàng đầu
            var allCategories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            ViewBag.TopCategories = allCategories
                .OrderByDescending(c => c.Products?.Count ?? 0)
                .Take(6)
                .ToList();

            return View(featuredProducts);
        }
    }
}