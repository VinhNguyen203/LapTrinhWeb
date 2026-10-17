using LaptopShop.Controllers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // 1. Thống kê số liệu an toàn (hỗ trợ cả tiếng Anh và tiếng Việt: Completed / Đã giao, Pending / Đang xử lý)
            var orders = await _context.Orders.ToListAsync();

            var monthlyRevenue = orders
                .Where(o => (o.Status == "Completed" || o.Status == "Đã giao") && o.OrderDate.HasValue && o.OrderDate.Value >= startOfMonth)
                .Sum(o => o.TotalAmount ?? 0);

            var totalAllTimeRevenue = orders
                .Where(o => o.Status == "Completed" || o.Status == "Đã giao")
                .Sum(o => o.TotalAmount ?? 0);

            var pendingOrdersCount = orders
                .Count(o => o.Status == "Pending" || o.Status == "Đang xử lý");

            // Số sản phẩm và số cảnh báo kho
            var activeProductsCount = await _context.Products.CountAsync();
            var lowStockCount = await _context.Products.CountAsync(p => p.StockQuantity < 5);

            var users = await _context.Users.ToListAsync();
            var newCustomersCount = users
                .Count(u => u.Role == "Customer" && u.CreatedAt >= startOfMonth);

            // Top 5 sản phẩm bán chạy nhất
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order != null && (od.Order.Status == "Completed" || od.Order.Status == "Đã giao") && od.Product != null)
                .ToListAsync();

            var topSellingProducts = orderDetails
                .GroupBy(od => new { od.ProductId, ProductName = od.Product!.Name })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity ?? 0),
                    TotalRevenue = g.Sum(x => (x.Quantity ?? 0) * (x.UnitPrice ?? 0))
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalAllTimeRevenue = totalAllTimeRevenue;
            ViewBag.PendingOrdersCount = pendingOrdersCount;
            ViewBag.ActiveProductsCount = activeProductsCount;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.NewCustomersCount = newCustomersCount;
            ViewBag.TopSellingProducts = topSellingProducts;

            return View();
        }

        // =========================================================================
        // AJAX ENDPOINTS CHO BIỂU ĐỒ CHART.JS
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string range = "7d")
        {
            var resultLabels = new List<string>();
            var resultValues = new List<decimal>();
            var today = DateTime.Today;
            var orders = await _context.Orders
                .Where(o => (o.Status == "Completed" || o.Status == "Đã giao") && o.OrderDate.HasValue)
                .ToListAsync();

            if (range == "12m")
            {
                for (int i = 11; i >= 0; i--)
                {
                    var targetMonth = today.AddMonths(-i);
                    var year = targetMonth.Year;
                    var month = targetMonth.Month;

                    resultLabels.Add($"Tháng {month}/{year}");

                    var monthlySum = orders
                        .Where(o => o.OrderDate!.Value.Year == year && o.OrderDate.Value.Month == month)
                        .Sum(o => o.TotalAmount ?? 0);

                    resultValues.Add(monthlySum);
                }
            }
            else
            {
                for (int i = 6; i >= 0; i--)
                {
                    var targetDate = today.AddDays(-i);
                    resultLabels.Add(targetDate.ToString("dd/MM"));

                    var dailySum = orders
                        .Where(o => o.OrderDate!.Value.Date == targetDate)
                        .Sum(o => o.TotalAmount ?? 0);

                    resultValues.Add(dailySum);
                }
            }

            return Json(new { labels = resultLabels, data = resultValues });
        }

        [HttpGet]
        public async Task<IActionResult> GetBrandShareData(string type = "revenue")
        {
            var brands = await _context.Brands.ToListAsync();
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order != null && (od.Order.Status == "Completed" || od.Order.Status == "Đã giao") && od.Product != null)
                .ToListAsync();

            var labels = new List<string>();
            var dataValues = new List<decimal>();

            foreach (var brand in brands)
            {
                labels.Add(brand.Name);
                var brandItems = orderDetails.Where(od => od.Product!.BrandId == brand.Id);

                if (type == "quantity")
                {
                    decimal totalQty = brandItems.Sum(od => (decimal)(od.Quantity ?? 0));
                    dataValues.Add(totalQty);
                }
                else
                {
                    decimal totalRev = brandItems.Sum(od => (decimal)((od.Quantity ?? 0) * (od.UnitPrice ?? 0)));
                    dataValues.Add(totalRev);
                }
            }

            return Json(new { labels, data = dataValues });
        }
    }
}