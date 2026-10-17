using Microsoft.AspNetCore.Mvc;
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, string sortOrder, string status)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            // Bắt buộc đăng nhập để xem đơn hàng
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng của bạn.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders" });
            }

            // Nếu là Admin, chuyển hướng đến trang Quản lý đơn hàng của Admin
            if (currentUserRole == "Admin")
            {
                return RedirectToAction("Index", "Orders", new { area = "Admin" });
            }

            // Truyền tham số sắp xếp sang View để đổi chiều mũi tên
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            // Xác định chiều sắp xếp cho từng cột
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["PriceSortParm"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            // Chỉ lấy danh sách đơn hàng của khách hàng hiện tại
            var ordersQuery = _context.Orders
                .Where(o => o.UserId == currentUserId)
                .AsQueryable();

            // 1. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // 2. Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int orderId))
                {
                    ordersQuery = ordersQuery.Where(o => o.Id == orderId || (o.CustomerName != null && o.CustomerName.Contains(searchString)));
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => o.CustomerName != null && o.CustomerName.Contains(searchString));
                }
            }

            // 3. Sắp xếp đa cột linh hoạt
            var ordersList = ordersQuery.ToList();

            string GetLastName(Order o)
            {
                var name = (o.CustomerName ?? string.Empty).Trim();
                int lastIdx = name.LastIndexOf(' ');
                return lastIdx >= 0 ? name.Substring(lastIdx + 1) : name;
            }

            switch (sortOrder)
            {
                case "id_desc":
                    ordersList = ordersList.OrderByDescending(o => o.Id).ToList();
                    break;
                case "name_asc":
                    ordersList = ordersList.OrderBy(GetLastName).ToList();
                    break;
                case "name_desc":
                    ordersList = ordersList.OrderByDescending(GetLastName).ToList();
                    break;
                case "date_asc":
                    ordersList = ordersList.OrderBy(o => o.OrderDate).ToList();
                    break;
                case "date_desc":
                    ordersList = ordersList.OrderByDescending(o => o.OrderDate).ToList();
                    break;
                case "price_asc":
                    ordersList = ordersList.OrderBy(o => o.TotalAmount).ToList();
                    break;
                case "price_desc":
                    ordersList = ordersList.OrderByDescending(o => o.TotalAmount).ToList();
                    break;
                default:
                    ordersList = ordersList.OrderByDescending(o => o.Id).ToList();
                    break;
            }

            return View(ordersList);
        }

        public IActionResult Details(int id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            var currentUserRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Product)
                                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            // Khách chỉ được xem đơn hàng của chính mình (Admin được xem mọi đơn)
            if (currentUserRole != "Admin" && order.UserId != currentUserId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem chi tiết đơn hàng của người khác.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }
    }
}