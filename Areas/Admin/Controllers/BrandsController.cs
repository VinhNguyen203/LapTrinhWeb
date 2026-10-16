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
    /// Controller Admin quản lý Hãng sản xuất / Thương hiệu (CRUD Brands).
    /// Kế thừa AdminBaseController để bảo vệ phiên đăng nhập Admin.
    /// Phụ trách: Khoa (Danh mục sản phẩm & Tìm kiếm nâng cao)
    /// </summary>
    [Area("Admin")]
    public class BrandsController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public BrandsController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH HÃNG SẢN XUẤT
        // ===========================
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null)
        {
            var query = _context.Brands
                .Include(b => b.Products)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(search));
            }

            query = query.OrderByDescending(b => b.Id);

            var paginatedBrands = await PaginatedList<Brand>.CreateAsync(query, pageNumber, PageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedBrands);
        }

        // ===========================
        // THÊM MỚI HÃNG SẢN XUẤT
        // ===========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Brand());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                ModelState.AddModelError("Name", "Tên thương hiệu không được để trống.");
            }
            else
            {
                var trimmedName = brand.Name.Trim();
                var exists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == trimmedName.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", $"Hãng \"{trimmedName}\" đã tồn tại trong hệ thống.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    brand.Name = brand.Name.Trim();
                    _context.Brands.Add(brand);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm hãng \"{brand.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi thêm hãng: {ex.Message}";
                }
            }

            return View(brand);
        }

        // ===========================
        // CHỈNH SỬA HÃNG SẢN XUẤT
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hãng sản xuất yêu cầu.";
                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                ModelState.AddModelError("Name", "Tên thương hiệu không được để trống.");
            }
            else
            {
                var trimmedName = brand.Name.Trim();
                var exists = await _context.Brands.AnyAsync(b => b.Id != id && b.Name.ToLower() == trimmedName.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", $"Hãng \"{trimmedName}\" đã tồn tại trên hệ thống.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    brand.Name = brand.Name.Trim();
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật hãng \"{brand.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BrandExists(brand.Id))
                    {
                        TempData["ErrorMessage"] = "Hãng sản xuất không còn tồn tại.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật hãng: {ex.Message}";
                }
            }

            return View(brand);
        }

        // ===========================
        // XÓA HÃNG SẢN XUẤT
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hãng sản xuất.";
                return RedirectToAction(nameof(Index));
            }

            var productCount = brand.Products?.Count ?? 0;
            ViewBag.ProductCount = productCount;

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                TempData["ErrorMessage"] = "Hãng sản xuất không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var productCount = await _context.Products.CountAsync(p => p.BrandId == id);
            if (productCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa hãng \"{brand.Name}\" vì đang có {productCount} sản phẩm liên kết. Vui lòng chuyển hãng hoặc xóa sản phẩm trước!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa hãng \"{brand.Name}\" thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa hãng: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BrandExists(int id)
        {
            return await _context.Brands.AnyAsync(e => e.Id == id);
        }
    }
}
