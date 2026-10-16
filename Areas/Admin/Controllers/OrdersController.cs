using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LaptopShop.Models;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Tắt tạm thời để tránh bị chuyển hướng về Trang chủ/Login khi test
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Khởi tạo Session tạm thời cho Sidebar Admin
        private void EnsureAdminSession()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                HttpContext.Session.SetString("UserName", "Admin Hưng");
            }
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string? status)
        {
            EnsureAdminSession();

            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            ViewBag.CurrentStatus = status;
            var orders = await query.ToListAsync();
            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            EnsureAdminSession();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Cập nhật trạng thái đơn hàng thành công!" });
            }

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/Orders/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int id)
        {
            EnsureAdminSession();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}