using LaptopShop.Models;
using LaptopShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly CheckoutService _checkoutService;
        private readonly AppDbContext _context;

        public CheckoutController(
            CartService cartService,
            CheckoutService checkoutService,
            AppDbContext context)
        {
            _cartService = cartService;
            _checkoutService = checkoutService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // 1. Bắt buộc đăng nhập trước khi thanh toán
            if (string.IsNullOrEmpty(userId))
            {
                TempData["CartError"] = "Vui lòng đăng nhập tài khoản Khách hàng để tiến hành thanh toán.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            // 2. Phân quyền: Chặn tài khoản Quản trị viên (Admin) đặt hàng
            if (userRole == "Admin")
            {
                TempData["CartError"] = "Tài khoản Quản trị viên (Admin) không được phép đặt hàng. Vui lòng đăng nhập tài khoản Khách hàng.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                _cartService.GetCheckoutItems();

                // Tự động điền thông tin sẵn có của khách hàng nếu có trong CSDL
                var model = new CheckoutViewModel();
                if (int.TryParse(userId, out int parsedUserId))
                {
                    var user = await _context.Users.FindAsync(parsedUserId);
                    if (user != null)
                    {
                        model.CustomerName = user.FullName;
                        model.PhoneNumber = user.PhoneNumber ?? string.Empty;
                        model.ShippingAddress = user.Address ?? string.Empty;
                    }
                }
                else
                {
                    model.CustomerName = HttpContext.Session.GetString("UserName") ?? string.Empty;
                }

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["CartError"] = ex.Message;

                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutViewModel input)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // 1. Bắt buộc đăng nhập
            if (string.IsNullOrEmpty(userId))
            {
                TempData["CartError"] = "Vui lòng đăng nhập tài khoản Khách hàng để thanh toán.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            // 2. Chặn Admin đặt hàng
            if (userRole == "Admin")
            {
                TempData["CartError"] = "Tài khoản Quản trị viên (Admin) không được phép đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                int orderId = _checkoutService.PlaceOrder(input, userId);

                _cartService.ClearCart();

                if (input.PaymentMethod == "QR")
                {
                    HttpContext.Session.SetInt32("QrOrderId", orderId);

                    return RedirectToAction(nameof(PaymentQr));
                }

                TempData["CreatedOrderId"] = orderId;

                return RedirectToAction(nameof(Success));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(input);
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            if (TempData["CreatedOrderId"] is not int orderId)
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(orderId);
        }

        [HttpGet]
        public IActionResult PaymentQr()
        {
            int? orderId = HttpContext.Session.GetInt32("QrOrderId");

            if (orderId == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = _context.Orders
                .AsNoTracking()
                .SingleOrDefault(order => order.Id == orderId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}