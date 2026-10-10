// Controllers/HomeController.cs
// Phần của Khoa / Client chung
using Microsoft.AspNetCore.Mvc;

namespace LaptopShop.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}