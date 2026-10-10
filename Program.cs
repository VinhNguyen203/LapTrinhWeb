var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ MVC vào container
builder.Services.AddControllersWithViews();

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

// 1. Cấu hình route cho khu vực Admin (Quản trị viên của nhóm)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Cấu hình route mặc định cho Khách hàng (Trang chủ, sản phẩm, v.v.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();