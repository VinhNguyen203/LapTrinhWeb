using System;
using System.Linq;
using System.Threading.Tasks;
using LaptopShop.Controllers;
using LaptopShop.Helpers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH ĐƠN HÀNG
        // ===========================
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, string? statusFilter = null)
        {
            var query = _context.Orders.AsNoTracking().AsQueryable();

            // Tìm kiếm theo mã đơn, họ tên, số điện thoại hoặc địa chỉ
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                if (int.TryParse(searchString.Trim().Replace("#", ""), out int orderId))
                {
                    query = query.Where(o => o.Id == orderId ||
                                             (o.CustomerName != null && o.CustomerName.ToLower().Contains(search)) ||
                                             (o.PhoneNumber != null && o.PhoneNumber.Contains(search)));
                }
                else
                {
                    query = query.Where(o => (o.CustomerName != null && o.CustomerName.ToLower().Contains(search)) ||
                                             (o.PhoneNumber != null && o.PhoneNumber.Contains(search)) ||
                                             (o.ShippingAddress != null && o.ShippingAddress.ToLower().Contains(search)));
                }
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            // Sắp xếp đơn mới nhất lên đầu
            query = query.OrderByDescending(o => o.OrderDate);

            var paginatedOrders = await PaginatedList<Order>.CreateAsync(query, pageNumber, PageSize);

            // Thống kê nhanh
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            ViewBag.TotalRevenue = await _context.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => o.TotalAmount ?? 0);

            ViewBag.CurrentSearch = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedOrders);
        }

        // ===========================
        // CHI TIẾT ĐƠN HÀNG
        // ===========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ===========================
        // CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{order.Id} sang '{newStatus}'.";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // XÓA ĐƠN HÀNG
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa đơn hàng #{id} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần xóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
