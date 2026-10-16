// Controllers/HomeController.cs
// Phần của Khoa / Client chung
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy 8 sản phẩm nổi bật từ database
            var featuredProducts = _context.Products
                .OrderByDescending(p => p.Price)
                .Take(8)
                .ToList();

            return View(featuredProducts);
        }
    }
}