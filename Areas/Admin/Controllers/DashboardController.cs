// Areas/Admin/Controllers/DashboardController.cs
// Phần của Huân: Dashboard
using Microsoft.AspNetCore.Mvc;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")] // Rất quan trọng để định tuyến vào đúng thư mục Admin
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}