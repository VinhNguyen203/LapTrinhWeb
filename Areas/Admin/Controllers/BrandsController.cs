// Areas/Admin/Controllers/BrandsController.cs
// Phần của Khoa: CRUD hãng Admin
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandsController : Controller
    {
        private readonly AppDbContext _context;

        public BrandsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Brands
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Brands
                .Include(b => b.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.Name.Contains(search));
                ViewBag.Search = search;
            }

            var brands = await query.OrderBy(b => b.Name).ToListAsync();
            return View(brands);
        }

        // GET: /Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                // Kiểm tra tên hãng trùng lặp
                var exists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == brand.Name.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "Tên hãng này đã tồn tại.");
                    return View(brand);
                }

                brand.Name = brand.Name.Trim();
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm hãng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: /Admin/Brands/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        // POST: /Admin/Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id) return NotFound();

            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                var exists = await _context.Brands.AnyAsync(b => b.Id != id && b.Name.ToLower() == brand.Name.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "Tên hãng này đã tồn tại.");
                    return View(brand);
                }

                brand.Name = brand.Name.Trim();
                _context.Brands.Update(brand);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật hãng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // POST: /Admin/Brands/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                TempData["Error"] = "Không tìm thấy hãng cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
            if (hasProducts)
            {
                TempData["Error"] = $"Không thể xóa hãng «{brand.Name}» vì đang có sản phẩm thuộc hãng này!";
                return RedirectToAction(nameof(Index));
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa hãng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
