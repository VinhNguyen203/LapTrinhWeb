using LaptopShop.Controllers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Tính tổng doanh thu từ các đơn hàng hoàn thành (An toàn tuyệt đối với giá trị null)
            var totalRevenue = await _context.Orders
                .Where(o => o.Status != null && o.Status.ToLower() == "completed")
                .SumAsync(o => (decimal?)(o.TotalAmount)) ?? 0;
            ViewBag.TotalRevenue = totalRevenue;

            // 2. Đếm số lượng đơn hàng đang chờ xử lý
            var pendingCount = await _context.Orders
                .Where(o => o.Status != null && o.Status.ToLower() == "pending")
                .CountAsync();
            ViewBag.PendingOrdersCount = pendingCount;

            // 3. Đếm tổng số sản phẩm laptop đang có trong hệ thống
            var activeProductsCount = await _context.Products.CountAsync();
            ViewBag.ActiveProductsCount = activeProductsCount;

            // 4. Đếm tổng số khách hàng đã mua hàng (dựa trên UserId hoặc khách hàngdistinct)
            var totalCustomers = await _context.Orders
                .Where(o => o.UserId != null)
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();
            ViewBag.TotalCustomers = totalCustomers;

            return View();
        }

        // API trả về dữ liệu JSON phục vụ vẽ biểu đồ Chart.js
        [HttpGet]
        public async Task<IActionResult> GetRevenueChartData()
        {
            try
            {
                // Lấy toàn bộ đơn hàng hoàn thành về RAM để nhóm ngày tháng an toàn tuyệt đối
                var orders = await _context.Orders
                    .Where(o => o.Status != null && o.Status.ToLower() == "completed")
                    .ToListAsync();

                var data = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new {
                        Date = g.Key.ToString("dd/MM/yyyy"),
                        Revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => DateTime.ParseExact(x.Date, "dd/MM/yyyy", null))
                    .Take(7)
                    .ToList();

                return Json(data);
            }
            catch (Exception)
            {
                // Trả về mảng rỗng nếu phát sinh sự cố dữ liệu để không làm sập giao diện web
                return Json(new List<object>());
            }
        }
    }
}