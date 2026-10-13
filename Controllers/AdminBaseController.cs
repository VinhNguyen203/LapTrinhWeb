using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LaptopShop.Controllers
{
    /// <summary>
    /// Base controller trừu tượng dành cho các controller trong khu vực Admin.
    /// Kiểm tra quyền Admin trước mỗi action thông qua Session.
    /// </summary>
    public abstract class AdminBaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var userRole = HttpContext.Session.GetString("UserRole");

            // Nếu chưa đăng nhập hoặc không phải Admin -> chuyển hướng sang Login
            if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = RedirectToAction("Login", "Account", new { area = "", returnUrl });
                return;
            }
        }
    }
}
