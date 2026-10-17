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
                var checkoutItems = _cartService.GetCheckoutItems();
                ViewBag.CartItems = checkoutItems;
                ViewBag.Subtotal = checkoutItems.Sum(item => item.TotalPrice);

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

            var items = _cartService.GetCartDetails();
            ViewBag.CartItems = items;
            ViewBag.Subtotal = items.Sum(item => item.TotalPrice);

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

        // =========================================================================
        // AJAX: KIỂM TRA MÃ GIẢM GIÁ (COUPON)
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> CheckCoupon([FromForm] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá!" });
            }

            var cleanCode = code.Trim().ToUpper();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == cleanCode);

            if (coupon == null || !coupon.IsActive)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã bị vô hiệu hóa!" });
            }

            if (coupon.ExpiryDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá này đã hết hạn sử dụng!" });
            }

            if (coupon.UsageCount >= coupon.MaxUsage)
            {
                return Json(new { success = false, message = "Mã giảm giá này đã hết lượt sử dụng!" });
            }

            var items = _cartService.GetCartDetails();
            decimal subtotal = items.Sum(i => i.TotalPrice);

            if (subtotal < coupon.MinOrderAmount)
            {
                return Json(new
                {
                    success = false,
                    message = $"Mã này chỉ áp dụng cho đơn hàng từ {coupon.MinOrderAmount:N0} đ trở lên (Hiện tại: {subtotal:N0} đ)!"
                });
            }

            decimal discount = 0;
            if (coupon.DiscountType == "Percentage")
            {
                discount = subtotal * (coupon.DiscountValue / 100m);
            }
            else
            {
                discount = coupon.DiscountValue;
            }

            discount = Math.Min(discount, subtotal);
            decimal finalTotal = Math.Max(0, subtotal - discount);

            return Json(new
            {
                success = true,
                message = $"Áp dụng mã '{coupon.Code}' thành công! {coupon.Description}",
                discountAmount = discount,
                finalTotal = finalTotal,
                code = coupon.Code
            });
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