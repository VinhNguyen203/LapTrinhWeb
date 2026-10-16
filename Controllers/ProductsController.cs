using Microsoft.AspNetCore.Mvc;
using LaptopShop.Models;

namespace LaptopShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách Laptop
        public IActionResult Index()
        {
            // Lấy toàn bộ Laptop từ Database truyền sang View
            var products = _context.Products.ToList();
            return View(products);
        }
    }
}