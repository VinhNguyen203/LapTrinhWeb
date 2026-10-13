// Areas/Admin/Controllers/CategoriesController.cs
// Phần của Khoa: CRUD danh mục Admin
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
                ViewBag.Search = search;
            }

            var categories = await query.OrderBy(c => c.Name).ToListAsync();
            return View(categories);
        }

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.Name.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
                    return View(category);
                }

                category.Name = category.Name.Trim();
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            ModelState.Remove("Products");
            if (ModelState.IsValid)
            {
                var exists = await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == category.Name.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
                    return View(category);
                }

                category.Name = category.Name.Trim();
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: /Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = $"Không thể xóa danh mục «{category.Name}» vì đang có sản phẩm thuộc danh mục này!";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
