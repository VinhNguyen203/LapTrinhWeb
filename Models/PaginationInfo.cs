using System.Collections.Generic;

namespace LaptopShop.Models
{
    /// <summary>
    /// Thông tin phục vụ render component phân trang (_Pagination.cshtml)
    /// </summary>
    public class PaginationInfo
    {
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public string Action { get; set; } = "Index";
        public string? Controller { get; set; }
        public IDictionary<string, string?> RouteValues { get; set; } = new Dictionary<string, string?>();
    }
}
