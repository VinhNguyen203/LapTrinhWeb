using LaptopShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class DatabaseTestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseTestController(ApplicationDbContext context)
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