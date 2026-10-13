using LaptopShop.Data;
using LaptopShop.Models;
using LaptopShop.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. Đăng ký các dịch vụ (Dependency Injection)
// ================================================

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Đăng ký StockService cho module kho
builder.Services.AddScoped<IStockService, StockService>();

// Đăng ký Session với thời gian timeout 30 phút
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;       // Bảo mật: không cho JavaScript đọc cookie
    options.Cookie.IsEssential = true;    // Luôn lưu session
    options.Cookie.Name = ".LaptopShop.Session";
});

// Đăng ký HTTP Context Accessor (truy cập Session từ View/Service)
builder.Services.AddHttpContextAccessor();

// Đăng ký MVC với Controllers và Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ================================================
// 2. Tự động seed tài khoản Admin & Khách hàng mẫu
// ================================================
await DbSeeder.SeedAsync(app.Services);

// ================================================
// 3. Cấu hình middleware pipeline
// ================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt Session TRƯỚC Authentication/Authorization
app.UseSession();

app.UseAuthorization();

// ================================================
// 4. Cấu hình Route
// ================================================

// 1. Route cho khu vực Admin
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Route mặc định cho người dùng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
