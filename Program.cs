using LaptopShop.Data;
using Microsoft.EntityFrameworkCore;
using LaptopShop.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();
// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cho phép service truy cập HttpContext của yêu cầu hiện tại
builder.Services.AddHttpContextAccessor();

// Đăng ký DbContext và kết nối SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CheckoutService>();
var app = builder.Build();

// Xử lý lỗi ngoài môi trường Development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();


app.UseAuthorization();

// Route cho khu vực Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route cho trang khách hàng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();