// Areas/Admin/Controllers/ProductsController.cs
// Phần của Khoa: CRUD sản phẩm Admin
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Index(string? search, int? brandId, int? categoryId, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.Name.Contains(term) || (p.CPU != null && p.CPU.Contains(term)) || (p.RAM != null && p.RAM.Contains(term)));
                ViewBag.Search = term;
            }

            if (brandId.HasValue && brandId > 0)
            {
                query = query.Where(p => p.BrandId == brandId);
                ViewBag.BrandId = brandId;
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
                ViewBag.CategoryId = categoryId;
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", brandId);
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        // GET: /Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                product.Name = product.Name.Trim();
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Thêm sản phẩm «{product.Name}» thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", product.BrandId);
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", product.BrandId);
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Brand");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                product.Name = product.Name.Trim();
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cập nhật sản phẩm «{product.Name}» thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = new SelectList(await _context.Brands.OrderBy(b => b.Name).ToListAsync(), "Id", "Name", product.BrandId);
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: /Admin/Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem sản phẩm có trong đơn hàng nào không
            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            if (hasOrders)
            {
                TempData["Error"] = $"Không thể xóa sản phẩm «{product.Name}» vì đã tồn tại trong lịch sử đơn hàng!";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Xóa sản phẩm «{product.Name}» thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
