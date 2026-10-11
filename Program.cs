using Microsoft.EntityFrameworkCore;
using LaptopShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ MVC vào container
builder.Services.AddControllersWithViews();

// ===== THÊM ĐOẠN NÀY ĐỂ KẾT NỐI DATABASE =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// ==============================================

var app = builder.Build();

// Cấu hình môi trường khi gặp lỗi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 1. Cấu hình route cho khu vực Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Cấu hình route mặc định cho Khách hàng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();