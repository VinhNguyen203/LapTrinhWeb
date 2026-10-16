using System;
using System.Linq;
using System.Threading.Tasks;
using LaptopShop.Controllers;
using LaptopShop.Helpers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller Admin quản lý Danh mục sản phẩm (CRUD Categories).
    /// Kế thừa AdminBaseController để bảo vệ phiên đăng nhập Admin.
    /// Phụ trách: Khoa (Danh mục sản phẩm & Tìm kiếm nâng cao)
    /// </summary>
    [Area("Admin")]
    public class CategoriesController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH DANH MỤC
        // ===========================
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null)
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search));
            }

            query = query.OrderByDescending(c => c.Id);

            var paginatedCategories = await PaginatedList<Category>.CreateAsync(query, pageNumber, PageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedCategories);
        }

        // ===========================
        // THÊM MỚI DANH MỤC
        // ===========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục không được để trống.");
            }
            else
            {
                var trimmedName = category.Name.Trim();
                var exists = await _context.Categories.AnyAsync(c => c.Name.ToLower() == trimmedName.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", $"Danh mục \"{trimmedName}\" đã tồn tại trong hệ thống.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = category.Name.Trim();
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm danh mục \"{category.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi thêm danh mục: {ex.Message}";
                }
            }

            return View(category);
        }

        // ===========================
        // CHỈNH SỬA DANH MỤC
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục yêu cầu.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục không được để trống.");
            }
            else
            {
                var trimmedName = category.Name.Trim();
                var exists = await _context.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == trimmedName.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", $"Danh mục \"{trimmedName}\" đã tồn tại trên hệ thống.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Name = category.Name.Trim();
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật danh mục \"{category.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExists(category.Id))
                    {
                        TempData["ErrorMessage"] = "Danh mục không còn tồn tại.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật danh mục: {ex.Message}";
                }
            }

            return View(category);
        }

        // ===========================
        // XÓA DANH MỤC
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            var productCount = category.Products?.Count ?? 0;
            ViewBag.ProductCount = productCount;

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Danh mục không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var productCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa danh mục \"{category.Name}\" vì đang có {productCount} sản phẩm liên kết. Vui lòng chuyển danh mục hoặc xóa các sản phẩm đó trước!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa danh mục \"{category.Name}\" thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa danh mục: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }
    }
}
