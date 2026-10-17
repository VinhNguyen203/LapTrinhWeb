using ClosedXML.Excel;
using LaptopShop.Controllers;
using LaptopShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LaptopShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : AdminBaseController
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var now = DateTime.Today;
            var from = fromDate ?? new DateTime(now.Year, now.Month, 1);
            var to = toDate ?? now;

            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToString("yyyy-MM-dd");
            ViewBag.Status = status;

            var (orders, errorMsg) = await GetFilteredOrdersAsync(from, to, status);
            if (errorMsg != null)
            {
                TempData["Error"] = errorMsg;
            }

            // Thống kê nhanh theo kết quả lọc
            ViewBag.TotalOrders = orders.Count;
            ViewBag.CompletedOrders = orders.Count(o => o.Status == "Completed" || o.Status == "Đã giao");
            ViewBag.TotalRevenue = orders.Where(o => o.Status == "Completed" || o.Status == "Đã giao").Sum(o => o.TotalAmount);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var (orders, errorMsg) = await GetFilteredOrdersAsync(fromDate, toDate, status);
            if (errorMsg != null)
            {
                TempData["Error"] = errorMsg;
                return RedirectToAction(nameof(Index), new { fromDate = fromDate?.ToString("yyyy-MM-dd"), toDate = toDate?.ToString("yyyy-MM-dd"), status });
            }

            var sb = new StringBuilder();
            sb.Append('\ufeff'); // BOM cho UTF-8
            sb.AppendLine("MaDonHang,NgayDat,KhachHang,SoDienThoai,DiaChi,TongTien,TrangThai");

            foreach (var o in orders)
            {
                string line = $"{o.Id},{o.KeyDate:yyyy-MM-dd HH:mm},{EscapeCsv(o.CustomerName)},{EscapeCsv(o.PhoneNumber)},{EscapeCsv(o.ShippingAddress)},{o.TotalAmount},{o.Status}";
                sb.AppendLine(line);
            }

            byte[] buffer = new UTF8Encoding(true).GetBytes(sb.ToString());
            string fileName = $"BaoCao_DonHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(buffer, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var (orders, errorMsg) = await GetFilteredOrdersAsync(fromDate, toDate, status);
            if (errorMsg != null)
            {
                TempData["Error"] = errorMsg;
                return RedirectToAction(nameof(Index), new { fromDate = fromDate?.ToString("yyyy-MM-dd"), toDate = toDate?.ToString("yyyy-MM-dd"), status });
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo đơn hàng");

            worksheet.Cell("A1").Value = "BÁO CÁO DOANH THU VÀ ĐƠN HÀNG LAPTOPSHOP";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 16;
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.Navy;
            worksheet.Range("A1:G1").Merge();

            worksheet.Cell("A2").Value = $"Từ ngày: {(fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "Tất cả")} đến {(toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "Hôm nay")} | Trạng thái: {status}";
            worksheet.Cell("A2").Style.Font.Italic = true;
            worksheet.Range("A2:G2").Merge();

            int row = 4;
            worksheet.Cell(row, 1).Value = "Mã Đơn";
            worksheet.Cell(row, 2).Value = "Ngày Đặt";
            worksheet.Cell(row, 3).Value = "Khách Hàng";
            worksheet.Cell(row, 4).Value = "Số Điện Thoại";
            worksheet.Cell(row, 5).Value = "Địa Chỉ Giao Hàng";
            worksheet.Cell(row, 6).Value = "Tổng Tiền (VNĐ)";
            worksheet.Cell(row, 7).Value = "Trạng Thái";

            var headerRange = worksheet.Range(row, 1, row, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            foreach (var o in orders)
            {
                row++;
                worksheet.Cell(row, 1).Value = o.Id;
                worksheet.Cell(row, 2).Value = o.KeyDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 3).Value = o.CustomerName;
                worksheet.Cell(row, 4).Value = o.PhoneNumber;
                worksheet.Cell(row, 5).Value = o.ShippingAddress;
                worksheet.Cell(row, 6).Value = o.TotalAmount;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(row, 7).Value = o.Status;
            }

            row++;
            worksheet.Cell(row, 5).Value = "Tổng cộng doanh thu:";
            worksheet.Cell(row, 5).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var totalSum = orders.Where(o => o.Status == "Completed" || o.Status == "Đã giao").Sum(o => o.TotalAmount);
            worksheet.Cell(row, 6).Value = totalSum;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(row, 6).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string fileName = $"BaoCao_DoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<(List<OrderExportModel> Orders, string? ErrorMessage)> GetFilteredOrdersAsync(DateTime? fromDate, DateTime? toDate, string status)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return (new List<OrderExportModel>(), "Lỗi: Ngày bắt đầu không được lớn hơn ngày kết thúc!");
            }

            var query = _context.Orders.AsQueryable();

            if (fromDate.HasValue)
            {
                var fromDateStart = fromDate.Value.Date;
                query = query.Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= fromDateStart);
            }

            if (toDate.HasValue)
            {
                var toDateNextDay = toDate.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate.HasValue && o.OrderDate.Value < toDateNextDay);
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                {
                    query = query.Where(o => o.Status == "Completed" || o.Status == "Đã giao");
                }
                else if (status == "Pending")
                {
                    query = query.Where(o => o.Status == "Pending" || o.Status == "Đang xử lý");
                }
                else if (status == "Cancelled")
                {
                    query = query.Where(o => o.Status == "Cancelled" || o.Status == "Đã hủy");
                }
                else if (status == "Processing")
                {
                    query = query.Where(o => o.Status == "Processing" || o.Status == "Đang giao");
                }
                else
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            var data = await query
                .OrderByDescending(o => o.Id)
                .Select(o => new OrderExportModel
                {
                    Id = o.Id,
                    KeyDate = o.OrderDate ?? DateTime.Now,
                    CustomerName = o.CustomerName ?? "Khách vãng lai",
                    PhoneNumber = o.PhoneNumber ?? "",
                    ShippingAddress = o.ShippingAddress ?? "",
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = o.Status ?? "Pending"
                })
                .ToListAsync();

            return (data, null);
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }
    }

    public class OrderExportModel
    {
        public int Id { get; set; }
        public DateTime KeyDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}