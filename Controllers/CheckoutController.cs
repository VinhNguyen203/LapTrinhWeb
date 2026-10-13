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
        public IActionResult Index()
        {
            try
            {
                _cartService.GetCheckoutItems();

                return View(new CheckoutViewModel());
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
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            try
            {
                int orderId = _checkoutService.PlaceOrder(input);

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