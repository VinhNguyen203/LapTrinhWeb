using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Helpers;
using LaptopShop.Models;

namespace LaptopShop.Controllers
{
    /// <summary>
    /// Controller hiển thị danh sách sản phẩm, bộ lọc tìm kiếm nâng cao và chi tiết sản phẩm phía khách hàng.
    /// Phụ trách: Khoa (Danh mục sản phẩm & Tìm kiếm nâng cao)
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 8; // 8 sản phẩm mỗi trang trên lưới hiển thị

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DANH SÁCH SẢN PHẨM & BỘ LỌC TÌM KIẾM NÂNG CAO
        // ==========================================
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? brandId,
            int? categoryId,
            string? ram,
            string? priceRange,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            int pageNumber = 1)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            // 1. Tìm kiếm theo từ khóa (tên laptop, CPU, GPU, RAM)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                         (p.CPU != null && p.CPU.ToLower().Contains(term)) ||
                                         (p.GPU != null && p.GPU.ToLower().Contains(term)) ||
                                         (p.RAM != null && p.RAM.ToLower().Contains(term)));
            }

            // 2. Lọc theo hãng / thương hiệu
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // 3. Lọc theo danh mục sản phẩm
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // 4. Lọc theo dung lượng RAM (8GB, 16GB, 32GB...)
            if (!string.IsNullOrWhiteSpace(ram))
            {
                var ramVal = ram.Trim().ToLower();
                query = query.Where(p => p.RAM != null && p.RAM.ToLower().Contains(ramVal));
            }

            // 5. Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                query = priceRange switch
                {
                    "under-15" => query.Where(p => p.Price < 15000000),
                    "15-25" => query.Where(p => p.Price >= 15000000 && p.Price <= 25000000),
                    "25-35" => query.Where(p => p.Price > 25000000 && p.Price <= 35000000),
                    "over-35" => query.Where(p => p.Price > 35000000),
                    _ => query
                };
            }
            else
            {
                if (minPrice.HasValue && minPrice.Value > 0)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }
                if (maxPrice.HasValue && maxPrice.Value > 0)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }
            }

            // 6. Sắp xếp kết quả
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.Id) // Mặc định: Mới nhất
            };

            // Phân trang
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(query, pageNumber, PageSize);

            // Dữ liệu hỗ trợ giao diện bộ lọc
            ViewBag.Brands = await _context.Brands.AsNoTracking().ToListAsync();
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();

            // Lấy danh sách các giá trị RAM hiện có trong hệ thống để đưa vào dropdown
            ViewBag.AvailableRams = new List<string> { "8GB", "16GB", "32GB", "64GB" };

            // Giữ lại trạng thái lọc của người dùng
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentRam = ram;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.CurrentSortBy = sortBy;

            // Thông tin phân trang
            var routeValues = new Dictionary<string, string?>();
            if (!string.IsNullOrEmpty(searchTerm)) routeValues["searchTerm"] = searchTerm;
            if (brandId.HasValue) routeValues["brandId"] = brandId.ToString();
            if (categoryId.HasValue) routeValues["categoryId"] = categoryId.ToString();
            if (!string.IsNullOrEmpty(ram)) routeValues["ram"] = ram;
            if (!string.IsNullOrEmpty(priceRange)) routeValues["priceRange"] = priceRange;
            if (!string.IsNullOrEmpty(sortBy)) routeValues["sortBy"] = sortBy;

            ViewBag.Pagination = new PaginationInfo
            {
                PageIndex = paginatedProducts.PageIndex,
                TotalPages = paginatedProducts.TotalPages,
                TotalCount = paginatedProducts.TotalCount,
                Controller = "Products",
                Action = "Index",
                RouteValues = routeValues
            };

            return View(paginatedProducts);
        }

        // ==========================================
        // CHI TIẾT SẢN PHẨM (PRODUCT DETAILS)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy các sản phẩm liên quan (cùng hãng hoặc cùng danh mục)
            var relatedProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.Id != id && (p.BrandId == product.BrandId || p.CategoryId == product.CategoryId))
                .OrderByDescending(p => p.Id)
                .Take(4)
                .AsNoTracking()
                .ToListAsync();

            // Lấy các đánh giá của sản phẩm này (Phần của Hưng tích hợp)
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == id.Value)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            double averageRating = reviews.Any(r => r.Rating.HasValue) 
                ? reviews.Where(r => r.Rating.HasValue).Average(r => (double)r.Rating!.Value) 
                : 5.0;

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = Math.Round(averageRating, 1);
            ViewBag.ReviewCount = reviews.Count;

            return View(product);
        }

        // ==========================================
        // GỬI ĐÁNH GIÁ SẢN PHẨM (REVIEWS)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var userName = HttpContext.Session.GetString("UserName") ?? "Khách hàng";
            var userId = HttpContext.Session.GetString("UserId");

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                UserName = userName,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment?.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm!";
            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}