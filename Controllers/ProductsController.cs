// Controllers/ProductsController.cs
// Phần của Khoa: Danh sách sản phẩm client + tìm kiếm nâng cao + phân trang
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

namespace LaptopShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 8;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Products  (danh sách + lọc + tìm kiếm + phân trang)
        public async Task<IActionResult> Index(
            string? search,
            int? brandId,
            int? categoryId,
            string? ram,
            decimal? minPrice,
            decimal? maxPrice,
            string? priceRange,
            string? sort,
            int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // --- 1. Tìm kiếm từ khóa ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.Name.Contains(term) 
                    || (p.CPU != null && p.CPU.Contains(term))
                    || (p.GPU != null && p.GPU.Contains(term))
                    || (p.Brand != null && p.Brand.Name.Contains(term))
                    || (p.Category != null && p.Category.Name.Contains(term)));
            }

            // --- 2. Lọc theo hãng ---
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // --- 3. Lọc theo danh mục ---
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // --- 4. Lọc theo RAM ---
            if (!string.IsNullOrWhiteSpace(ram))
            {
                query = query.Where(p => p.RAM != null && p.RAM.Contains(ram.Trim()));
            }

            // --- 5. Lọc theo khoảng giá nhanh (priceRange) hoặc trực tiếp minPrice/maxPrice ---
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under15":
                        maxPrice = 15000000;
                        break;
                    case "15to25":
                        minPrice = 15000000;
                        maxPrice = 25000000;
                        break;
                    case "25to35":
                        minPrice = 25000000;
                        maxPrice = 35000000;
                        break;
                    case "above35":
                        minPrice = 35000000;
                        break;
                }
            }

            if (minPrice.HasValue && minPrice.Value > 0)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue && maxPrice.Value > 0)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // --- 6. Sắp xếp ---
            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc"   => query.OrderBy(p => p.Name),
                "name_desc"  => query.OrderByDescending(p => p.Name),
                _            => query.OrderByDescending(p => p.Id)   // Mới nhất
            };

            // --- 7. Phân trang ---
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Danh sách Hãng và Danh mục để hiển thị lên thanh lọc
            ViewBag.BrandsList     = await _context.Brands.OrderBy(b => b.Name).ToListAsync();
            ViewBag.CategoriesList = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            // Danh sách các loại RAM có trong DB
            var dbRamList = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.RAM))
                .Select(p => p.RAM!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
            ViewBag.RamList = dbRamList;

            // Lưu lại state hiện tại để bind vào View
            ViewBag.CurrentSearch     = search;
            ViewBag.CurrentBrandId    = brandId;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentRam        = ram;
            ViewBag.CurrentMinPrice   = minPrice;
            ViewBag.CurrentMaxPrice   = maxPrice;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentSort       = sort;
            ViewBag.CurrentPage       = page;
            ViewBag.TotalPages        = totalPages;
            ViewBag.TotalItems        = totalItems;

            return View(products);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Lấy 4 sản phẩm cùng Hãng hoặc cùng Danh mục làm sản phẩm gợi ý
            var related = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.Id != id && (p.BrandId == product.BrandId || p.CategoryId == product.CategoryId))
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = related;
            return View(product);
        }
    }
}