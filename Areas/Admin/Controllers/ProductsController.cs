using System;
using System.Linq;
using System.Threading.Tasks;
using LaptopShop.Controllers;
using LaptopShop.Helpers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller Admin quản lý Sản phẩm (CRUD).
    /// Kế thừa AdminBaseController để bảo vệ phiên đăng nhập Admin.
    /// </summary>
    [Area("Admin")]
    public class ProductsController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH SẢN PHẨM
        // ===========================
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string? searchString = null,
            int? brandId = null,
            int? categoryId = null,
            string? sortBy = null)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            // Tìm kiếm theo tên sản phẩm hoặc thông số (CPU, GPU, RAM)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                         (p.CPU != null && p.CPU.ToLower().Contains(search)) ||
                                         (p.GPU != null && p.GPU.ToLower().Contains(search)) ||
                                         (p.RAM != null && p.RAM.ToLower().Contains(search)));
            }

            // Lọc theo hãng
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sắp xếp
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "stock_asc" => query.OrderBy(p => p.StockQuantity),
                "stock_desc" => query.OrderByDescending(p => p.StockQuantity),
                _ => query.OrderByDescending(p => p.Id)
            };

            var paginatedProducts = await PaginatedList<Product>.CreateAsync(query, pageNumber, PageSize);

            // Dữ liệu cho các dropdown lọc
            ViewBag.Brands = await _context.Brands.AsNoTracking().ToListAsync();
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedProducts);
        }

        // ===========================
        // THÊM MỚI SẢN PHẨM
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View(new Product { StockQuantity = 10 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Tên sản phẩm không được để trống.");
            }

            if (product.Price < 0)
            {
                ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn hoặc bằng 0.");
            }

            if (product.StockQuantity < 0)
            {
                ModelState.AddModelError("StockQuantity", "Số lượng tồn kho không được âm.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm sản phẩm \"{product.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi lưu sản phẩm: {ex.Message}";
                }
            }

            await LoadDropdownData(product.BrandId, product.CategoryId);
            return View(product);
        }

        // ===========================
        // CẬP NHẬT SẢN PHẨM
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm yêu cầu.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(product.BrandId, product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError("Name", "Tên sản phẩm không được để trống.");
            }

            if (product.Price < 0)
            {
                ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn hoặc bằng 0.");
            }

            if (product.StockQuantity < 0)
            {
                ModelState.AddModelError("StockQuantity", "Số lượng tồn kho không được âm.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật sản phẩm \"{product.Name}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                    {
                        TempData["ErrorMessage"] = "Sản phẩm không còn tồn tại trên hệ thống.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
                }
            }

            await LoadDropdownData(product.BrandId, product.CategoryId);
            return View(product);
        }

        // ===========================
        // XÓA SẢN PHẨM
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem sản phẩm có trong đơn hàng nào không
            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            ViewBag.HasOrders = hasOrders;

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = $"Không thể xóa sản phẩm \"{product.Name}\" vì đã có trong đơn hàng của khách. Hãy giảm tồn kho về 0 thay vì xóa!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Xóa các đánh giá liên quan (nếu có) để tránh lỗi khóa ngoại FK_Reviews_Products
                var relatedReviews = await _context.Reviews.Where(r => r.ProductId == id).ToListAsync();
                if (relatedReviews.Any())
                {
                    _context.Reviews.RemoveRange(relatedReviews);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm \"{product.Name}\" thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // HÀM HỖ TRỢ
        // ===========================
        private async Task LoadDropdownData(int? selectedBrandId = null, int? selectedCategoryId = null)
        {
            var brands = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.BrandId = new SelectList(brands, "Id", "Name", selectedBrandId);
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
    }
}
