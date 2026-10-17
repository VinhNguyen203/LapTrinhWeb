using Microsoft.AspNetCore.Mvc;
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? rating, string sortOrder)
        {
            // Truyền giá trị để giữ trạng thái trên View
            ViewData["CurrentRating"] = rating;

            // Thiết lập chiều sắp xếp cho từng cột
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["RatingSortParm"] = sortOrder == "rating_asc" ? "rating_desc" : "rating_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            var reviewsQuery = _context.Reviews
                                       .Include(r => r.Product)
                                       .AsQueryable();

            // 1. Lọc theo số sao (nếu có chọn)
            if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating == rating.Value);
            }

            // Đưa về List trước để sắp xếp tên theo chữ cái cuối cùng an toàn trên bộ nhớ C#
            var reviewsList = reviewsQuery.ToList();

            // 2. Sắp xếp đa cột
            switch (sortOrder)
            {
                case "id_desc":
                    reviewsList = reviewsList.OrderByDescending(r => r.Id).ToList();
                    break;
                case "name_asc":
                    reviewsList = reviewsList.OrderBy(r => !string.IsNullOrEmpty(r.UserName) ? r.UserName.Trim().Substring(r.UserName.Trim().LastIndexOf(' ') + 1) : "").ToList();
                    break;
                case "name_desc":
                    reviewsList = reviewsList.OrderByDescending(r => !string.IsNullOrEmpty(r.UserName) ? r.UserName.Trim().Substring(r.UserName.Trim().LastIndexOf(' ') + 1) : "").ToList();
                    break;
                case "rating_asc":
                    reviewsList = reviewsList.OrderBy(r => r.Rating).ToList();
                    break;
                case "rating_desc":
                    reviewsList = reviewsList.OrderByDescending(r => r.Rating).ToList();
                    break;
                case "date_asc":
                    reviewsList = reviewsList.OrderBy(r => r.CreatedAt).ToList();
                    break;
                case "date_desc":
                    reviewsList = reviewsList.OrderByDescending(r => r.CreatedAt).ToList();
                    break;
                default:
                    reviewsList = reviewsList.OrderByDescending(r => r.Id).ToList(); // Mặc định mới nhất lên đầu
                    break;
            }

            return View(reviewsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện thao tác.";
                return RedirectToAction("Login", "Account");
            }

            var review = _context.Reviews.FirstOrDefault(r => r.Id == id);
            if (review != null)
            {
                if (userRole == "Admin" || review.UserId == userId)
                {
                    _context.Reviews.Remove(review);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Đã xóa nhận xét thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xóa nhận xét của người khác.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}