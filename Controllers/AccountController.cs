using System;
using System.Threading.Tasks;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace LaptopShop.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng xác thực người dùng:
    /// Đăng nhập, Đăng ký, Đăng xuất, Hồ sơ, Đổi mật khẩu
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // ĐĂNG NHẬP
        // ===========================

        /// <summary>
        /// GET: Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form.ContainsKey("isAjax");
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }
                return View(model);
            }

            // Tìm người dùng theo email (không phân biệt hoa thường)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                if (isAjax)
                    return Json(new { success = false, message = "Email hoặc mật khẩu không chính xác." });

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (!user.IsActive)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ quản trị viên." });

                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (isAjax)
            {
                string targetUrl = user.Role == "Admin"
                    ? "/Admin/Dashboard"
                    : (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Account/Login") ? returnUrl : "/");

                return Json(new { success = true, redirectUrl = targetUrl, role = user.Role });
            }

            // Điều hướng: Nếu tài khoản là Admin, tự động chuyển hướng ngay vào Dashboard Quản trị
            if (user.Role == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Account/Login"))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ===========================
        // ĐĂNG KÝ
        // ===========================

        /// <summary>
        /// GET: Hiển thị trang đăng ký
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// POST: Xử lý đăng ký tài khoản mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Form.ContainsKey("isAjax");

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }
                return View(model);
            }

            var existingUser = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (existingUser)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Địa chỉ Email này đã được đăng ký trong hệ thống." });

                ModelState.AddModelError("Email", "Địa chỉ Email này đã được đăng ký trong hệ thống.");
                return View(model);
            }

            var newUser = new AppUser
            {
                FullName = model.FullName,
                Email = model.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Tự động đăng nhập
            HttpContext.Session.SetString("UserId", newUser.Id.ToString());
            HttpContext.Session.SetString("UserName", newUser.FullName);
            HttpContext.Session.SetString("UserEmail", newUser.Email);
            HttpContext.Session.SetString("UserRole", newUser.Role);

            if (isAjax)
            {
                return Json(new { success = true, redirectUrl = "/" });
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với LaptopShop.";
            return RedirectToAction("Index", "Home");
        }

        // ===========================
        // ĐĂNG XUẤT
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất tài khoản thành công.";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public IActionResult LogoutGet()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất tài khoản thành công.";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // ===========================
        // HỒ SƠ NGƯỜI DÙNG
        // ===========================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            _context.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", user.FullName);
            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Profile");
        }

        // ===========================
        // ĐỔI MẬT KHẨU
        // ===========================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}
