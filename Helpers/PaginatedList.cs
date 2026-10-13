using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaptopShop.Helpers
{
    /// <summary>
    /// Lớp hỗ trợ phân trang cho danh sách dữ liệu
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của phần tử trong danh sách</typeparam>
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// Chỉ số trang hiện tại (bắt đầu từ 1)
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Tổng số bản ghi
        /// </summary>
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            this.AddRange(items);
        }

        /// <summary>
        /// Có trang trước hay không
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Có trang kế tiếp hay không
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// Tạo PaginatedList bất đồng bộ từ IQueryable
        /// </summary>
        /// <param name="source">Nguồn dữ liệu IQueryable</param>
        /// <param name="pageIndex">Chỉ số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số phần tử mỗi trang</param>
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            // Đảm bảo pageIndex tối thiểu là 1
            if (pageIndex < 1) pageIndex = 1;

            var count = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
