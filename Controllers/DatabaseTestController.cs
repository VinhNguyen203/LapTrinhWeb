using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class DatabaseTestController : Controller
    {
        private readonly AppDbContext _context;

        public DatabaseTestController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Take(5)
                .ToListAsync();

            return View(products);
        }
    }
}