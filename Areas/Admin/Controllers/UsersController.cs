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
    /// Controller Admin quản lý người dùng hệ thống.
    /// Kế thừa AdminBaseController để bảo vệ bằng kiểm tra Session Role == "Admin".
    /// </summary>
    [Area("Admin")]
    public class UsersController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH NGƯỜI DÙNG
        // ===========================

        /// <summary>
        /// GET: Danh sách người dùng có phân trang, tìm kiếm và lọc theo vai trò
        /// </summary>
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, string? roleFilter = null)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            // Tìm kiếm theo tên, email hoặc số điện thoại
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(search) || 
                                         u.Email.ToLower().Contains(search) ||
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            // Lọc theo vai trò nếu có
            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            // Sắp xếp người dùng mới nhất lên đầu
            query = query.OrderByDescending(u => u.CreatedAt);

            var paginatedUsers = await PaginatedList<AppUser>.CreateAsync(query, pageNumber, PageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedUsers);
        }

        // ===========================
        // CHI TIẾT NGƯỜI DÙNG
        // ===========================

        /// <summary>
        /// GET: Xem chi tiết thông tin người dùng
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Lấy danh sách đơn hàng liên quan của user (nếu có)
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Email || o.UserId == user.Id.ToString())
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = orders;
            return View(user);
        }

        // ===========================
        // SỬA NGƯỜI DÙNG
        // ===========================

        /// <summary>
        /// GET: Hiển thị form sửa thông tin người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        /// <summary>
        /// POST: Lưu thay đổi thông tin người dùng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,Address,Role,IsActive")] AppUser model)
        {
            if (id != model.Id)
                return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và tên không được để trống.");
                return View(user);
            }

            if (model.Role != "Admin" && model.Role != "Customer")
            {
                ModelState.AddModelError("Role", "Vai trò không hợp lệ.");
                return View(user);
            }

            // Bảo vệ: Không cho phép Admin tự hạ quyền của chính mình nếu đang đăng nhập
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString() && model.Role != "Admin")
            {
                ModelState.AddModelError("Role", "Bạn không thể tự hạ quyền Admin của chính mình.");
                return View(user);
            }

            try
            {
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Role = model.Role;
                user.IsActive = model.IsActive;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật thông tin tài khoản [{user.FullName}] thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == id))
                    return NotFound();
                throw;
            }
        }

        // ===========================
        // KHÓA / MỞ KHÓA TÀI KHOẢN
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                TempData["ErrorMessage"] = "Bạn không thể tự khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = user.IsActive 
                ? $"Đã kích hoạt lại tài khoản [{user.FullName}]." 
                : $"Đã khóa tài khoản [{user.FullName}].";

            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // XÓA NGƯỜI DÙNG
        // ===========================

        /// <summary>
        /// GET: Trang xác nhận xóa người dùng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa tài khoản của chính mình khi đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        /// <summary>
        /// POST: Thực hiện xóa người dùng
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                TempData["ErrorMessage"] = "Bạn không thể xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa người dùng [{user.FullName}] vĩnh viễn.";
            return RedirectToAction(nameof(Index));
        }
    }
}
