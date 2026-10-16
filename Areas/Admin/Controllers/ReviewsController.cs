using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LaptopShop.Models;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? rating, string sortOrder)
        {
            ViewData["CurrentRating"] = rating;
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["RatingSortParm"] = sortOrder == "rating_asc" ? "rating_desc" : "rating_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            var reviewsQuery = _context.Reviews
                                       .Include(r => r.Product)
                                       .AsQueryable();

            if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
            {
                reviewsQuery = reviewsQuery.Where(r => r.Rating == rating.Value);
            }

            var reviewsList = await reviewsQuery.ToListAsync();

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
                    reviewsList = reviewsList.OrderByDescending(r => r.Id).ToList();
                    break;
            }

            return View(reviewsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đánh giá!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}