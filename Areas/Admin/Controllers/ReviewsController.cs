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
    [Area("Admin")]
    public class ReviewsController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH ĐÁNH GIÁ
        // ===========================
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, int? ratingFilter = null)
        {
            var query = _context.Reviews
                .Include(r => r.Product)
                .AsNoTracking()
                .AsQueryable();

            // Tìm kiếm theo tên khách, tên sản phẩm hoặc nội dung nhận xét
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(r => (r.UserName != null && r.UserName.ToLower().Contains(search)) ||
                                         (r.Comment != null && r.Comment.ToLower().Contains(search)) ||
                                         (r.Product != null && r.Product.Name.ToLower().Contains(search)));
            }

            // Lọc theo số sao
            if (ratingFilter.HasValue && ratingFilter > 0)
            {
                query = query.Where(r => r.Rating == ratingFilter.Value);
            }

            // Sắp xếp đánh giá mới nhất lên đầu
            query = query.OrderByDescending(r => r.CreatedAt);

            var paginatedReviews = await PaginatedList<Review>.CreateAsync(query, pageNumber, PageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.RatingFilter = ratingFilter;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedReviews);
        }

        // ===========================
        // CHI TIẾT ĐÁNH GIÁ
        // ===========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // ===========================
        // XÓA ĐÁNH GIÁ
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa đánh giá #{id} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đánh giá cần xóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
