using LaptopShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaptopShop.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartBadgeViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            int count = _cartService.GetCart()
                .Sum(item => item.Quantity);

            return View(count);
        }
    }
}