using LaptopShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaptopShop.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = _cartService.GetCartDetails();

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tài khoản Quản trị viên (Admin) không thực hiện mua hàng. Vui lòng dùng tài khoản Khách hàng."
                    });
                }

                TempData["CartError"] = "Tài khoản Quản trị viên không thể mua hàng. Vui lòng sử dụng tài khoản Khách hàng.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                _cartService.AddItem(productId, quantity);

                TempData["CartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    int count = _cartService.GetCart()
                        .Sum(item => item.Quantity);

                    return Json(new
                    {
                        success = true,
                        message = "Đã thêm sản phẩm vào giỏ hàng thành công!",
                        count = count
                    });
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                TempData["CartError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            try
            {
                _cartService.RemoveItem(productId);
                TempData["CartSuccess"] = "Đã xóa sản phẩm khỏi giỏ hàng.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return CartResult("Đã xóa sản phẩm khỏi giỏ hàng.");
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                TempData["CartError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(int productId)
        {
            try
            {
                _cartService.DecreaseItem(productId);
                TempData["CartSuccess"] = "Đã giảm số lượng sản phẩm.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return CartResult("Đã cập nhật số lượng.");
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                TempData["CartError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            try
            {
                _cartService.UpdateItem(productId, quantity);
                TempData["CartSuccess"] = "Đã cập nhật số lượng sản phẩm.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return CartResult("Đã cập nhật số lượng.");
                }

                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                TempData["CartError"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        private IActionResult CartResult(string message)
        {
            var items = _cartService.GetCartDetails();

            return Json(new
            {
                success = true,
                message = message,
                count = items.Sum(item => item.Quantity),
                total = items.Sum(item => item.TotalPrice),
                items = items
            });
        }
    }
}