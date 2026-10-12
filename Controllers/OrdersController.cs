using Microsoft.AspNetCore.Mvc;
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor: Yêu cầu hệ thống cấp cho DbContext để lấy dữ liệu
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Action Index: Lấy danh sách đơn hàng và gửi sang View
        public IActionResult Index()
        {
            // Lấy toàn bộ danh sách đơn hàng từ SQL Server, sắp xếp theo ngày mới nhất
            var orders = _context.Orders
                                 .OrderByDescending(o => o.OrderDate)
                                 .ToList();

            return View(orders);
        }
        // Action Details: Xem chi tiết một đơn hàng
        public IActionResult Details(int id)
        {
            // Tìm đơn hàng theo ID, bao gồm (Include) luôn cả chi tiết đơn hàng (OrderDetails) 
            // và thông tin sản phẩm (Product) bên trong chi tiết đó
            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Product)
                                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound(); // Nếu không tìm thấy trả về lỗi 404
            }

            return View(order);
        }
    }
}