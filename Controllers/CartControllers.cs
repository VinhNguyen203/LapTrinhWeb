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
            try
            {
                _cartService.AddItem(productId, quantity);

                int count = _cartService.GetCart()
                    .Sum(item => item.Quantity);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm sản phẩm vào giỏ.",
                    count = count
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            try
            {
                _cartService.RemoveItem(productId);

                return CartResult("Đã xóa sản phẩm khỏi giỏ.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(int productId)
        {
            try
            {
                _cartService.DecreaseItem(productId);

                return CartResult("Đã cập nhật số lượng.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
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