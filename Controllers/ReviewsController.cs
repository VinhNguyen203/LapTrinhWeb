using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LaptopShop.Models;

namespace LaptopShop.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được đánh giá
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Reviews/Create (Nhận form đánh giá 1-5 sao từ trang chi tiết sản phẩm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Vui lòng chọn mức đánh giá từ 1 đến 5 sao!";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "Khách hàng";

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                UserName = userName,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm!";
            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}