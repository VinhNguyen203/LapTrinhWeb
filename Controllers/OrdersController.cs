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
            // Truyền tham số sắp xếp sang View để đổi chiều mũi tên
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;

            // Xác định chiều sắp xếp cho từng cột
            ViewData["IdSortParm"] = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewData["PriceSortParm"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            var ordersQuery = _context.Orders.AsQueryable();

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
                    ordersQuery = ordersQuery.Where(o => o.Id == orderId || o.CustomerName.Contains(searchString));
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => o.CustomerName.Contains(searchString));
                }
            }

            // 3. Sắp xếp đa cột linh hoạt (Đưa về dạng List trước để xử lý chuỗi C# an toàn)
            var ordersList = ordersQuery.ToList();

            switch (sortOrder)
            {
                case "id_desc":
                    ordersList = ordersList.OrderByDescending(o => o.Id).ToList();
                    break;
                case "name_asc":
                    // Sắp xếp theo tên cuối cùng (tăng dần A-Z) trên bộ nhớ C#
                    ordersList = ordersList.OrderBy(o => o.CustomerName.Trim().Substring(o.CustomerName.Trim().LastIndexOf(' ') + 1)).ToList();
                    break;
                case "name_desc":
                    // Sắp xếp theo tên cuối cùng (giảm dần Z-A) trên bộ nhớ C#
                    ordersList = ordersList.OrderByDescending(o => o.CustomerName.Trim().Substring(o.CustomerName.Trim().LastIndexOf(' ') + 1)).ToList();
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
                    ordersList = ordersList.OrderBy(o => o.Id).ToList();
                    break;
            }

            return View(ordersList);
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Product)
                                .FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}