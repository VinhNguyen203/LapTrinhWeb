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
    public class CouponsController : AdminBaseController
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public CouponsController(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // DANH SÁCH MÃ GIẢM GIÁ
        // ===========================
        public async Task<IActionResult> Index(int pageNumber = 1, string? searchString = null, string? statusFilter = null)
        {
            var query = _context.Coupons.AsNoTracking().AsQueryable();

            // Tìm kiếm theo Code hoặc Description
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                query = query.Where(c => c.Code.ToLower().Contains(search) || 
                                         (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Active")
                {
                    query = query.Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now);
                }
                else if (statusFilter == "Inactive")
                {
                    query = query.Where(c => !c.IsActive || c.ExpiryDate < DateTime.Now);
                }
            }

            // Sắp xếp mới nhất lên đầu
            query = query.OrderByDescending(c => c.CreatedAt);

            var paginatedCoupons = await PaginatedList<Coupon>.CreateAsync(query, pageNumber, PageSize);

            ViewBag.CurrentSearch = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = pageNumber;

            return View(paginatedCoupons);
        }

        // ===========================
        // TẠO MÃ MỚI (GET & POST)
        // ===========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Coupon
            {
                Code = "VOUCHER" + new Random().Next(100, 999),
                ExpiryDate = DateTime.Now.AddMonths(1),
                DiscountValue = 100000,
                MinOrderAmount = 5000000,
                MaxUsage = 100,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại, vui lòng chọn mã khác.");
            }

            if (ModelState.IsValid)
            {
                coupon.Code = coupon.Code.Trim().ToUpper();
                coupon.CreatedAt = DateTime.Now;
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã tạo mã giảm giá '{coupon.Code}' thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        // ===========================
        // CHỈNH SỬA MÃ (GET & POST)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id) return NotFound();

            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code && c.Id != id))
            {
                ModelState.AddModelError("Code", "Mã giảm giá này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Coupons.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Code = coupon.Code.Trim().ToUpper();
                    existing.Description = coupon.Description;
                    existing.DiscountType = coupon.DiscountType;
                    existing.DiscountValue = coupon.DiscountValue;
                    existing.MinOrderAmount = coupon.MinOrderAmount;
                    existing.ExpiryDate = coupon.ExpiryDate;
                    existing.MaxUsage = coupon.MaxUsage;
                    existing.IsActive = coupon.IsActive;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật mã giảm giá '{existing.Code}' thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Coupons.AnyAsync(c => c.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(coupon);
        }

        // ===========================
        // BẬT / TẮT TRẠNG THÁI
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã giảm giá.";
                return RedirectToAction(nameof(Index));
            }

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã {(coupon.IsActive ? "kích hoạt" : "vô hiệu hóa")} mã giảm giá '{coupon.Code}'.";
            return RedirectToAction(nameof(Index));
        }

        // ===========================
        // XÓA MÃ
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa mã giảm giá '{coupon.Code}' thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã cần xóa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
