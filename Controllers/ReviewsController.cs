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

        // Lấy danh sách tất cả đánh giá
        public IActionResult Index()
        {
            // Dùng Include để lấy luôn thông tin cái Laptop (Product) mà khách đang đánh giá
            var reviews = _context.Reviews
                                  .Include(r => r.Product)
                                  .OrderByDescending(r => r.CreatedAt)
                                  .ToList();

            return View(reviews);
        }
        // Action Delete: Nhận ID của đánh giá và tiến hành xóa khỏi CSDL
        [HttpPost] // Dùng HttpPost để bảo mật, tránh việc xóa nhầm khi gõ nhầm link
        public IActionResult Delete(int id)
        {
            // 1. Tìm cái đánh giá cần xóa trong cơ sở dữ liệu
            var review = _context.Reviews.FirstOrDefault(r => r.Id == id);

            if (review != null)
            {
                // 2. Xóa nó khỏi Entity Framework
                _context.Reviews.Remove(review);
                // 3. Lưu sự thay đổi xuống SQL Server
                _context.SaveChanges();
            }

            // 4. Xóa xong thì load lại trang danh sách đánh giá
            return RedirectToAction("Index");
        }
    }
}